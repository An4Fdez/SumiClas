const rtf = new Intl.RelativeTimeFormat('es', { numeric: 'auto' });

export function formatFechaRelativa(fechaISO: string): string {
  const fecha = new Date(fechaISO);
  const diffMs = fecha.getTime() - Date.now();
  const diffMin = Math.round(diffMs / 60_000);
  const diffH = Math.round(diffMs / 3_600_000);
  const diffD = Math.round(diffMs / 86_400_000);

  if (Math.abs(diffMin) < 60) return rtf.format(diffMin, 'minute');
  if (Math.abs(diffH) < 24) return rtf.format(diffH, 'hour');
  if (Math.abs(diffD) < 7) return rtf.format(diffD, 'day');

  return fecha.toLocaleDateString('es-ES', { day: 'numeric', month: 'short' });
}

export type DiaCalendario = {
  fecha: Date;
  enMesActual: boolean;
  esHoy: boolean;
};

export function esMismoDiaCalendario(a: Date, b: Date): boolean {
  return a.getFullYear() === b.getFullYear() && a.getMonth() === b.getMonth() && a.getDate() === b.getDate();
}

/** Año en el que mostrar un evento: el real, salvo recurrentes anuales, que siempre se navegan al año en curso. */
export function anioVisibleEvento(fechaISO: string, recurrenteAnual: boolean): number {
  return recurrenteAnual ? new Date().getFullYear() : new Date(fechaISO).getFullYear();
}

/** true si un evento ocurre ese día; para recurrentes anuales (cumpleaños) compara solo mes+día. */
export function eventoOcurreEnDia(fechaEventoISO: string, dia: Date, recurrenteAnual: boolean): boolean {
  const fechaEvento = new Date(fechaEventoISO);
  if (recurrenteAnual) {
    return fechaEvento.getMonth() === dia.getMonth() && fechaEvento.getDate() === dia.getDate();
  }
  return esMismoDiaCalendario(fechaEvento, dia);
}

/** Matriz de semanas (L-D) que cubren el mes indicado (mes: 0-11), con relleno de días adyacentes. */
export function generarMatrizMes(anio: number, mes: number): DiaCalendario[][] {
  const primerDia = new Date(anio, mes, 1);
  const ultimoDia = new Date(anio, mes + 1, 0);
  const offsetInicio = (primerDia.getDay() + 6) % 7;
  const offsetFin = (7 - ((ultimoDia.getDay() + 6) % 7) - 1 + 7) % 7;
  const totalDias = offsetInicio + ultimoDia.getDate() + offsetFin;

  const hoy = new Date();
  const cursor = new Date(anio, mes, 1 - offsetInicio);
  const dias: DiaCalendario[] = [];

  for (let i = 0; i < totalDias; i++) {
    dias.push({
      fecha: new Date(cursor),
      enMesActual: cursor.getMonth() === mes,
      esHoy: esMismoDiaCalendario(cursor, hoy),
    });
    cursor.setDate(cursor.getDate() + 1);
  }

  const semanas: DiaCalendario[][] = [];
  for (let i = 0; i < dias.length; i += 7) {
    semanas.push(dias.slice(i, i + 7));
  }
  return semanas;
}

export function formatMesAnio(anio: number, mes: number): string {
  const texto = new Date(anio, mes, 1).toLocaleDateString('es-ES', { month: 'long', year: 'numeric' });
  const limpio = texto.replace(' de ', ' ');
  return limpio.charAt(0).toUpperCase() + limpio.slice(1);
}

/** Semana (L-D) que contiene la fecha indicada. */
export function generarSemana(fecha: Date): DiaCalendario[] {
  const offsetInicio = (fecha.getDay() + 6) % 7;
  const inicio = new Date(fecha);
  inicio.setDate(fecha.getDate() - offsetInicio);

  const hoy = new Date();
  const dias: DiaCalendario[] = [];
  for (let i = 0; i < 7; i++) {
    const d = new Date(inicio);
    d.setDate(inicio.getDate() + i);
    dias.push({ fecha: d, enMesActual: d.getMonth() === fecha.getMonth(), esHoy: esMismoDiaCalendario(d, hoy) });
  }
  return dias;
}

export function formatRangoSemana(dias: DiaCalendario[]): string {
  const inicio = dias[0].fecha;
  const fin = dias[6].fecha;
  const mesInicio = nombresMeses[inicio.getMonth()].slice(0, 3).toLowerCase();
  const mesFin = nombresMeses[fin.getMonth()].slice(0, 3).toLowerCase();

  if (inicio.getFullYear() === fin.getFullYear()) {
    if (inicio.getMonth() === fin.getMonth()) {
      return `${inicio.getDate()} – ${fin.getDate()} ${mesFin} ${fin.getFullYear()}`;
    }
    return `${inicio.getDate()} ${mesInicio} – ${fin.getDate()} ${mesFin} ${fin.getFullYear()}`;
  }
  return `${inicio.getDate()} ${mesInicio} ${inicio.getFullYear()} – ${fin.getDate()} ${mesFin} ${fin.getFullYear()}`;
}

export const nombresMeses = [
  'Enero',
  'Febrero',
  'Marzo',
  'Abril',
  'Mayo',
  'Junio',
  'Julio',
  'Agosto',
  'Septiembre',
  'Octubre',
  'Noviembre',
  'Diciembre',
];

export function formatDiaCompleto(fecha: Date): string {
  const texto = fecha.toLocaleDateString('es-ES', { weekday: 'long', day: 'numeric', month: 'long' });
  return texto.charAt(0).toUpperCase() + texto.slice(1);
}
