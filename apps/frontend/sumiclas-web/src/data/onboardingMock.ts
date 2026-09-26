export type School = {
  id: string;
  name: string;
  city: string;
  address: string;
};

export type SchoolClass = {
  id: string;
  schoolId: string;
  name: string;
};

export type RegisteredStudent = {
  id: string;
  classId: string;
  name: string;
  lastName: string;
};

export const schoolsMock: School[] = [
  { id: 'col-1', name: 'CEIP Miguel de Cervantes', city: 'Madrid', address: 'Calle Mayor, 12' },
  { id: 'col-2', name: 'CEIP Miguel de Cervantes', city: 'Alcalá de Henares', address: 'Av. Complutense, 5' },
  { id: 'col-3', name: 'Colegio San José', city: 'Valencia', address: 'Calle Colón, 44' },
  { id: 'col-4', name: 'Colegio San José', city: 'Bilbao', address: 'Gran Vía, 8' },
  { id: 'col-5', name: 'CEIP Federico García Lorca', city: 'Madrid', address: 'Paseo de la Castellana, 210' },
  { id: 'col-6', name: 'IES Federico García Lorca', city: 'Granada', address: 'Camino de Ronda, 90' },
  { id: 'col-7', name: 'CEIP Pablo Picasso', city: 'Málaga', address: 'Calle Larios, 3' },
  { id: 'col-8', name: 'Colegio Santa Teresa', city: 'Zaragoza', address: 'Paseo Independencia, 15' },
  { id: 'col-9', name: 'CEIP Antonio Machado', city: 'Madrid', address: 'Calle Alcalá, 155' },
  { id: 'col-10', name: 'IES Antonio Machado', city: 'Sevilla', address: 'Av. de la Constitución, 22' },
  { id: 'col-11', name: 'Colegio La Salle', city: 'Barcelona', address: 'Passeig de Gràcia, 60' },
  { id: 'col-12', name: 'CEIP Los Olivos', city: 'Murcia', address: 'Calle Trapería, 9' },
  { id: 'col-13', name: 'Colegio Montessori', city: 'Valencia', address: 'Av. del Puerto, 100' },
  { id: 'col-14', name: 'IES Miguel Hernández', city: 'Alicante', address: 'Rambla Méndez Núñez, 18' },
  { id: 'col-15', name: 'CEIP Vicente Aleixandre', city: 'Madrid', address: 'Calle Bravo Murillo, 300' },
  { id: 'col-16', name: 'CEIP Joaquín Sorolla', city: 'Valencia', address: 'Calle Cirilo Amorós, 50' },
];

export const schoolClassesMock: SchoolClass[] = [
  { id: 'cl-1', schoolId: 'col-1', name: '1ºA' },
  { id: 'cl-2', schoolId: 'col-1', name: '2ºA' },
  { id: 'cl-3', schoolId: 'col-1', name: '3ºA' },
  { id: 'cl-4', schoolId: 'col-1', name: '3ºB' },
  { id: 'cl-5', schoolId: 'col-3', name: '1º Infantil' },
  { id: 'cl-6', schoolId: 'col-3', name: '2º Infantil' },
  { id: 'cl-7', schoolId: 'col-3', name: '1º Primaria' },
  { id: 'cl-8', schoolId: 'col-7', name: '4ºA' },
  { id: 'cl-9', schoolId: 'col-7', name: '4ºB' },
  { id: 'cl-10', schoolId: 'col-11', name: '5ºA' },
  { id: 'cl-11', schoolId: 'col-11', name: '5ºB' },
  { id: 'cl-12', schoolId: 'col-11', name: '6ºA' },
];

export const registeredStudentsMock: RegisteredStudent[] = [
  { id: 'al-1', classId: 'cl-3', name: 'Martina', lastName: 'Ruiz' },
  { id: 'al-2', classId: 'cl-3', name: 'Pablo', lastName: 'Gómez' },
  { id: 'al-3', classId: 'cl-8', name: 'Lucía', lastName: 'Fernández' },
];
