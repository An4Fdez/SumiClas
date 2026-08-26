export type Colegio = {
  id: string;
  nombre: string;
  localidad: string;
  direccion: string;
};

export type Clase = {
  id: string;
  colegioId: string;
  nombre: string;
};

export type AlumnoRegistrado = {
  id: string;
  claseId: string;
  nombre: string;
  apellido: string;
};

export const colegiosMock: Colegio[] = [
  { id: 'col-1', nombre: 'CEIP Miguel de Cervantes', localidad: 'Madrid', direccion: 'Calle Mayor, 12' },
  { id: 'col-2', nombre: 'CEIP Miguel de Cervantes', localidad: 'Alcalá de Henares', direccion: 'Av. Complutense, 5' },
  { id: 'col-3', nombre: 'Colegio San José', localidad: 'Valencia', direccion: 'Calle Colón, 44' },
  { id: 'col-4', nombre: 'Colegio San José', localidad: 'Bilbao', direccion: 'Gran Vía, 8' },
  { id: 'col-5', nombre: 'CEIP Federico García Lorca', localidad: 'Madrid', direccion: 'Paseo de la Castellana, 210' },
  { id: 'col-6', nombre: 'IES Federico García Lorca', localidad: 'Granada', direccion: 'Camino de Ronda, 90' },
  { id: 'col-7', nombre: 'CEIP Pablo Picasso', localidad: 'Málaga', direccion: 'Calle Larios, 3' },
  { id: 'col-8', nombre: 'Colegio Santa Teresa', localidad: 'Zaragoza', direccion: 'Paseo Independencia, 15' },
  { id: 'col-9', nombre: 'CEIP Antonio Machado', localidad: 'Madrid', direccion: 'Calle Alcalá, 155' },
  { id: 'col-10', nombre: 'IES Antonio Machado', localidad: 'Sevilla', direccion: 'Av. de la Constitución, 22' },
  { id: 'col-11', nombre: 'Colegio La Salle', localidad: 'Barcelona', direccion: 'Passeig de Gràcia, 60' },
  { id: 'col-12', nombre: 'CEIP Los Olivos', localidad: 'Murcia', direccion: 'Calle Trapería, 9' },
  { id: 'col-13', nombre: 'Colegio Montessori', localidad: 'Valencia', direccion: 'Av. del Puerto, 100' },
  { id: 'col-14', nombre: 'IES Miguel Hernández', localidad: 'Alicante', direccion: 'Rambla Méndez Núñez, 18' },
  { id: 'col-15', nombre: 'CEIP Vicente Aleixandre', localidad: 'Madrid', direccion: 'Calle Bravo Murillo, 300' },
  { id: 'col-16', nombre: 'CEIP Joaquín Sorolla', localidad: 'Valencia', direccion: 'Calle Cirilo Amorós, 50' },
];

export const clasesMock: Clase[] = [
  { id: 'cl-1', colegioId: 'col-1', nombre: '1ºA' },
  { id: 'cl-2', colegioId: 'col-1', nombre: '2ºA' },
  { id: 'cl-3', colegioId: 'col-1', nombre: '3ºA' },
  { id: 'cl-4', colegioId: 'col-1', nombre: '3ºB' },
  { id: 'cl-5', colegioId: 'col-3', nombre: '1º Infantil' },
  { id: 'cl-6', colegioId: 'col-3', nombre: '2º Infantil' },
  { id: 'cl-7', colegioId: 'col-3', nombre: '1º Primaria' },
  { id: 'cl-8', colegioId: 'col-7', nombre: '4ºA' },
  { id: 'cl-9', colegioId: 'col-7', nombre: '4ºB' },
  { id: 'cl-10', colegioId: 'col-11', nombre: '5ºA' },
  { id: 'cl-11', colegioId: 'col-11', nombre: '5ºB' },
  { id: 'cl-12', colegioId: 'col-11', nombre: '6ºA' },
];

export const alumnosMock: AlumnoRegistrado[] = [
  { id: 'al-1', claseId: 'cl-3', nombre: 'Martina', apellido: 'Ruiz' },
  { id: 'al-2', claseId: 'cl-3', nombre: 'Pablo', apellido: 'Gómez' },
  { id: 'al-3', claseId: 'cl-8', nombre: 'Lucía', apellido: 'Fernández' },
];
