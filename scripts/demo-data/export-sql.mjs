#!/usr/bin/env node
// Exports the demo from the LOCAL database as plain SQL files (INSERT statements) that can be run in any PostgreSQL
// client, plus the SQL script that moves the demo dates to a new day. Local only: it reads with `docker exec ... psql`.
// Rows are picked ONLY by the demo ids (never by dumping a table) and literals are built by PostgreSQL itself.
// Usage: node scripts/demo-data/export-sql.mjs --out=<folder>
import { mkdirSync, writeFileSync } from 'node:fs';
import { join, resolve } from 'node:path';
import { spawnSync } from 'node:child_process';
import { ANCHOR_SENTINEL, SCHOOL_YEAR, allDemoIds, anchorSentinelId, extensionDemoIds } from './dataset.mjs';

const HELP = `Uso: node scripts/demo-data/export-sql.mjs [opciones]
  --out=CARPETA          Carpeta donde se escriben los ficheros (por defecto, la actual).
  --db-container=NOMBRE  Contenedor de PostgreSQL (por defecto sumiclas-postgres).
  --db-user=USUARIO      Usuario (por defecto sumiclas).
  --db-name=BASE         Base de datos (por defecto sumiclas).
Escribe Datos-demo-SumiClas.sql, Datos-demo-ampliados-SumiClas.sql y Actualizar-fechas-demo-SumiClas.sql.
Solo lee de la base local; no toca ninguna fila.`;

const flags = Object.fromEntries(
  process.argv.slice(2).map((arg) => {
    const [key, value] = arg.replace(/^--/, '').split('=');
    return [key, value ?? true];
  }),
);
if (flags.help) {
  console.log(HELP);
  process.exit(0);
}

const DB = {
  container: flags['db-container'] ?? 'sumiclas-postgres',
  user: flags['db-user'] ?? 'sumiclas',
  name: flags['db-name'] ?? 'sumiclas',
};
const OUT = resolve(typeof flags.out === 'string' ? flags.out : '.');
const BATCH_SIZE = 500;
const MAX_OUTPUT_BYTES = 512 * 1024 * 1024;

const FILE_FULL = 'Datos-demo-SumiClas.sql';
const FILE_ADDITIVE = 'Datos-demo-ampliados-SumiClas.sql';
const FILE_REFRESH = 'Actualizar-fechas-demo-SumiClas.sql';
const FILE_BASE_ONLY = 'Datos-demo-base-solo-SumiClas.sql';

function psql(sql) {
  const result = spawnSync(
    'docker',
    ['exec', '-i', DB.container, 'psql', '-U', DB.user, '-d', DB.name, '-X', '-A', '-t', '-q', '-v', 'ON_ERROR_STOP=1'],
    { input: sql, encoding: 'utf8', maxBuffer: MAX_OUTPUT_BYTES },
  );
  if (result.status !== 0) throw new Error(`psql fallo: ${result.stderr || result.stdout || result.error}`);
  return result.stdout.trim();
}

const list = (values) => values.map((value) => `'${value}'`).join(', ');
const subtract = (all, removed) => {
  const toRemove = new Set(removed);
  return all.filter((value) => !toRemove.has(value));
};

// Parent-before-child order. `where` receives the id sets of the subset being exported (an empty set yields no rows).
const TABLES = [
  { name: 'Provinces', catalog: true },
  { name: 'Localities', catalog: true },
  { name: 'PostalCodes', catalog: true },
  { name: 'Schools', catalog: true },
  { name: 'Users', where: (s) => `"Id" IN (${list(s.users)})`, needs: ['users'] },
  { name: 'SchoolYears', where: (s) => `"Id" IN (${list(s.years)})`, needs: ['years'] },
  { name: 'Classes', where: (s) => `"Id" IN (${list(s.classes)})`, needs: ['classes'] },
  { name: 'Students', where: (s) => `"Id" IN (${list(s.students)})`, needs: ['students'] },
  { name: 'StudentGuardians', where: (s) => `"StudentId" IN (${list(s.students)})`, needs: ['students'] },
  { name: 'UserClassRoles', where: (s) => `"ClassId" IN (${list(s.classes)}) AND "UserId" IN (${list(s.users)})`, needs: ['classes', 'users'] },
  { name: 'Enrollments', where: (s) => `"Id" IN (${list(s.enrollments)})`, needs: ['enrollments'] },
  { name: 'Subjects', where: (s) => `"Id" IN (${list(s.subjects)})`, needs: ['subjects'] },
  { name: 'CalendarEvents', where: (s) => `"Id" IN (${list(s.events)})`, needs: ['events'] },
  { name: 'LostItems', where: (s) => `"Id" IN (${list(s.lostItems)})`, needs: ['lostItems'] },
  { name: 'ClassExpenses', where: (s) => `"Id" IN (${list(s.expenses)})`, needs: ['expenses'] },
  { name: 'ClassExpenseParticipants', where: (s) => `"ClassExpenseId" IN (${list(s.expenses)})`, needs: ['expenses'] },
  { name: 'Assignments', where: (s) => `"Id" IN (${list(s.assignments)})`, needs: ['assignments'] },
  { name: 'AssignmentCompletions', where: (s) => `"AssignmentId" IN (${list(s.assignments)})`, needs: ['assignments'] },
  { name: 'AssignmentComments', where: (s) => `"AssignmentId" IN (${list(s.assignments)})`, needs: ['assignments'] },
  { name: 'Announcements', where: (s) => `"Id" IN (${list(s.announcements)})`, needs: ['announcements'] },
  { name: 'AnnouncementReadReceipts', where: (s) => `"AnnouncementId" IN (${list(s.announcements)})`, needs: ['announcements'] },
];

const all = allDemoIds();
const extension = extensionDemoIds();
const CONTENT_KINDS = Object.keys(extension);
const EMPTY = Object.fromEntries(Object.keys(all).map((kind) => [kind, []]));

const SUBSETS = {
  full: { catalog: true, ids: all },
  base: {
    catalog: true,
    ids: { ...all, ...Object.fromEntries(CONTENT_KINDS.map((kind) => [kind, subtract(all[kind], extension[kind])])) },
  },
  extension: { catalog: false, ids: { ...EMPTY, ...extension } },
};

function columnsOf(table) {
  const columns = psql(
    `SELECT string_agg(quote_ident(column_name), ', ' ORDER BY ordinal_position) FROM information_schema.columns WHERE table_schema = 'public' AND table_name = '${table}';`,
  );
  if (!columns) throw new Error(`La tabla ${table} no existe en la base de datos.`);
  return columns;
}

function rowLiterals(table, where) {
  const filter = where ? ` WHERE ${where}` : '';
  const json = psql(
    `SELECT coalesce(json_agg(r ORDER BY r), '[]'::json) FROM (SELECT '(' || (SELECT string_agg(quote_nullable(v.value), ', ') FROM json_each_text(to_json(t)) v) || ')' AS r FROM "${table}" t${filter}) x;`,
  );
  return JSON.parse(json);
}

const columnCache = new Map();
function tableStatements(table, rows) {
  if (!columnCache.has(table)) columnCache.set(table, columnsOf(table));
  const columns = columnCache.get(table);
  const statements = [];
  for (let start = 0; start < rows.length; start += BATCH_SIZE) {
    statements.push(`INSERT INTO "${table}" (${columns}) VALUES\n${rows.slice(start, start + BATCH_SIZE).join(',\n')};`);
  }
  return statements;
}

function exportSubset(subsetName) {
  const subset = SUBSETS[subsetName];
  const tables = [];
  for (const table of TABLES) {
    if (table.catalog && !subset.catalog) continue;
    if (!table.catalog && table.needs.some((kind) => subset.ids[kind].length === 0)) continue;
    const rows = rowLiterals(table.name, table.catalog ? null : table.where(subset.ids));
    if (rows.length > 0) tables.push({ name: table.name, rows });
  }
  return tables;
}

function renderInserts(tables) {
  const body = tables.map((table) => tableStatements(table.name, table.rows).join('\n\n')).join('\n\n');
  return `BEGIN;\n\n${body}\n\nCOMMIT;\n`;
}

const FULL_HEADER = `-- SumiClas: catalogo territorial (provincias, poblaciones, codigos postales y colegios) y datos de demostracion
-- (cuentas, familias, clases, avisos, eventos, deberes, gastos y objetos perdidos), todo inventado.
--
-- Requisitos: las migraciones de la aplicacion ya aplicadas y estas tablas vacias.
-- Se ejecuta tal cual desde pgAdmin, DBeaver, Azure Data Studio o psql: son sentencias INSERT estandar, sin comandos
-- propios de ningun cliente. Todo va en una unica transaccion: o entra todo o no entra nada.
-- No incluye sesiones, tokens de refresco, resumenes de IA ni reclamaciones de objetos perdidos.
-- Las cuentas de la demo usan la contrasena publica Demo1234 (correos @demo.sumiclas.test).
--
-- Despues de cargarlo, ejecuta ${FILE_REFRESH} para colocar las fechas en el dia que quieras.
`;

const ADDITIVE_HEADER = `-- SumiClas: ampliacion de la demo (contenido de todo el curso: mas avisos, eventos, cumpleanos, deberes, gastos y
-- objetos perdidos, con sus lecturas, comentarios, pagos y entregas). Es ADITIVO: solo anade filas nuevas.
--
-- Requisito: una base de datos que ya haya cargado ${FILE_FULL} en su version anterior (la demo base) y, por tanto,
-- tenga ya el catalogo, las cuentas, las familias y las clases. Si la base esta vacia, carga ${FILE_FULL} (que ya
-- incluye todo esto) y no este fichero.
-- Se ejecuta tal cual desde pgAdmin, DBeaver, Azure Data Studio o psql, en una unica transaccion. No debe ejecutarse
-- dos veces (las claves repetidas lo harian fallar entero, sin dejar nada a medias).
-- Las cuentas de la demo usan la contrasena publica Demo1234.
--
-- Despues, ejecuta ${FILE_REFRESH}: las filas de la demo base cargadas antes quedan un dia desplazadas hasta entonces.
`;

const BASE_ONLY_HEADER = `-- SOLO PARA VERIFICAR: catalogo y demo base (sin la ampliacion). Se usa para comprobar que base + ampliacion = completo.
`;

const KNOWN_TIME_COLUMNS = [
  'Users.PasswordChangedAt',
  'Announcements.CreatedAt',
  'Announcements.UpdatedAt',
  'AnnouncementReadReceipts.ReadAt',
  'Assignments.DueDate',
  'Assignments.CreatedAt',
  'Assignments.UpdatedAt',
  'AssignmentComments.CreatedAt',
  'AssignmentCompletions.CompletedAt',
  'CalendarEvents.CreatedAt',
  'CalendarEvents.UpdatedAt',
  'ClassExpenses.CreatedAt',
  'ClassExpenses.UpdatedAt',
  'Enrollments.CreatedAt',
  'Enrollments.ConfirmedAt',
  'LostItems.CreatedAt',
  'LostItems.UpdatedAt',
  'LostItemClaims.CreatedAt',
];

const NOT_SHIFTED_TIME_COLUMNS = [
  'SchoolYears.StartDate',
  'SchoolYears.EndDate',
  'WeeklySummaries.WeekStart',
  'WeeklySummaries.GeneratedAt',
  'Sessions.CreatedAt',
  'Sessions.RevokedAt',
  'Sessions.LastRotatedAt',
  'RefreshTokens.CreatedAt',
  'RefreshTokens.ExpiresAt',
  'RefreshTokens.ReplacedAt',
];

const uuidArray = (name, values) => `  ${name} uuid[] := ARRAY[\n    ${values.map((value) => `'${value}'`).join(',\n    ')}\n  ]::uuid[];`;
const quotedList = (values) => values.map((value) => `'${value}'`).join(', ');

// Shift of a timestamptz by whole days computed in UTC, so the result does not depend on the session time zone.
const shiftedTimestamp = (column) => `"${column}" = (("${column}" AT TIME ZONE 'UTC') + shift) AT TIME ZONE 'UTC'`;

function updateBlock(table, idColumn, idArray, assignments, label = table) {
  return `  UPDATE "${table}" SET ${assignments.join(', ')}
    WHERE "${idColumn}" = ANY(${idArray});
  GET DIAGNOSTICS affected = ROW_COUNT;
  report := report || format(E'\\n  %-28s %s', '${label}', affected);`;
}

function refreshScript() {
  const sentinel = anchorSentinelId();
  const blocks = [
    updateBlock('Users', 'Id', 'demo_users', [shiftedTimestamp('PasswordChangedAt')]),
    updateBlock('Enrollments', 'Id', 'demo_enrollments', [shiftedTimestamp('CreatedAt'), shiftedTimestamp('ConfirmedAt')]),
    updateBlock('CalendarEvents', 'Id', 'demo_events', [shiftedTimestamp('CreatedAt'), shiftedTimestamp('UpdatedAt')]),
    `  UPDATE "CalendarEvents" SET
      "EventYear" = EXTRACT(YEAR FROM make_date("EventYear", "EventMonth", "EventDay") + delta)::int,
      "EventMonth" = EXTRACT(MONTH FROM make_date("EventYear", "EventMonth", "EventDay") + delta)::int,
      "EventDay" = EXTRACT(DAY FROM make_date("EventYear", "EventMonth", "EventDay") + delta)::int
    WHERE "Id" = ANY(demo_events) AND "EventYear" IS NOT NULL;
  GET DIAGNOSTICS affected = ROW_COUNT;
  report := report || format(E'\\n  %-28s %s', 'CalendarEvents (fecha)', affected);`,
    `  -- Birthdays repeat every year (no year stored): move the occurrence closest to the old anchor and keep month and day.
  UPDATE "CalendarEvents" AS e SET "EventMonth" = moved.new_month, "EventDay" = moved.new_day
    FROM (
      SELECT ev."Id" AS event_id,
             EXTRACT(MONTH FROM best.occurrence + delta)::int AS new_month,
             EXTRACT(DAY FROM best.occurrence + delta)::int AS new_day
      FROM "CalendarEvents" ev
      CROSS JOIN LATERAL (
        SELECT candidate.occurrence
        FROM (
          SELECT make_date(
                   candidate_year,
                   ev."EventMonth",
                   LEAST(ev."EventDay", EXTRACT(DAY FROM (make_date(candidate_year, ev."EventMonth", 1) + INTERVAL '1 month' - INTERVAL '1 day'))::int)
                 ) AS occurrence
          FROM generate_series(EXTRACT(YEAR FROM current_anchor)::int - 1, EXTRACT(YEAR FROM current_anchor)::int + 1) AS candidate_year
        ) candidate
        ORDER BY abs(candidate.occurrence - current_anchor)
        LIMIT 1
      ) best
      WHERE ev."Id" = ANY(demo_events) AND ev."EventYear" IS NULL
    ) moved
    WHERE e."Id" = moved.event_id;
  GET DIAGNOSTICS affected = ROW_COUNT;
  report := report || format(E'\\n  %-28s %s', 'CalendarEvents (cumples)', affected);`,
    updateBlock('Announcements', 'Id', 'demo_announcements', [shiftedTimestamp('CreatedAt'), shiftedTimestamp('UpdatedAt')]),
    updateBlock('AnnouncementReadReceipts', 'AnnouncementId', 'demo_announcements', [shiftedTimestamp('ReadAt')]),
    updateBlock('LostItems', 'Id', 'demo_lost_items', [shiftedTimestamp('CreatedAt'), shiftedTimestamp('UpdatedAt')]),
    updateBlock('LostItemClaims', 'LostItemId', 'demo_lost_items', [shiftedTimestamp('CreatedAt')]),
    updateBlock('ClassExpenses', 'Id', 'demo_expenses', [shiftedTimestamp('CreatedAt'), shiftedTimestamp('UpdatedAt')]),
    updateBlock('Assignments', 'Id', 'demo_assignments', [
      '"DueDate" = "DueDate" + delta',
      shiftedTimestamp('CreatedAt'),
      shiftedTimestamp('UpdatedAt'),
    ]),
    updateBlock('AssignmentCompletions', 'AssignmentId', 'demo_assignments', [shiftedTimestamp('CompletedAt')]),
    updateBlock('AssignmentComments', 'AssignmentId', 'demo_assignments', [shiftedTimestamp('CreatedAt')]),
  ];

  return `-- SumiClas: colocar las fechas de la demo en el dia que quieras.
--
-- Cuando ejecutarlo: cada dos semanas aproximadamente (o antes de una demostracion), para que el contenido de la demo
-- (avisos, eventos, deberes, cumpleanos...) siga cayendo alrededor de hoy y el resumen semanal tenga material.
-- Como usarlo: cambia, en las dos primeras lineas del bloque, new_anchor (por defecto, hoy) y, si quieres solo ver lo
-- que haria, pon dry_run en true. Despues ejecuta TODO el bloque en pgAdmin, DBeaver, Azure Data Studio o psql.
--
-- Que hace: lee el dia ancla actual del aviso centinela de la demo (el aviso '${ANCHOR_SENTINEL.key}' de 3A, cuya fecha de
-- publicacion es exactamente el ancla), calcula cuantos dias hay hasta new_anchor y mueve ESA cantidad de dias todas las
-- fechas de las filas de la demo (se reconocen por sus ids, que van escritos aqui): publicaciones y ediciones, fechas de
-- entrega, fecha de los eventos y cumpleanos, lecturas, entregas, comentarios, altas y confirmaciones de matricula, etc.
-- Las fechas vacias siguen vacias. No mueve las fechas del curso escolar (se avisa si el nuevo ancla cae fuera de el) ni
-- toca nada que no sea de la demo. Borra los resumenes semanales de IA de las clases de la demo para que se vuelvan a generar
-- (hace falta la clave Ai__ApiKey en la aplicacion). Es atomico y se puede repetir: si el ancla ya es new_anchor, no hace nada.
-- Con dry_run = true informa y deja todo como estaba (termina con un error a proposito: simulacion).
-- Los mensajes (avisos del bloque) salen en la pestana de mensajes de tu cliente.

DO $refresh$
DECLARE
  new_anchor date := CURRENT_DATE;
  dry_run boolean := false;

  sentinel_id uuid := '${sentinel}';
${uuidArray('demo_users', all.users)}
${uuidArray('demo_enrollments', all.enrollments)}
${uuidArray('demo_classes', all.classes)}
${uuidArray('demo_years', all.years)}
${uuidArray('demo_events', all.events)}
${uuidArray('demo_announcements', all.announcements)}
${uuidArray('demo_lost_items', all.lostItems)}
${uuidArray('demo_expenses', all.expenses)}
${uuidArray('demo_assignments', all.assignments)}

  current_anchor date;
  delta integer;
  shift interval;
  affected integer;
  report text := '';
  unknown_column record;
  school_year record;
  known_time_columns text[] := ARRAY[${quotedList(KNOWN_TIME_COLUMNS)}];
  ignored_time_columns text[] := ARRAY[${quotedList(NOT_SHIFTED_TIME_COLUMNS)}];
BEGIN
  SELECT ("CreatedAt" AT TIME ZONE 'UTC')::date INTO current_anchor FROM "Announcements" WHERE "Id" = sentinel_id;
  IF current_anchor IS NULL THEN
    RAISE EXCEPTION 'No se encuentra el aviso centinela de la demo (%). Se han cargado los datos de la demo en esta base de datos?', sentinel_id;
  END IF;

  delta := new_anchor - current_anchor;
  IF delta = 0 THEN
    RAISE NOTICE 'El ancla de la demo ya es % : no hay nada que cambiar.', current_anchor;
    RETURN;
  END IF;
  shift := delta * INTERVAL '1 day';

  FOR unknown_column IN
    SELECT table_name, column_name FROM information_schema.columns
    WHERE table_schema = current_schema()
      AND data_type IN ('date', 'timestamp with time zone', 'timestamp without time zone')
      AND table_name || '.' || column_name <> ALL (known_time_columns || ignored_time_columns)
      AND table_name <> '__EFMigrationsHistory'
  LOOP
    RAISE NOTICE 'AVISO: la columna de fecha %.% no la conoce este script y no se ha movido.', unknown_column.table_name, unknown_column.column_name;
  END LOOP;

  FOR school_year IN SELECT "Label", "StartDate", "EndDate" FROM "SchoolYears" WHERE "Id" = ANY(demo_years) LOOP
    IF new_anchor < school_year."StartDate" OR new_anchor > school_year."EndDate" THEN
      RAISE NOTICE 'AVISO: % queda fuera del curso escolar de la demo % (% a %), cuyas fechas no se mueven.',
        new_anchor, school_year."Label", school_year."StartDate", school_year."EndDate";
    END IF;
  END LOOP;

${blocks.join('\n\n')}

  DELETE FROM "WeeklySummaries" WHERE "ClassId" = ANY(demo_classes);
  GET DIAGNOSTICS affected = ROW_COUNT;
  report := report || format(E'\\n  %-28s %s (borradas)', 'WeeklySummaries', affected);

  RAISE NOTICE E'Demo desplazada: ancla anterior %, ancla nueva %, diferencia % dias. Filas actualizadas:%',
    current_anchor, new_anchor, delta, report;

  IF dry_run THEN
    RAISE EXCEPTION 'simulacion: no se ha cambiado nada (dry_run = true). Pon dry_run := false para aplicar el cambio.';
  END IF;
END
$refresh$;
`;
}

function countsOf(tables) {
  return Object.fromEntries(tables.map((table) => [table.name, table.rows.length]));
}

function write(file, content) {
  const path = join(OUT, file);
  writeFileSync(path, content, 'utf8');
  console.log(`  ${path}  (${(Buffer.byteLength(content) / 1024).toFixed(0)} KB)`);
}

try {
  mkdirSync(OUT, { recursive: true });
  console.log(`Base de datos: contenedor ${DB.container}, base ${DB.name}. Escribiendo en ${OUT}`);
  const full = exportSubset('full');
  const extensionRows = exportSubset('extension');
  write(FILE_FULL, `${FULL_HEADER}\n${renderInserts(full)}`);
  write(FILE_ADDITIVE, `${ADDITIVE_HEADER}\n${renderInserts(extensionRows)}`);
  write(FILE_REFRESH, refreshScript());
  if (flags['emit-base-only']) write(FILE_BASE_ONLY, `${BASE_ONLY_HEADER}\n${renderInserts(exportSubset('base'))}`);
  console.log('\nFilas por tabla (completo):', JSON.stringify(countsOf(full)));
  console.log('Filas por tabla (ampliacion):', JSON.stringify(countsOf(extensionRows)));
  console.log(`Curso escolar de referencia: ${SCHOOL_YEAR.startDate} a ${SCHOOL_YEAR.endDate}`);
} catch (error) {
  console.error(`\nERROR: ${error.message}`);
  process.exit(1);
}
