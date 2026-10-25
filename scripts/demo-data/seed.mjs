#!/usr/bin/env node
// Loads (or removes) the demo data of SumiClas through the real HTTP API.
// Local development only: it refuses to talk to any host that is not local.
import { spawnSync } from 'node:child_process';
import {
  ALL_CHILDREN,
  CLASSES,
  CONTENT,
  FAMILIES,
  FEATURED,
  PASSWORD,
  PEOPLE,
  SCHOOLS,
  SCHOOL_YEAR,
  addDays,
  allDemoIds,
  ids,
} from './dataset.mjs';

const HELP = `Uso: node scripts/demo-data/seed.mjs [load|reset|verify] [opciones]

  load     Crea los datos de demostración (por defecto). Se puede repetir: lo que ya existe se salta.
  reset    Borra solo lo creado por la demo (por sus ids) en la base de datos.
  verify   Entra con las cuentas destacadas y muestra lo que ve cada una.

Opciones:
  --day=AAAA-MM-DD       Día de referencia de las fechas (por defecto, hoy en UTC).
  --api=URL              API local (por defecto https://localhost:7064).
  --db-container=NOMBRE  Contenedor de PostgreSQL para reset (por defecto sumiclas-postgres).
  --db-user=USUARIO      Usuario de la base (por defecto sumiclas).
  --db-name=BASE         Nombre de la base (por defecto sumiclas).
  --dry-run              Con reset, muestra lo que borraría sin borrar nada.
`;

const MS_PER_SECOND = 1_000;
const TOKEN_MAX_AGE_MS = 10 * 60 * MS_PER_SECOND;
const AUTH_PAUSE_MS = 120;
const DEFAULT_RETRY_AFTER_SECONDS = 10;
const MAX_RATE_LIMIT_RETRIES = 8;
const DEMO_EMAIL_PATTERN = '%@demo.sumiclas.test';

function parseArguments(argv) {
  const options = { command: 'load', flags: {} };
  for (const argument of argv) {
    if (argument === '--help' || argument === '-h') options.flags.help = true;
    else if (argument.startsWith('--')) {
      const [name, value] = argument.slice(2).split('=');
      options.flags[name] = value ?? true;
    } else options.command = argument;
  }
  return options;
}

const { command, flags } = parseArguments(process.argv.slice(2));
if (flags.help) {
  console.log(HELP);
  process.exit(0);
}

const API = String(flags.api ?? 'https://localhost:7064').replace(/\/$/, '');
const DAY = String(flags.day ?? new Date().toISOString().slice(0, 10));
const DB = {
  container: String(flags['db-container'] ?? 'sumiclas-postgres'),
  user: String(flags['db-user'] ?? 'sumiclas'),
  name: String(flags['db-name'] ?? 'sumiclas'),
};

if (!/^\d{4}-\d{2}-\d{2}$/.test(DAY) || Number.isNaN(Date.parse(`${DAY}T00:00:00Z`))) {
  console.error(`--day tiene que ser una fecha AAAA-MM-DD, y es «${DAY}».`);
  process.exit(1);
}

const apiHost = new URL(API).hostname;
if (!['localhost', '127.0.0.1', '[::1]', '::1'].includes(apiHost)) {
  console.error(`Este script solo habla con una API local, y «${apiHost}» no lo es.`);
  process.exit(1);
}
process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

const stats = { created: {}, skipped: {} };
const tokens = new Map();
const sleep = (ms) => new Promise((resolve) => setTimeout(resolve, ms));

function count(kind, bucket) {
  stats[bucket][kind] = (stats[bucket][kind] ?? 0) + 1;
}

async function send(method, path, { as, body } = {}) {
  for (let attempt = 0; attempt <= MAX_RATE_LIMIT_RETRIES; attempt++) {
    const headers = { 'Content-Type': 'application/json' };
    if (as) headers.Authorization = `Bearer ${await tokenOf(as)}`;
    const response = await fetch(`${API}${path}`, {
      method,
      headers,
      body: body === undefined ? undefined : JSON.stringify(body),
    });
    if (response.status === 429) {
      const waitSeconds = Number(response.headers.get('Retry-After')) || DEFAULT_RETRY_AFTER_SECONDS;
      console.log(`  límite de peticiones: espero ${waitSeconds} s`);
      await sleep(waitSeconds * MS_PER_SECOND);
      continue;
    }
    if (response.status === 401 && as && attempt === 0) {
      tokens.delete(as);
      continue;
    }
    const text = await response.text();
    let data = null;
    try {
      data = text ? JSON.parse(text) : null;
    } catch {
      data = text;
    }
    return { status: response.status, data };
  }
  throw new Error(`Demasiados rechazos por límite de peticiones en ${method} ${path}`);
}

function describe(result) {
  const problem = result.data;
  return problem && typeof problem === 'object' ? `${problem.code ?? problem.title} - ${problem.detail ?? ''}` : String(problem);
}

async function login(userKey) {
  const person = PEOPLE[userKey];
  const result = await send('POST', '/auth/login', { body: { email: person.email, password: PASSWORD } });
  if (result.status !== 200) throw new Error(`No se pudo entrar como ${person.email}: ${result.status} ${describe(result)}`);
  tokens.set(userKey, { token: result.data.accessToken, at: Date.now() });
  await sleep(AUTH_PAUSE_MS);
  return result.data.accessToken;
}

async function tokenOf(userKey) {
  const cached = tokens.get(userKey);
  if (cached && Date.now() - cached.at < TOKEN_MAX_AGE_MS) return cached.token;
  return login(userKey);
}

// Creates one thing. A 409 means it already exists (same deterministic id), so it is skipped.
async function step(kind, method, path, as, body) {
  const result = await send(method, path, { as, body });
  if (result.status >= 200 && result.status < 300) {
    count(kind, 'created');
    return result;
  }
  if (result.status === 409) {
    count(kind, 'skipped');
    return result;
  }
  throw new Error(`${kind}: ${method} ${path} como ${as ?? 'anónimo'} -> ${result.status} ${describe(result)}`);
}

function monthAndDay(day) {
  const date = new Date(`${day}T00:00:00Z`);
  return { month: date.getUTCMonth() + 1, day: date.getUTCDate() };
}

async function loadAll() {
  console.log(`Cargando la demo en ${API} con día de referencia ${DAY}`);

  for (const person of Object.values(PEOPLE)) {
    const result = await send('POST', '/auth/register', {
      body: { id: person.id, fullName: person.fullName, email: person.email, password: PASSWORD },
    });
    if (result.status === 201 || result.status === 200) count('cuentas', 'created');
    else if (result.status === 409) count('cuentas', 'skipped');
    else throw new Error(`Registro de ${person.email}: ${result.status} ${describe(result)}`);
    await sleep(AUTH_PAUSE_MS);
  }
  for (const key of Object.keys(PEOPLE)) await login(key);

  for (const [schoolKey, school] of Object.entries(SCHOOLS)) {
    const probe = await send('GET', `/schools/${school.id}`, { as: 'lucia' });
    if (probe.status !== 200) {
      throw new Error(`El colegio ${school.label} (${school.id}) no está en el catálogo de esta base de datos.`);
    }
    const creator = Object.values(CLASSES).find((definition) => definition.school === schoolKey).delegate;
    await step('cursos escolares', 'POST', '/school-years', creator, {
      id: ids.year(schoolKey),
      schoolId: school.id,
      label: SCHOOL_YEAR.label,
      startDate: SCHOOL_YEAR.startDate,
      endDate: SCHOOL_YEAR.endDate,
    });
  }

  for (const [classKey, definition] of Object.entries(CLASSES)) {
    await step('clases', 'POST', '/classes', definition.delegate, {
      id: ids.class(classKey),
      schoolYearId: ids.year(definition.school),
      name: definition.name,
    });
  }

  for (const child of ALL_CHILDREN) {
    await step('hijos', 'POST', '/students', child.guardian, {
      id: ids.student(child.key),
      firstName: child.first,
      firstSurname: child.s1,
      secondSurname: child.s2,
      firstGuardianRole: child.role,
    });
    await step('matrículas', 'POST', '/enrollments', child.guardian, {
      id: ids.enrollment(child.key),
      studentId: ids.student(child.key),
      classId: ids.class(child.class),
    });
  }

  for (const [classKey, definition] of Object.entries(CLASSES)) {
    const content = CONTENT[classKey];
    const classId = ids.class(classKey);
    const delegate = definition.delegate;
    console.log(`Clase ${definition.name} (${SCHOOLS[definition.school].label})`);

    const subjectIds = {};
    for (const label of definition.subjects) {
      subjectIds[label] = ids.subject(classKey, label);
      await step('asignaturas', 'POST', '/subjects', delegate, { id: subjectIds[label], classId, label });
    }

    for (const event of content.events) {
      await step('eventos', 'POST', '/calendar-events', delegate, {
        id: ids.event(classKey, event.key),
        scope: 'Class',
        classId,
        type: event.type,
        title: event.title,
        date: addDays(DAY, event.offset),
        time: event.time,
        location: event.location ?? null,
        cost: event.cost ?? null,
      });
    }
    for (const birthday of content.birthdays) {
      const child = ALL_CHILDREN.find((candidate) => candidate.key === birthday.child);
      const { month, day } = monthAndDay(addDays(DAY, birthday.offset));
      await step('cumpleaños', 'POST', '/calendar-events/birthdays', birthday.by, {
        id: ids.birthday(classKey, birthday.key),
        studentId: ids.student(child.key),
        title: `Cumpleaños de ${child.first}`,
        month,
        day,
      });
    }

    for (const announcement of content.announcements) {
      await step('avisos', 'POST', '/announcements', delegate, {
        id: ids.announcement(classKey, announcement.key),
        scope: 'Class',
        classId,
        title: announcement.title,
        body: announcement.body,
        priority: announcement.priority,
        relatedEventId: announcement.event ? ids.event(classKey, announcement.event) : null,
      });
    }
    for (const [announcementKey, readers] of Object.entries(content.reads)) {
      for (const reader of readers) {
        await step('avisos leídos', 'PUT', `/announcements/${ids.announcement(classKey, announcementKey)}/read`, reader);
      }
    }

    for (const item of content.lostItems) {
      const itemId = ids.lostItem(classKey, item.key);
      await step('objetos perdidos', 'POST', '/lost-items', item.by, {
        id: itemId,
        scope: 'Class',
        classId,
        status: item.status,
        description: item.description,
        photoUrl: null,
        photoPrivacyConfirmed: false,
      });
      if (item.recovered) await step('objetos recuperados', 'PUT', `/lost-items/${itemId}/recovered`, item.by);
    }

    for (const assignment of content.assignments) {
      await step('deberes', 'POST', '/assignments', assignment.by, {
        id: ids.assignment(classKey, assignment.key),
        classId,
        subjectId: subjectIds[assignment.subject],
        title: assignment.title,
        description: assignment.description ?? null,
        dueDate: addDays(DAY, assignment.offset),
      });
    }
    for (const comment of content.comments) {
      const assignmentId = ids.assignment(classKey, comment.assignment);
      const known = await send('GET', `/assignments?classId=${classId}&limit=50`, { as: comment.by });
      const alreadyThere = known.status === 200 && JSON.stringify(known.data).includes(comment.text);
      if (alreadyThere) count('comentarios', 'skipped');
      else await step('comentarios', 'POST', `/assignments/${assignmentId}/comments`, comment.by, { text: comment.text });
    }
    for (const [assignmentKey, users] of Object.entries(content.done)) {
      for (const user of users) {
        await step('deberes hechos', 'PUT', `/assignments/${ids.assignment(classKey, assignmentKey)}/done`, user);
      }
    }

    for (const expense of content.expenses) {
      const expenseId = ids.expense(classKey, expense.key);
      await step('gastos', 'POST', '/class-expenses', delegate, {
        id: expenseId,
        classId,
        item: expense.item,
        totalAmount: expense.amount,
        participantIds: expense.participants.map(ids.user),
        relatedEventId: expense.event ? ids.event(classKey, expense.event) : null,
      });
      for (const payer of expense.paid) {
        await step('pagos', 'PUT', `/class-expenses/${expenseId}/participants/${ids.user(payer)}/paid`, payer);
      }
    }
  }

  printSummary();
}

function printSummary() {
  console.log('\nResumen (creado / ya existía):');
  const kinds = new Set([...Object.keys(stats.created), ...Object.keys(stats.skipped)]);
  for (const kind of kinds) {
    console.log(`  ${kind.padEnd(20)} ${String(stats.created[kind] ?? 0).padStart(4)} / ${stats.skipped[kind] ?? 0}`);
  }
  console.log(`\nContraseña de todas las cuentas: ${PASSWORD}`);
  console.log('Cuentas destacadas:');
  for (const featured of FEATURED) console.log(`  ${PEOPLE[featured.key].email}  ${featured.case}`);
}

function classesOf(userKey) {
  const keys = new Set();
  for (const [classKey, definition] of Object.entries(CLASSES)) if (definition.delegate === userKey) keys.add(classKey);
  for (const child of ALL_CHILDREN) if (child.guardian === userKey) keys.add(child.class);
  return [...keys];
}

async function verifyAll() {
  console.log(`Comprobación contra ${API}`);
  for (const featured of FEATURED) {
    const person = PEOPLE[featured.key];
    const me = await send('GET', '/users/me', { as: featured.key });
    const students = await send('GET', '/users/me/students', { as: featured.key });
    const names = Array.isArray(students.data) ? students.data.map((s) => s.firstName ?? s.name).join(', ') : '-';
    console.log(`\n${person.fullName} <${person.email}> (${me.status}) hijos: ${names || 'ninguno'}`);
    for (const classKey of classesOf(featured.key)) {
      const classId = ids.class(classKey);
      const get = async (path) => (await send('GET', path, { as: featured.key })).data;
      const dashboard = await get(`/dashboard/summary?classId=${classId}&days=30`);
      const announcements = await get(`/announcements?classId=${classId}&limit=50`);
      const events = await get(`/calendar-events?classId=${classId}`);
      const lost = await get(`/lost-items?classId=${classId}&limit=50`);
      const homework = await get(`/assignments?classId=${classId}&limit=50`);
      const expenses = await get(`/class-expenses?classId=${classId}&limit=50`);
      const size = (value) => (Array.isArray(value) ? value.length : (value?.items?.length ?? '?'));
      console.log(
        `  ${CLASSES[classKey].name}: avisos ${size(announcements)}, eventos ${size(events)}, objetos ${size(lost)}, ` +
          `deberes ${size(homework)}, gastos ${size(expenses)}`,
      );
      console.log(`    panel: ${JSON.stringify(dashboard)}`);
    }
    if (featured.key === 'irene') {
      const provinces = await send('GET', '/provinces', { as: 'irene' });
      console.log(`  Alta: ${Array.isArray(provinces.data) ? provinces.data.length : '?'} provincias disponibles`);
    }
  }
}

function runSql(sql) {
  const result = spawnSync(
    'docker',
    ['exec', '-i', DB.container, 'psql', '-U', DB.user, '-d', DB.name, '-X', '-v', 'ON_ERROR_STOP=1'],
    { input: sql, encoding: 'utf8' },
  );
  if (result.status !== 0) throw new Error(`psql falló: ${result.stderr || result.stdout}`);
  return result.stdout;
}

const quoteList = (values) => values.map((value) => `'${value}'`).join(',');

function resetAll() {
  const demo = allDemoIds();
  const users = quoteList(demo.users);
  const classes = quoteList(demo.classes);
  const students = quoteList(demo.students);

  const guard = runSql(
    `SELECT count(*) FROM "Users" WHERE "Id" IN (${users}) AND "Email" NOT LIKE '${DEMO_EMAIL_PATTERN}';`,
  );
  if (!/\n\s*0\s*\n/.test(guard)) {
    throw new Error('Algún usuario con id de la demo no tiene correo de demostración: no se borra nada.');
  }

  const deletions = [
    ['AnnouncementReadReceipts', `"AnnouncementId" IN (${quoteList(demo.announcements)})`],
    ['Announcements', `"Id" IN (${quoteList(demo.announcements)})`],
    ['AssignmentComments', `"AssignmentId" IN (${quoteList(demo.assignments)})`],
    ['AssignmentCompletions', `"AssignmentId" IN (${quoteList(demo.assignments)})`],
    ['Assignments', `"Id" IN (${quoteList(demo.assignments)})`],
    ['ClassExpenseParticipants', `"ClassExpenseId" IN (${quoteList(demo.expenses)})`],
    ['ClassExpenses', `"Id" IN (${quoteList(demo.expenses)})`],
    ['LostItems', `"Id" IN (${quoteList(demo.lostItems)})`],
    ['CalendarEvents', `"Id" IN (${quoteList(demo.events)})`],
    ['Subjects', `"Id" IN (${quoteList(demo.subjects)})`],
    ['WeeklySummaries', `"ClassId" IN (${classes})`],
    ['Enrollments', `"Id" IN (${quoteList(demo.enrollments)})`],
    ['UserClassRoles', `"ClassId" IN (${classes}) AND "UserId" IN (${users})`],
    ['StudentGuardians', `"StudentId" IN (${students})`],
    ['Students', `"Id" IN (${students})`],
    ['RefreshTokens', `"SessionId" IN (SELECT "Id" FROM "Sessions" WHERE "UserId" IN (${users}))`],
    ['Sessions', `"UserId" IN (${users})`],
    ['Classes', `"Id" IN (${classes})`],
    ['SchoolYears', `"Id" IN (${quoteList(demo.years)})`],
    ['Users', `"Id" IN (${users})`],
  ];

  console.log(`Base de datos: contenedor ${DB.container}, base ${DB.name}`);
  const counts = deletions
    .map(([table, where]) => `SELECT '${table}' AS tabla, count(*) AS filas FROM "${table}" WHERE ${where}`)
    .join('\nUNION ALL\n');
  console.log(runSql(`${counts};`));
  if (flags['dry-run']) {
    console.log('Simulación: no se ha borrado nada.');
    return;
  }
  const statements = deletions.map(([table, where]) => `DELETE FROM "${table}" WHERE ${where};`);
  console.log(runSql(`BEGIN;\n${statements.join('\n')}\nCOMMIT;`));
  console.log('Demo borrada. Lo que ya existía antes de la demo no se ha tocado.');
}

try {
  if (command === 'load') await loadAll();
  else if (command === 'reset') resetAll();
  else if (command === 'verify') await verifyAll();
  else {
    console.error(`Orden desconocida «${command}».\n${HELP}`);
    process.exit(1);
  }
} catch (error) {
  console.error(`\nERROR: ${error.message}`);
  process.exit(1);
}
