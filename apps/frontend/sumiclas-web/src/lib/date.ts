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
