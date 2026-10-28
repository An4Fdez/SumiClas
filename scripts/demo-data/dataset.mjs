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

// Content of every class. Every offset is a number of days from the anchor day (--day, D). The window the content
// covers is D-6 .. D+14, so the Monday-Sunday week that contains D (whatever weekday D is) and the following week both
// have material for the weekly summary. Announcements, lost items and expenses also carry `offset` (and `at`, the UTC
// time of day, for announcements): it is the planned creation date, applied directly in the database because the API
// stamps them with the moment of loading. Announcements are listed in creation order. The text of the items avoids
// weekdays and calendar dates, so it stays true when the whole demo is moved to a new anchor day.
export const CONTENT = {
  aravaca3a: {
    events: [
      { key: 'prado', type: 'Trip', title: 'Excursión al Museo del Prado', offset: 6, time: '10:00:00', location: 'Museo Nacional del Prado (Madrid)', cost: '8 € por alumno' },
      { key: 'reunion', type: 'Meeting', title: 'Reunión de padres del primer trimestre', offset: 3, time: '17:30:00', location: 'Aula de 3ºA' },
      { key: 'reciclaje', type: 'Activity', title: 'Taller de reciclaje con material de casa', offset: 9, time: '12:00:00', location: 'Patio del colegio' },
      { key: 'fotos', type: 'Other', title: 'Fotografía de clase', offset: 12, time: '09:30:00', location: 'Salón de actos' },
      { key: 'charla', type: 'Activity', title: 'Charla sobre alimentación saludable', offset: -4, time: '10:00:00', location: 'Salón de actos' },
      { key: 'otono', type: 'Activity', title: 'Fiesta de otoño con disfraces', offset: 2, time: '11:30:00', location: 'Patio del colegio' },
      { key: 'alimentos', type: 'Other', title: 'Recogida de alimentos para el banco de alimentos', offset: 7, time: '09:00:00', location: 'Entrada principal del colegio' },
      { key: 'teatro', type: 'Trip', title: 'Salida al teatro infantil', offset: 14, time: '10:15:00', location: 'Teatro Municipal de Aravaca', cost: '6 € por alumno' },
    ],
    birthdays: [
      { key: 'hugo', child: 'hugo', by: 'lucia', offset: 2 },
      { key: 'sara', child: 'sara', by: 'marcos', offset: 4 },
      { key: 'nora', child: 'nora', by: 'tomas', offset: 10 },
      { key: 'lola', child: 'lola', by: 'raul', offset: 15 },
      { key: 'pablo', child: 'pablo', by: 'marcos', offset: 4 },
      { key: 'iker', child: 'iker', by: 'marta', offset: -5 },
      { key: 'valeria', child: 'valeria', by: 'andres', offset: -3 },
      { key: 'daniel', child: 'daniel', by: 'elena', offset: 8 },
      { key: 'leo', child: 'leo', by: 'beatriz', offset: 12 },
      { key: 'mario', child: 'mario', by: 'rosa', offset: 13 },
    ],
    announcements: [
      {
        key: 'calzado',
        priority: 'Low',
        offset: -5,
        at: '09:10',
        title: 'Recordatorio: calzado deportivo los martes y jueves',
        body: 'Os recuerdo que los martes y los jueves tenemos Educación Física a primera hora. Los niños tienen que venir con zapatillas de deporte y con ropa cómoda. Gracias por vuestra ayuda.',
      },
      {
        key: 'reunion',
        priority: 'Normal',
        event: 'reunion',
        offset: -3,
        at: '08:40',
        title: 'Reunión de padres del primer trimestre',
        body: 'Queridas familias: os convoco a la reunión del primer trimestre en el aula de 3ºA. Hablaremos del ritmo de la clase, de las excursiones previstas y de cómo ayudar en casa con los deberes. Es importante que asista al menos un adulto por familia.',
      },
      {
        key: 'prado',
        priority: 'Normal',
        event: 'prado',
        offset: -1,
        at: '18:20',
        title: 'Excursión al Museo del Prado: autorización y 8 euros',
        body: 'Vamos a visitar el Museo del Prado. Salimos a las 10:00 y volvemos para la hora de comer. Necesito la autorización firmada y 8 euros por alumno cuanto antes. El gasto está en la sección Gastos para que cada familia marque su pago.',
      },
      {
        key: 'huelga',
        priority: 'High',
        offset: 0,
        at: '07:50',
        title: 'Cambio de horario de salida por la huelga de autobuses',
        body: 'Por la huelga de autobuses convocada esta semana, la salida del día afectado se adelanta a las 14:00. Si no podéis recoger a vuestros hijos a esa hora, avisadme hoy mismo para organizar el comedor. Gracias por vuestra comprensión.',
      },
      {
        key: 'agenda',
        priority: 'Normal',
        offset: 0,
        at: '12:00',
        title: 'Agenda de las próximas dos semanas',
        body: 'Os resumo lo que viene: fiesta de otoño con disfraces, reunión de padres, excursión al Prado, taller de reciclaje, recogida de alimentos y fotografía de clase. Tenéis las fechas en el calendario y los gastos en su sección.',
      },
      {
        key: 'otono',
        priority: 'Normal',
        event: 'otono',
        offset: 1,
        at: '17:45',
        title: 'Fiesta de otoño: venid disfrazados',
        body: 'Vamos a celebrar una fiesta de otoño en el patio. Los niños pueden venir disfrazados desde casa, sin armas ni máscaras que tapen la cara. Habrá castañas y música; no hace falta traer nada más.',
      },
      {
        key: 'alimentos',
        priority: 'Normal',
        event: 'alimentos',
        offset: 3,
        at: '19:05',
        title: 'Recogida de alimentos: qué podemos aportar',
        body: 'Organizamos una recogida solidaria. Se aceptan legumbres, pasta, arroz, aceite y conservas, siempre sin abrir y con la fecha de caducidad en vigor. Los dejaremos en una caja a la entrada del colegio.',
      },
      {
        key: 'piojos',
        priority: 'High',
        offset: 4,
        at: '08:15',
        title: 'Aviso: se han detectado casos de piojos en el colegio',
        body: 'Hemos recibido el aviso del centro. Os pido que reviséis la cabeza de vuestros hijos este fin de semana y que, si hace falta, apliquéis el tratamiento. Los niños con el pelo largo deben venir con coleta o trenza.',
      },
      {
        key: 'navidad',
        priority: 'Normal',
        offset: 9,
        at: '18:30',
        title: 'Preparamos el festival de Navidad',
        body: 'Empezamos a organizar el festival de Navidad. Necesito dos o tres familias voluntarias para ayudar con el vestuario y la decoración. Si podéis echar una mano, decídmelo por aquí.',
      },
      {
        key: 'teatro',
        priority: 'Normal',
        event: 'teatro',
        offset: 11,
        at: '08:50',
        title: 'Salida al teatro: autorización y 6 euros',
        body: 'Iremos a ver una función infantil. Necesito la autorización firmada y 6 euros por alumno. El gasto ya está en la sección Gastos para que cada familia marque su pago.',
      },
      {
        key: 'regalo',
        priority: 'Low',
        offset: 13,
        at: '17:20',
        title: 'Regalo de Navidad para la maestra',
        body: 'Como cada año, queremos hacerle un detalle a la maestra por Navidad. Propongo una aportación pequeña por familia; quien quiera colaborar con una idea para el regalo puede escribirme.',
      },
    ],
    reads: {
      calzado: ['tomas', 'marcos', 'beatriz', 'raul', 'marta', 'andres', 'rosa'],
      reunion: ['tomas', 'marcos', 'beatriz', 'rosa', 'elena', 'raul', 'marta'],
      prado: ['tomas', 'beatriz'],
      huelga: ['tomas', 'marcos', 'elena', 'beatriz', 'raul', 'marta'],
      agenda: ['tomas', 'beatriz', 'marcos'],
      otono: ['tomas'],
    },
    lostItems: [
      { key: 'chaqueta', by: 'marcos', status: 'Lost', offset: -6, description: 'Chaqueta azul marino con capucha, talla 8, con el nombre «Sara» escrito en la etiqueta.' },
      { key: 'botella', by: 'beatriz', status: 'Lost', offset: -4, description: 'Botella de agua de acero inoxidable de color verde, con una pegatina de un cohete.' },
      { key: 'estuche', by: 'andres', status: 'Found', offset: -3, description: 'Estuche con dibujos de unicornios encontrado en el patio después del recreo.' },
      { key: 'jersey', by: 'raul', status: 'Lost', recovered: true, offset: -5, description: 'Jersey gris del uniforme con el nombre «Lola» bordado en el cuello.' },
      { key: 'sudadera', by: 'raul', status: 'Lost', offset: -2, description: 'Sudadera roja con capucha, talla 9, perdida después de la clase de Educación Física.' },
      { key: 'gafas', by: 'marta', status: 'Found', offset: -1, description: 'Gafas de sol infantiles de montura verde encontradas en el banco del patio.' },
      { key: 'fiambrera', by: 'andres', status: 'Lost', recovered: true, offset: -3, description: 'Fiambrera azul con un dibujo de dinosaurios y cierre de rosca.' },
    ],
    assignments: [
      { key: 'multiplicaciones', subject: 'Matemáticas', by: 'lucia', offset: 1, title: 'Ejercicios de multiplicaciones de la página 54', description: 'Hacer los ejercicios del 1 al 8. El 9 y el 10 son voluntarios.' },
      { key: 'repaso', subject: 'Matemáticas', by: 'raul', offset: 2, title: 'Repasar la tabla del 7 y la del 8', description: 'Propuesta: repasarlas en voz alta con un adulto antes del examen.' },
      { key: 'lectura', subject: 'Lengua', by: 'lucia', offset: 3, title: 'Leer el capítulo 3 de «Manolito Gafotas» y hacer un resumen', description: 'Resumen de cinco líneas en el cuaderno de Lengua.' },
      { key: 'agua', subject: 'Ciencias Naturales', by: 'lucia', offset: 5, title: 'Dibujar el ciclo del agua en una cartulina', description: 'Con flechas y los nombres de cada fase: evaporación, condensación, precipitación y recogida.' },
      { key: 'vocabulario', subject: 'Inglés', by: 'lucia', offset: 7, title: 'Aprender el vocabulario de los animales de la granja', description: 'Unit 2: tienen que saber escribirlas y decirlas en voz alta.' },
      { key: 'atletismo', subject: 'Educación Física', by: 'beatriz', offset: 8, title: 'Traer chándal y botella de agua para la jornada de atletismo', description: 'Propuesta: llevar también una gorra por si hace sol.' },
      { key: 'dictado', subject: 'Lengua', by: 'lucia', offset: -2, title: 'Preparar el dictado de palabras con b y v', description: 'Diez palabras que hemos trabajado en clase, para estudiar en el cuaderno.' },
      { key: 'division', subject: 'Matemáticas', by: 'lucia', offset: 10, title: 'Problemas de división de la página 70', description: 'Los problemas del 1 al 6, con la prueba de cada división.' },
      { key: 'volcan', subject: 'Ciencias Naturales', by: 'marta', offset: 11, title: 'Maqueta de un volcán con material reciclado', description: 'Propuesta: hacerla por parejas y traerla para exponerla en clase.' },
      { key: 'cancion', subject: 'Inglés', by: 'lucia', offset: 13, title: 'Aprender la canción «Head, Shoulders, Knees and Toes»', description: 'La cantaremos en clase todos juntos.' },
    ],
    comments: [
      { assignment: 'multiplicaciones', by: 'marcos', text: '¿Son los ejercicios del 1 al 8?' },
      { assignment: 'multiplicaciones', by: 'lucia', text: 'Sí, del 1 al 8. El 9 y el 10 son voluntarios.' },
      { assignment: 'lectura', by: 'tomas', text: 'Ya lo hemos leído esta tarde, ¡a Nora le ha encantado!' },
      { assignment: 'dictado', by: 'beatriz', text: '¿Las palabras están en la fotocopia que repartisteis?' },
      { assignment: 'volcan', by: 'lucia', text: 'Me parece una idea estupenda. Os confirmo la fecha de exposición en cuanto la tenga.' },
    ],
    done: {
      multiplicaciones: ['tomas', 'marcos', 'beatriz'],
      repaso: ['tomas'],
      lectura: ['tomas', 'marta'],
      agua: ['tomas'],
      vocabulario: ['tomas'],
      atletismo: ['tomas'],
      dictado: ['tomas', 'marta', 'beatriz', 'andres'],
    },
    expenses: [
      {
        key: 'prado',
        item: 'Excursión al Museo del Prado (entrada y autobús)',
        amount: 112,
        offset: -1,
        event: 'prado',
        participants: ['lucia', 'marcos', 'elena', 'tomas', 'beatriz', 'raul', 'marta', 'andres'],
        paid: ['lucia', 'marcos', 'tomas', 'beatriz', 'marta'],
      },
      {
        key: 'regalo',
        item: 'Regalo de fin de trimestre para la maestra',
        amount: 40,
        offset: -2,
        participants: ['lucia', 'marcos', 'elena', 'tomas', 'beatriz', 'raul', 'marta', 'andres', 'rosa'],
        paid: ['lucia', 'tomas', 'rosa', 'andres'],
      },
      {
        key: 'material',
        item: 'Material de plástica para el taller de reciclaje',
        amount: 18,
        offset: -4,
        event: 'reciclaje',
        participants: ['lucia', 'marcos', 'elena', 'tomas', 'beatriz', 'raul', 'marta', 'andres', 'rosa'],
        paid: ['lucia', 'marcos', 'elena', 'tomas', 'beatriz', 'marta', 'rosa'],
      },
      {
        key: 'teatro',
        item: 'Entrada a la función de teatro infantil',
        amount: 54,
        offset: 0,
        event: 'teatro',
        participants: ['lucia', 'marcos', 'elena', 'tomas', 'beatriz', 'raul', 'marta', 'andres', 'rosa'],
        paid: ['lucia', 'tomas'],
      },
    ],
  },
  aravaca4b: {
    events: [
      { key: 'biblioteca', type: 'Activity', title: 'Visita a la biblioteca municipal', offset: 4, time: '11:00:00', location: 'Biblioteca Municipal de Aravaca' },
      { key: 'tutoria', type: 'Meeting', title: 'Tutoría grupal con las familias', offset: 8, time: '17:00:00', location: 'Aula de 4ºB' },
      { key: 'concierto', type: 'Activity', title: 'Concierto didáctico en el auditorio', offset: -2, time: '11:00:00', location: 'Auditorio del colegio' },
      { key: 'fotos', type: 'Other', title: 'Fotografía escolar individual y de grupo', offset: 5, time: '09:15:00', location: 'Salón de actos' },
      { key: 'museo', type: 'Trip', title: 'Visita al Museo de Ciencias Naturales', offset: 11, time: '10:00:00', location: 'Museo Nacional de Ciencias Naturales (Madrid)', cost: '7 € por alumno' },
      { key: 'coro', type: 'Activity', title: 'Ensayo de villancicos con el coro', offset: 14, time: '12:00:00', location: 'Aula de música' },
    ],
    birthdays: [
      { key: 'julia', child: 'julia', by: 'javier', offset: 3 },
      { key: 'alba', child: 'alba', by: 'elena', offset: 9 },
      { key: 'gonzalo', child: 'gonzalo', by: 'patricia', offset: 6 },
      { key: 'noa', child: 'noa', by: 'sergio', offset: -2 },
      { key: 'adrian', child: 'adrian', by: 'cristina', offset: 12 },
    ],
    announcements: [
      {
        key: 'aula',
        priority: 'High',
        offset: -4,
        at: '08:05',
        title: 'Cambio de aula por las obras del pasillo',
        body: 'Mientras duren las obras del pasillo, 4ºB da clase en el aula de música, en la planta baja. Los niños entran por la puerta del patio. Avisaremos cuando podamos volver a nuestra aula.',
      },
      {
        key: 'concierto',
        priority: 'Normal',
        event: 'concierto',
        offset: -3,
        at: '17:40',
        title: 'Concierto didáctico: no hace falta traer nada',
        body: 'La clase asistirá a un concierto didáctico en el auditorio del colegio. Los niños van con el uniforme de siempre y no tienen que traer nada. Después haremos una actividad en clase sobre los instrumentos.',
      },
      {
        key: 'rios',
        priority: 'Normal',
        offset: -1,
        at: '09:30',
        title: 'Entrega del proyecto sobre los ríos de España',
        body: 'Recordad que el proyecto sobre los ríos de España se entrega la semana que viene. Tiene que incluir un mapa, el nombre de los cinco ríos principales y una frase sobre cada uno.',
      },
      {
        key: 'fotos',
        priority: 'Normal',
        event: 'fotos',
        offset: 2,
        at: '18:10',
        title: 'Fotografía escolar: cómo venir ese día',
        body: 'Habrá fotografía individual y de grupo. Los niños vendrán peinados y con el uniforme completo. El pack con las copias es opcional y se paga desde la sección Gastos.',
      },
      {
        key: 'calefaccion',
        priority: 'High',
        offset: 3,
        at: '07:55',
        title: 'Sin calefacción en el aula: traed una chaqueta',
        body: 'La caldera del edificio está averiada y el aula está más fría de lo normal. Hasta que la arreglen, los niños deben venir con una chaqueta o un jersey que se puedan dejar puestos en clase.',
      },
      {
        key: 'tutoria',
        priority: 'Normal',
        event: 'tutoria',
        offset: 6,
        at: '19:00',
        title: 'Tutoría grupal con las familias',
        body: 'Os convoco a una tutoría grupal para comentar cómo va el trimestre, la organización de los deberes y las próximas salidas. Es importante que venga al menos un adulto por familia.',
      },
      {
        key: 'museo',
        priority: 'Normal',
        event: 'museo',
        offset: 8,
        at: '08:30',
        title: 'Visita al Museo de Ciencias Naturales: autorización y 7 euros',
        body: 'Visitaremos el museo con un taller guiado. Necesito la autorización firmada y 7 euros por alumno. Llevarán el almuerzo en la mochila y una botella de agua.',
      },
      {
        key: 'coro',
        priority: 'Normal',
        event: 'coro',
        offset: 13,
        at: '18:05',
        title: 'Villancicos: ensayo del coro de la clase',
        body: 'Estamos preparando unos villancicos para el festival. El ensayo es en el aula de música y los niños solo tienen que venir con ganas de cantar. Podéis repasarlos en casa con ellos.',
      },
      {
        key: 'menu',
        priority: 'Low',
        offset: 11,
        at: '17:35',
        title: 'Nuevo menú del comedor para el próximo mes',
        body: 'El comedor ha publicado el menú del próximo mes. Lo tenéis en la entrada del colegio. Si algún niño tiene alergias o una dieta especial, avisadme para trasladarlo a la cocina.',
      },
    ],
    reads: {
      aula: ['patricia', 'sergio', 'cristina', 'elena'],
      concierto: ['patricia', 'sergio', 'cristina', 'elena'],
      rios: ['patricia', 'sergio', 'elena'],
      fotos: ['patricia'],
    },
    lostItems: [
      { key: 'gorra', by: 'patricia', status: 'Lost', offset: -3, description: 'Gorra roja con el nombre «Gonzalo» en la visera.' },
      { key: 'sudadera', by: 'sergio', status: 'Lost', offset: -2, description: 'Sudadera gris de talla 10 con un balón de fútbol dibujado en el pecho.' },
      { key: 'flauta', by: 'cristina', status: 'Found', offset: -1, description: 'Flauta dulce con funda de tela, encontrada en el aula de música.' },
      { key: 'bufanda', by: 'elena', status: 'Lost', recovered: true, offset: -5, description: 'Bufanda de lana con rayas rojas y blancas.' },
    ],
    assignments: [
      { key: 'fracciones', subject: 'Matemáticas', by: 'javier', offset: 2, title: 'Problemas de fracciones de la página 61', description: 'Los problemas del 3 al 9.' },
      { key: 'rios', subject: 'Ciencias Sociales', by: 'javier', offset: 4, title: 'Esquema de los ríos de la península ibérica', description: 'Con el nacimiento y la desembocadura de cada río.' },
      { key: 'weekend', subject: 'Inglés', by: 'sergio', offset: 6, title: 'Redacción «My weekend» en cinco frases', description: 'Propuesta: que cada alumno la lea en voz alta en clase.' },
      { key: 'resumen', subject: 'Lengua', by: 'javier', offset: -3, title: 'Resumen del cuento que hemos leído en clase', description: 'Diez líneas en el cuaderno, con el principio, el nudo y el desenlace.' },
      { key: 'presentacion', subject: 'Inglés', by: 'javier', offset: 9, title: 'Preparar la presentación oral «My favourite animal»', description: 'Un minuto, con un dibujo o una foto del animal.' },
      { key: 'flauta', subject: 'Música', by: 'cristina', offset: 11, title: 'Practicar la melodía de la flauta de la unidad 2', description: 'Propuesta: tocarla una vez al día en casa, aunque sea cinco minutos.' },
      { key: 'geometria', subject: 'Matemáticas', by: 'javier', offset: 13, title: 'Problemas de geometría de la página 75', description: 'Perímetros y áreas, los problemas del 1 al 7.' },
    ],
    comments: [
      { assignment: 'rios', by: 'cristina', text: '¿Hay que colorear el mapa o basta con el esquema?' },
      { assignment: 'presentacion', by: 'patricia', text: '¿Cuánto tiene que durar cada presentación?' },
      { assignment: 'presentacion', by: 'javier', text: 'Un minuto como máximo, para que podamos escuchar a todos.' },
    ],
    done: { fracciones: ['patricia', 'javier'], rios: ['patricia'], resumen: ['patricia', 'sergio', 'javier'] },
    expenses: [
      {
        key: 'libro',
        item: 'Libro de lectura del trimestre',
        amount: 56,
        offset: -4,
        participants: ['javier', 'elena', 'patricia', 'sergio', 'cristina'],
        paid: ['javier', 'patricia'],
      },
      {
        key: 'fotos',
        item: 'Pack de fotografías escolares',
        amount: 60,
        offset: -1,
        event: 'fotos',
        participants: ['javier', 'elena', 'patricia', 'sergio', 'cristina'],
        paid: ['javier', 'elena', 'sergio'],
      },
      {
        key: 'museo',
        item: 'Entrada y taller del Museo de Ciencias Naturales',
        amount: 35,
        offset: 0,
        event: 'museo',
        participants: ['javier', 'elena', 'patricia', 'sergio', 'cristina'],
        paid: ['javier'],
      },
    ],
  },
  sevilla2a: {
    events: [
      { key: 'alcazares', type: 'Trip', title: 'Visita a los Reales Alcázares', offset: 7, time: '10:30:00', location: 'Reales Alcázares de Sevilla', cost: '5 € por alumno' },
      { key: 'festival', type: 'Activity', title: 'Ensayo del festival de Navidad', offset: 10, time: '16:00:00', location: 'Gimnasio del colegio' },
      { key: 'castanada', type: 'Activity', title: 'Castañada en el patio', offset: -2, time: '10:30:00', location: 'Patio del colegio' },
      { key: 'tutoria', type: 'Meeting', title: 'Tutoría con la profesora de 2ºA', offset: 3, time: '17:00:00', location: 'Aula de 2ºA' },
      { key: 'taller', type: 'Activity', title: 'Taller de cocina: hacemos pan', offset: 12, time: '11:00:00', location: 'Comedor del colegio' },
    ],
    birthdays: [
      { key: 'candela', child: 'candela', by: 'carmen', offset: 5 },
      { key: 'lucas', child: 'lucas', by: 'manuel', offset: 1 },
      { key: 'rocio', child: 'rocio', by: 'inmaculada', offset: 8 },
      { key: 'mateo', child: 'mateo', by: 'antonio', offset: 11 },
    ],
    announcements: [
      {
        key: 'cuento',
        priority: 'Low',
        offset: -4,
        at: '09:20',
        title: 'El cuento viajero ya está disponible',
        body: 'Cada semana un alumno se lleva a casa el cuento viajero. Lo podéis leer en familia y escribir una frase en la libreta que lo acompaña. Esta semana toca a Lucas.',
      },
      {
        key: 'castanada',
        priority: 'Normal',
        event: 'castanada',
        offset: -3,
        at: '18:00',
        title: 'Castañada en el patio: qué traer',
        body: 'Haremos una castañada con todos los niños de 2ºA. Cada familia puede aportar un paquete de castañas ya asadas o un dulce casero, sin frutos secos por posibles alergias. Gracias por colaborar.',
      },
      {
        key: 'alcazares',
        priority: 'Normal',
        event: 'alcazares',
        offset: -1,
        at: '08:45',
        title: 'Visita a los Reales Alcázares: autorizaciones y 5 euros',
        body: 'Iremos a los Reales Alcázares en autobús. Necesito la autorización firmada y 5 euros por alumno. Los niños llevarán el almuerzo en la mochila y una gorra, por si hace calor.',
      },
      {
        key: 'tutoria',
        priority: 'Normal',
        event: 'tutoria',
        offset: 1,
        at: '17:30',
        title: 'Tutorías con la profesora: pedid hora',
        body: 'La profesora ofrece tutorías individuales para hablar de cómo va cada niño. Si queréis hablar con ella, decídmelo y os apunto en el primer hueco libre.',
      },
      {
        key: 'lluvia',
        priority: 'High',
        offset: 4,
        at: '07:40',
        title: 'Si llueve, el recreo se hace dentro',
        body: 'Con las lluvias previstas, el recreo se hará en el gimnasio. Los niños deben traer un chubasquero o paraguas pequeño con su nombre y una muda de calcetines en la mochila.',
      },
      {
        key: 'biblioteca',
        priority: 'Low',
        offset: 6,
        at: '18:15',
        title: 'Biblioteca de aula: libros para llevar a casa',
        body: 'Ya podemos llevarnos libros de la biblioteca de aula. Cada niño elige uno por semana y lo devuelve al final de la semana. Os pido que los cuidéis y los forréis si es posible.',
      },
      {
        key: 'festival',
        priority: 'Normal',
        event: 'festival',
        offset: 8,
        at: '17:50',
        title: 'Festival de Navidad: disfraces y ensayo',
        body: 'Estamos preparando el festival de Navidad. Cada niño necesitará un disfraz sencillo que os iremos detallando. Os pido que mientras tanto repaséis con ellos las canciones que traen anotadas en la libreta.',
      },
      {
        key: 'taller',
        priority: 'Low',
        event: 'taller',
        offset: 10,
        at: '09:00',
        title: 'Taller de pan: traed un delantal',
        body: 'Para el taller de cocina cada niño tiene que traer un delantal y un paño pequeño. Haremos pan con harina y levadura; si hay alguna alergia o intolerancia, avisadme antes.',
      },
      {
        key: 'boletines',
        priority: 'Normal',
        offset: 13,
        at: '18:25',
        title: 'Entrega de notas del primer trimestre',
        body: 'Se acerca la entrega de notas. Os las daremos a los adultos en mano, en una reunión corta. Más adelante os confirmo el horario por este mismo canal.',
      },
    ],
    reads: {
      cuento: ['manuel', 'antonio', 'inmaculada'],
      castanada: ['manuel', 'antonio', 'inmaculada'],
      alcazares: ['manuel', 'antonio'],
    },
    lostItems: [
      { key: 'mochila', by: 'manuel', status: 'Lost', offset: -4, description: 'Mochila azul con un dibujo de un delfín y el nombre «Lucas» por dentro.' },
      { key: 'abrigo', by: 'inmaculada', status: 'Lost', offset: -3, description: 'Abrigo verde de niño, talla 7, con el forro de cuadros.' },
      { key: 'bidon', by: 'antonio', status: 'Found', offset: -1, description: 'Bidón de plástico transparente con una pegatina de un coche de carreras.' },
      { key: 'pelota', by: 'carmen', status: 'Lost', recovered: true, offset: -2, description: 'Pelota de goma amarilla de las que se usan en el recreo.' },
    ],
    assignments: [
      { key: 'sumas', subject: 'Matemáticas', by: 'carmen', offset: 2, title: 'Ficha de sumas con llevadas', description: 'La ficha que se ha repartido en clase, las dos caras.' },
      { key: 'poesia', subject: 'Lengua', by: 'carmen', offset: 4, title: 'Aprender las dos primeras estrofas de la poesía de la primavera', description: 'La recitaremos en clase delante de los compañeros.' },
      { key: 'animales', subject: 'Conocimiento del Medio', by: 'inmaculada', offset: 6, title: 'Recortar fotos de animales de Doñana para el mural', description: 'Propuesta: traer tres fotos por alumno.' },
      { key: 'dictado', subject: 'Lengua', by: 'carmen', offset: -3, title: 'Estudiar las palabras del dictado', description: 'Ocho palabras con «ll» y «y» que hemos repasado en clase.' },
      { key: 'tabla', subject: 'Matemáticas', by: 'carmen', offset: 8, title: 'Aprender la tabla del 2 y la del 3', description: 'Hay que saberlas decir en orden y también salteadas.' },
      { key: 'hoja', subject: 'Conocimiento del Medio', by: 'antonio', offset: 10, title: 'Traer una hoja de árbol seca para clasificar', description: 'Propuesta: que cada niño traiga una de un árbol distinto y lo apunte en la libreta.' },
      { key: 'colores', subject: 'Inglés', by: 'carmen', offset: 12, title: 'Repasar los colores en inglés', description: 'Tienen que saber decir el color de cinco objetos de la clase.' },
    ],
    comments: [
      { assignment: 'poesia', by: 'antonio', text: 'Mateo ya se la sabe entera, ¡hay que frenarle!' },
      { assignment: 'hoja', by: 'inmaculada', text: '¿Vale cualquier árbol o tiene que ser de un parque concreto?' },
      { assignment: 'hoja', by: 'carmen', text: 'Vale cualquiera. Cuanto más distintas sean las hojas, mejor.' },
    ],
    done: { sumas: ['carmen', 'manuel'], poesia: ['manuel'], dictado: ['manuel', 'inmaculada', 'carmen'] },
    expenses: [
      {
        key: 'alcazares',
        item: 'Entrada a los Reales Alcázares',
        amount: 20,
        offset: -2,
        event: 'alcazares',
        participants: ['carmen', 'manuel', 'inmaculada', 'antonio'],
        paid: ['carmen', 'manuel'],
      },
      {
        key: 'castanada',
        item: 'Castañas y dulces para la castañada',
        amount: 24,
        offset: -4,
        event: 'castanada',
        participants: ['carmen', 'manuel', 'inmaculada', 'antonio'],
        paid: ['carmen', 'manuel', 'inmaculada'],
      },
      {
        key: 'festival',
        item: 'Material para el vestuario del festival de Navidad',
        amount: 32,
        offset: 0,
        event: 'festival',
        participants: ['carmen', 'manuel', 'inmaculada', 'antonio'],
        paid: ['carmen'],
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

// The sentinel is a demo announcement whose publication date is exactly the anchor day: a refresh script reads the
// current anchor from it (CreatedAt, as a UTC date) and moves every demo row by the difference to the new day.
export const ANCHOR_SENTINEL = { classKey: 'aravaca3a', key: 'agenda' };
export const anchorSentinelId = () => ids.announcement(ANCHOR_SENTINEL.classKey, ANCHOR_SENTINEL.key);

const DEFAULT_CREATION_TIME = '12:00';

// Planned creation moment (UTC) of an announcement, lost item or expense, for a SQL timestamp literal.
export function plannedCreatedAt(day, offset, at = DEFAULT_CREATION_TIME) {
  return `${addDays(day, offset)} ${at}:00+00`;
}

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

// Keys of the content added by the extension (the year-long content on top of the original base demo). The export of
// SQL files uses it to build a base-only subset and an additive file that complements it; it changes nothing else.
export const EXTENSION_KEYS = {
  aravaca3a: {
    events: ['charla', 'otono', 'alimentos', 'teatro'],
    birthdays: ['pablo', 'iker', 'valeria', 'daniel', 'leo', 'mario'],
    announcements: ['agenda', 'otono', 'alimentos', 'piojos', 'navidad', 'teatro', 'regalo'],
    lostItems: ['sudadera', 'gafas', 'fiambrera'],
    assignments: ['dictado', 'division', 'volcan', 'cancion'],
    expenses: ['material', 'teatro'],
  },
  aravaca4b: {
    events: ['concierto', 'fotos', 'museo', 'coro'],
    birthdays: ['gonzalo', 'noa', 'adrian'],
    announcements: ['concierto', 'fotos', 'calefaccion', 'tutoria', 'museo', 'coro', 'menu'],
    lostItems: ['sudadera', 'flauta', 'bufanda'],
    assignments: ['resumen', 'presentacion', 'flauta', 'geometria'],
    expenses: ['fotos', 'museo'],
  },
  sevilla2a: {
    events: ['castanada', 'tutoria', 'taller'],
    birthdays: ['lucas', 'rocio', 'mateo'],
    announcements: ['castanada', 'tutoria', 'lluvia', 'biblioteca', 'festival', 'taller', 'boletines'],
    lostItems: ['abrigo', 'bidon', 'pelota'],
    assignments: ['dictado', 'tabla', 'hoja', 'colores'],
    expenses: ['castanada', 'festival'],
  },
};

// Ids of the extension content only (same shape as allDemoIds, restricted to the kinds the extension adds).
export function extensionDemoIds() {
  const result = { events: [], announcements: [], lostItems: [], assignments: [], expenses: [] };
  for (const [classKey, keys] of Object.entries(EXTENSION_KEYS)) {
    for (const key of keys.events) result.events.push(ids.event(classKey, key));
    for (const key of keys.birthdays) result.events.push(ids.birthday(classKey, key));
    for (const key of keys.announcements) result.announcements.push(ids.announcement(classKey, key));
    for (const key of keys.lostItems) result.lostItems.push(ids.lostItem(classKey, key));
    for (const key of keys.assignments) result.assignments.push(ids.assignment(classKey, key));
    for (const key of keys.expenses) result.expenses.push(ids.expense(classKey, key));
  }
  return result;
}
