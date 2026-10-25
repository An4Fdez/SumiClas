import { createHash } from 'node:crypto';

export const PASSWORD = 'Demo1234';
export const EMAIL_DOMAIN = 'demo.sumiclas.test';

const NAMESPACE = 'b3f1c2a4-5d6e-4f70-8a91-0c2d3e4f5a6b';

export function uid(key) {
  const namespaceBytes = Buffer.from(NAMESPACE.replace(/-/g, ''), 'hex');
  const hash = createHash('sha1').update(namespaceBytes).update(key).digest();
  hash[6] = (hash[6] & 0x0f) | 0x50;
  hash[8] = (hash[8] & 0x3f) | 0x80;
  const hex = hash.subarray(0, 16).toString('hex');
  return `${hex.slice(0, 8)}-${hex.slice(8, 12)}-${hex.slice(12, 16)}-${hex.slice(16, 20)}-${hex.slice(20)}`;
}

export function addDays(day, offset) {
  const millisecondsPerDay = 86_400_000;
  return new Date(Date.parse(`${day}T00:00:00Z`) + offset * millisecondsPerDay).toISOString().slice(0, 10);
}

const COMPOUND_FIRST_NAMES = new Set(['rosa maría']);

function emailOf(fullName) {
  const words = fullName
    .normalize('NFD')
    .replace(/[̀-ͯ]/g, '')
    .toLowerCase()
    .split(' ');
  const firstNameWords = COMPOUND_FIRST_NAMES.has(fullName.split(' ').slice(0, 2).join(' ').toLowerCase()) ? 2 : 1;
  return `${words[0]}.${words[firstNameWords]}@${EMAIL_DOMAIN}`;
}

export const SCHOOLS = {
  aravaca: { id: '0547848d-a673-4599-a4f3-ca583fdd0b49', label: 'CEIP Aravaca (Madrid)' },
  sevilla: { id: 'fe57f114-ea79-40c4-8666-88c29ca25633', label: 'CEIP Aníbal González (Sevilla)' },
};

export const SCHOOL_YEAR = { label: '2026-2027', startDate: '2026-09-01', endDate: '2027-06-30' };

export const CLASSES = {
  aravaca3a: {
    school: 'aravaca',
    name: '3ºA',
    delegate: 'lucia',
    subjects: ['Matemáticas', 'Lengua', 'Ciencias Naturales', 'Inglés', 'Educación Física'],
  },
  aravaca4b: {
    school: 'aravaca',
    name: '4ºB',
    delegate: 'javier',
    subjects: ['Matemáticas', 'Lengua', 'Ciencias Sociales', 'Inglés', 'Música'],
  },
  sevilla2a: {
    school: 'sevilla',
    name: '2ºA',
    delegate: 'carmen',
    subjects: ['Matemáticas', 'Lengua', 'Conocimiento del Medio', 'Inglés'],
  },
};

const PEOPLE_NAMES = {
  lucia: 'Lucía Navarro Ortega',
  marcos: 'Marcos Gil Prieto',
  elena: 'Elena Ruiz Campos',
  tomas: 'Tomás Herrero Vidal',
  irene: 'Irene Castillo Mora',
  carmen: 'Carmen Ruiz Vega',
  javier: 'Javier Molina Castro',
  beatriz: 'Beatriz Domínguez Ríos',
  raul: 'Raúl Ortiz Fuentes',
  marta: 'Marta Sánchez Bravo',
  andres: 'Andrés Cabrera León',
  rosa: 'Rosa María Peña Gallego',
  patricia: 'Patricia Vega Montes',
  sergio: 'Sergio Blanco Iglesias',
  cristina: 'Cristina Ramos Delgado',
  manuel: 'Manuel Jiménez Bautista',
  inmaculada: 'Inmaculada Torres Reyes',
  antonio: 'Antonio Rivas Camacho',
};

export const PEOPLE = Object.fromEntries(
  Object.entries(PEOPLE_NAMES).map(([key, fullName]) => [
    key,
    { key, fullName, email: emailOf(fullName), id: uid(`user:${key}`) },
  ]),
);

export const FEATURED = [
  { key: 'lucia', case: 'Madre y delegada de 3ºA (CEIP Aravaca): publica avisos, eventos, deberes y gastos, y ve todo lo de su clase.' },
  { key: 'marcos', case: 'Padre con dos hijos mellizos en la misma clase (3ºA), con avisos sin leer y un gasto pendiente.' },
  { key: 'elena', case: 'Madre con dos hijos en clases distintas del mismo colegio (4ºB y 3ºA): cambia de hijo y de clase con el selector de hijo.' },
  { key: 'tomas', case: 'Padre con la hija al día: todo leído, deberes hechos y gastos pagados (panel «sin nada pendiente»).' },
  { key: 'carmen', case: 'Madre y delegada de 2ºA en otro colegio y otra ciudad (CEIP Aníbal González, Sevilla).' },
  { key: 'irene', case: 'Recién registrada, sin hijos ni clase: recorre el alta (provincia, población, colegio, curso y clase).' },
];

// guardian, role, children. Children: key, first name, surnames, class.
export const FAMILIES = [
  { guardian: 'lucia', role: 'Mother', children: [{ key: 'hugo', first: 'Hugo', s1: 'Navarro', s2: 'Salas', class: 'aravaca3a' }] },
  {
    guardian: 'marcos',
    role: 'Father',
    children: [
      { key: 'sara', first: 'Sara', s1: 'Gil', s2: 'Romero', class: 'aravaca3a' },
      { key: 'pablo', first: 'Pablo', s1: 'Gil', s2: 'Romero', class: 'aravaca3a' },
    ],
  },
  {
    guardian: 'elena',
    role: 'Mother',
    children: [
      { key: 'alba', first: 'Alba', s1: 'Ruiz', s2: 'Soto', class: 'aravaca4b' },
      { key: 'daniel', first: 'Daniel', s1: 'Ruiz', s2: 'Soto', class: 'aravaca3a' },
    ],
  },
  { guardian: 'tomas', role: 'Father', children: [{ key: 'nora', first: 'Nora', s1: 'Herrero', s2: 'Pons', class: 'aravaca3a' }] },
  { guardian: 'beatriz', role: 'Mother', children: [{ key: 'leo', first: 'Leo', s1: 'Domínguez', s2: 'Cano', class: 'aravaca3a' }] },
  { guardian: 'raul', role: 'Father', children: [{ key: 'lola', first: 'Lola', s1: 'Ortiz', s2: 'Mena', class: 'aravaca3a' }] },
  { guardian: 'marta', role: 'Mother', children: [{ key: 'iker', first: 'Íker', s1: 'Sánchez', s2: 'Lara', class: 'aravaca3a' }] },
  { guardian: 'andres', role: 'Father', children: [{ key: 'valeria', first: 'Valeria', s1: 'Cabrera', s2: 'Núñez', class: 'aravaca3a' }] },
  { guardian: 'rosa', role: 'Guardian', children: [{ key: 'mario', first: 'Mario', s1: 'Peña', s2: 'Gallego', class: 'aravaca3a' }] },
  { guardian: 'javier', role: 'Father', children: [{ key: 'julia', first: 'Julia', s1: 'Molina', s2: 'Aguirre', class: 'aravaca4b' }] },
  { guardian: 'patricia', role: 'Mother', children: [{ key: 'gonzalo', first: 'Gonzalo', s1: 'Vega', s2: 'Arias', class: 'aravaca4b' }] },
  { guardian: 'sergio', role: 'Father', children: [{ key: 'noa', first: 'Noa', s1: 'Blanco', s2: 'Sanz', class: 'aravaca4b' }] },
  { guardian: 'cristina', role: 'Mother', children: [{ key: 'adrian', first: 'Adrián', s1: 'Ramos', s2: 'Medina', class: 'aravaca4b' }] },
  { guardian: 'carmen', role: 'Mother', children: [{ key: 'candela', first: 'Candela', s1: 'Ruiz', s2: 'Soler', class: 'sevilla2a' }] },
  { guardian: 'manuel', role: 'Father', children: [{ key: 'lucas', first: 'Lucas', s1: 'Jiménez', s2: 'Cruz', class: 'sevilla2a' }] },
  { guardian: 'inmaculada', role: 'Mother', children: [{ key: 'rocio', first: 'Rocío', s1: 'Torres', s2: 'Macías', class: 'sevilla2a' }] },
  { guardian: 'antonio', role: 'Father', children: [{ key: 'mateo', first: 'Mateo', s1: 'Rivas', s2: 'Ortega', class: 'sevilla2a' }] },
];

export const ALL_CHILDREN = FAMILIES.flatMap((family) =>
  family.children.map((child) => ({ ...child, guardian: family.guardian, role: family.role })),
);

// Content of every class. Offsets are days from the chosen day. Announcements are listed in creation order, so the
// last one appears first in the app.
export const CONTENT = {
  aravaca3a: {
    events: [
      { key: 'prado', type: 'Trip', title: 'Excursión al Museo del Prado', offset: 6, time: '10:00:00', location: 'Museo Nacional del Prado (Madrid)', cost: '8 € por alumno' },
      { key: 'reunion', type: 'Meeting', title: 'Reunión de padres del primer trimestre', offset: 3, time: '17:30:00', location: 'Aula de 3ºA' },
      { key: 'reciclaje', type: 'Activity', title: 'Taller de reciclaje con material de casa', offset: 9, time: '12:00:00', location: 'Patio del colegio' },
      { key: 'fotos', type: 'Other', title: 'Fotografía de clase', offset: 12, time: '09:30:00', location: 'Salón de actos' },
    ],
    birthdays: [
      { key: 'hugo', child: 'hugo', by: 'lucia', offset: 2 },
      { key: 'sara', child: 'sara', by: 'marcos', offset: 4 },
      { key: 'nora', child: 'nora', by: 'tomas', offset: 10 },
      { key: 'lola', child: 'lola', by: 'raul', offset: 15 },
    ],
    announcements: [
      {
        key: 'calzado',
        priority: 'Low',
        title: 'Recordatorio: calzado deportivo los martes y jueves',
        body: 'Os recuerdo que los martes y los jueves tenemos Educación Física a primera hora. Los niños tienen que venir con zapatillas de deporte y con ropa cómoda. Gracias por vuestra ayuda.',
      },
      {
        key: 'reunion',
        priority: 'Normal',
        event: 'reunion',
        title: 'Reunión de padres del primer trimestre',
        body: 'Queridas familias: os convoco a la reunión del primer trimestre en el aula de 3ºA. Hablaremos del ritmo de la clase, de las excursiones previstas y de cómo ayudar en casa con los deberes. Es importante que asista al menos un adulto por familia.',
      },
      {
        key: 'prado',
        priority: 'Normal',
        event: 'prado',
        title: 'Excursión al Museo del Prado: autorización y 8 euros',
        body: 'Vamos a visitar el Museo del Prado. Salimos a las 10:00 y volvemos para la hora de comer. Necesito la autorización firmada y 8 euros por alumno cuanto antes. El gasto está en la sección Gastos para que cada familia marque su pago.',
      },
      {
        key: 'huelga',
        priority: 'High',
        title: 'Cambio de horario de salida por la huelga de autobuses',
        body: 'Por la huelga de autobuses convocada esta semana, la salida del día afectado se adelanta a las 14:00. Si no podéis recoger a vuestros hijos a esa hora, avisadme hoy mismo para organizar el comedor. Gracias por vuestra comprensión.',
      },
    ],
    reads: {
      calzado: ['tomas', 'marcos', 'beatriz', 'raul', 'marta', 'andres', 'rosa'],
      reunion: ['tomas', 'marcos', 'beatriz', 'rosa'],
      prado: ['tomas', 'beatriz'],
      huelga: ['tomas', 'marcos', 'elena', 'beatriz', 'raul', 'marta'],
    },
    lostItems: [
      { key: 'chaqueta', by: 'marcos', status: 'Lost', description: 'Chaqueta azul marino con capucha, talla 8, con el nombre «Sara» escrito en la etiqueta.' },
      { key: 'botella', by: 'beatriz', status: 'Lost', description: 'Botella de agua de acero inoxidable de color verde, con una pegatina de un cohete.' },
      { key: 'estuche', by: 'andres', status: 'Found', description: 'Estuche con dibujos de unicornios encontrado en el patio después del recreo.' },
      { key: 'jersey', by: 'raul', status: 'Lost', recovered: true, description: 'Jersey gris del uniforme con el nombre «Lola» bordado en el cuello.' },
    ],
    assignments: [
      { key: 'multiplicaciones', subject: 'Matemáticas', by: 'lucia', offset: 1, title: 'Ejercicios de multiplicaciones de la página 54', description: 'Hacer los ejercicios del 1 al 8. El 9 y el 10 son voluntarios.' },
      { key: 'repaso', subject: 'Matemáticas', by: 'raul', offset: 2, title: 'Repasar la tabla del 7 y la del 8', description: 'Propuesta: repasarlas en voz alta con un adulto antes del examen.' },
      { key: 'lectura', subject: 'Lengua', by: 'lucia', offset: 3, title: 'Leer el capítulo 3 de «Manolito Gafotas» y hacer un resumen', description: 'Resumen de cinco líneas en el cuaderno de Lengua.' },
      { key: 'agua', subject: 'Ciencias Naturales', by: 'lucia', offset: 5, title: 'Dibujar el ciclo del agua en una cartulina', description: 'Con flechas y los nombres de cada fase: evaporación, condensación, precipitación y recogida.' },
      { key: 'vocabulario', subject: 'Inglés', by: 'lucia', offset: 7, title: 'Aprender el vocabulario de los animales de la granja', description: 'Unit 2: tienen que saber escribirlas y decirlas en voz alta.' },
      { key: 'atletismo', subject: 'Educación Física', by: 'beatriz', offset: 8, title: 'Traer chándal y botella de agua para la jornada de atletismo', description: 'Propuesta: llevar también una gorra por si hace sol.' },
    ],
    comments: [
      { assignment: 'multiplicaciones', by: 'marcos', text: '¿Son los ejercicios del 1 al 8?' },
      { assignment: 'multiplicaciones', by: 'lucia', text: 'Sí, del 1 al 8. El 9 y el 10 son voluntarios.' },
      { assignment: 'lectura', by: 'tomas', text: 'Ya lo hemos leído esta tarde, ¡a Nora le ha encantado!' },
    ],
    done: {
      multiplicaciones: ['tomas', 'marcos', 'beatriz'],
      repaso: ['tomas'],
      lectura: ['tomas', 'marta'],
      agua: ['tomas'],
      vocabulario: ['tomas'],
      atletismo: ['tomas'],
    },
    expenses: [
      {
        key: 'prado',
        item: 'Excursión al Museo del Prado (entrada y autobús)',
        amount: 112,
        event: 'prado',
        participants: ['lucia', 'marcos', 'elena', 'tomas', 'beatriz', 'raul', 'marta', 'andres'],
        paid: ['lucia', 'marcos', 'tomas', 'beatriz', 'marta'],
      },
      {
        key: 'regalo',
        item: 'Regalo de fin de trimestre para la maestra',
        amount: 40,
        participants: ['lucia', 'marcos', 'elena', 'tomas', 'beatriz', 'raul', 'marta', 'andres', 'rosa'],
        paid: ['lucia', 'tomas', 'rosa', 'andres'],
      },
    ],
  },
  aravaca4b: {
    events: [
      { key: 'biblioteca', type: 'Activity', title: 'Visita a la biblioteca municipal', offset: 4, time: '11:00:00', location: 'Biblioteca Municipal de Aravaca' },
      { key: 'tutoria', type: 'Meeting', title: 'Tutoría grupal con las familias', offset: 8, time: '17:00:00', location: 'Aula de 4ºB' },
    ],
    birthdays: [
      { key: 'julia', child: 'julia', by: 'javier', offset: 3 },
      { key: 'alba', child: 'alba', by: 'elena', offset: 9 },
    ],
    announcements: [
      {
        key: 'rios',
        priority: 'Normal',
        title: 'Entrega del proyecto sobre los ríos de España',
        body: 'Recordad que el proyecto sobre los ríos de España se entrega la semana que viene. Tiene que incluir un mapa, el nombre de los cinco ríos principales y una frase sobre cada uno.',
      },
      {
        key: 'aula',
        priority: 'High',
        title: 'Cambio de aula por las obras del pasillo',
        body: 'Mientras duren las obras del pasillo, 4ºB da clase en el aula de música, en la planta baja. Los niños entran por la puerta del patio. Avisaremos cuando podamos volver a nuestra aula.',
      },
    ],
    reads: { rios: ['patricia', 'sergio'], aula: ['patricia'] },
    lostItems: [{ key: 'gorra', by: 'patricia', status: 'Lost', description: 'Gorra roja con el nombre «Gonzalo» en la visera.' }],
    assignments: [
      { key: 'fracciones', subject: 'Matemáticas', by: 'javier', offset: 2, title: 'Problemas de fracciones de la página 61', description: 'Los problemas del 3 al 9.' },
      { key: 'rios', subject: 'Ciencias Sociales', by: 'javier', offset: 4, title: 'Esquema de los ríos de la península ibérica', description: 'Con el nacimiento y la desembocadura de cada río.' },
      { key: 'weekend', subject: 'Inglés', by: 'sergio', offset: 6, title: 'Redacción «My weekend» en cinco frases', description: 'Propuesta: que cada alumno la lea en voz alta en clase.' },
    ],
    comments: [{ assignment: 'rios', by: 'cristina', text: '¿Hay que colorear el mapa o basta con el esquema?' }],
    done: { fracciones: ['patricia', 'javier'], rios: ['patricia'] },
    expenses: [
      {
        key: 'libro',
        item: 'Libro de lectura del trimestre',
        amount: 56,
        participants: ['javier', 'elena', 'patricia', 'sergio', 'cristina'],
        paid: ['javier', 'patricia'],
      },
    ],
  },
  sevilla2a: {
    events: [
      { key: 'alcazares', type: 'Trip', title: 'Visita a los Reales Alcázares', offset: 7, time: '10:30:00', location: 'Reales Alcázares de Sevilla', cost: '5 € por alumno' },
      { key: 'festival', type: 'Activity', title: 'Ensayo del festival de Navidad', offset: 10, time: '16:00:00', location: 'Gimnasio del colegio' },
    ],
    birthdays: [{ key: 'candela', child: 'candela', by: 'carmen', offset: 5 }],
    announcements: [
      {
        key: 'cuento',
        priority: 'Low',
        title: 'El cuento viajero ya está disponible',
        body: 'Cada semana un alumno se lleva a casa el cuento viajero. Lo podéis leer en familia y escribir una frase en la libreta que lo acompaña. Esta semana toca a Lucas.',
      },
      {
        key: 'alcazares',
        priority: 'Normal',
        event: 'alcazares',
        title: 'Visita a los Reales Alcázares: autorizaciones y 5 euros',
        body: 'Iremos a los Reales Alcázares en autobús. Necesito la autorización firmada y 5 euros por alumno. Los niños llevarán el almuerzo en la mochila y una gorra, por si hace calor.',
      },
    ],
    reads: { cuento: ['manuel', 'antonio'], alcazares: ['manuel'] },
    lostItems: [{ key: 'mochila', by: 'manuel', status: 'Lost', description: 'Mochila azul con un dibujo de un delfín y el nombre «Lucas» por dentro.' }],
    assignments: [
      { key: 'sumas', subject: 'Matemáticas', by: 'carmen', offset: 2, title: 'Ficha de sumas con llevadas', description: 'La ficha que se ha repartido en clase, las dos caras.' },
      { key: 'poesia', subject: 'Lengua', by: 'carmen', offset: 4, title: 'Aprender las dos primeras estrofas de la poesía de la primavera', description: 'La recitaremos en clase delante de los compañeros.' },
      { key: 'animales', subject: 'Conocimiento del Medio', by: 'inmaculada', offset: 6, title: 'Recortar fotos de animales de Doñana para el mural', description: 'Propuesta: traer tres fotos por alumno.' },
    ],
    comments: [{ assignment: 'poesia', by: 'antonio', text: 'Mateo ya se la sabe entera, ¡hay que frenarle!' }],
    done: { sumas: ['carmen', 'manuel'], poesia: ['manuel'] },
    expenses: [
      {
        key: 'alcazares',
        item: 'Entrada a los Reales Alcázares',
        amount: 20,
        event: 'alcazares',
        participants: ['carmen', 'manuel', 'inmaculada', 'antonio'],
        paid: ['carmen', 'manuel'],
      },
    ],
  },
};

export const ids = {
  user: (key) => PEOPLE[key].id,
  student: (key) => uid(`student:${key}`),
  enrollment: (key) => uid(`enrollment:${key}`),
  year: (schoolKey) => uid(`year:${schoolKey}`),
  class: (key) => uid(`class:${key}`),
  subject: (classKey, label) => uid(`subject:${classKey}:${label}`),
  event: (classKey, key) => uid(`event:${classKey}:${key}`),
  birthday: (classKey, key) => uid(`birthday:${classKey}:${key}`),
  announcement: (classKey, key) => uid(`announcement:${classKey}:${key}`),
  lostItem: (classKey, key) => uid(`lost:${classKey}:${key}`),
  assignment: (classKey, key) => uid(`assignment:${classKey}:${key}`),
  expense: (classKey, key) => uid(`expense:${classKey}:${key}`),
};

export function allDemoIds() {
  const result = {
    users: Object.keys(PEOPLE).map(ids.user),
    students: ALL_CHILDREN.map((child) => ids.student(child.key)),
    enrollments: ALL_CHILDREN.map((child) => ids.enrollment(child.key)),
    years: Object.keys(SCHOOLS).map(ids.year),
    classes: Object.keys(CLASSES).map(ids.class),
    subjects: [],
    events: [],
    announcements: [],
    lostItems: [],
    assignments: [],
    expenses: [],
  };
  for (const [classKey, definition] of Object.entries(CLASSES)) {
    for (const label of definition.subjects) result.subjects.push(ids.subject(classKey, label));
    const content = CONTENT[classKey];
    for (const item of content.events) result.events.push(ids.event(classKey, item.key));
    for (const item of content.birthdays) result.events.push(ids.birthday(classKey, item.key));
    for (const item of content.announcements) result.announcements.push(ids.announcement(classKey, item.key));
    for (const item of content.lostItems) result.lostItems.push(ids.lostItem(classKey, item.key));
    for (const item of content.assignments) result.assignments.push(ids.assignment(classKey, item.key));
    for (const item of content.expenses) result.expenses.push(ids.expense(classKey, item.key));
  }
  return result;
}
