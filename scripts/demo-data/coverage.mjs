#!/usr/bin/env node
// Checks, from the planned offsets only (no database, no API), that the demo has material for the weekly summary in the
// Monday-Sunday week that contains the anchor day and in the following one, whatever weekday the anchor day is.
// Usage: node scripts/demo-data/coverage.mjs
import { CLASSES, CONTENT } from './dataset.mjs';

const MIN_ITEMS = 4;
const MIN_SECTIONS = 2;
const DAYS_IN_WEEK = 7;
const WEEKDAY_NAMES = ['lunes', 'martes', 'miércoles', 'jueves', 'viernes', 'sábado', 'domingo'];

function offsetsOf(content) {
  return {
    announcements: content.announcements.map((item) => item.offset),
    events: content.events.map((item) => item.offset),
    assignments: content.assignments.map((item) => item.offset),
    birthdays: content.birthdays.map((item) => item.offset),
  };
}

const inRange = (values, from, to) => values.filter((offset) => offset >= from && offset <= to).length;

function weekCoverage(offsets, from) {
  const to = from + DAYS_IN_WEEK - 1;
  const announcements = inRange(offsets.announcements, from, to);
  const events = inRange(offsets.events, from, to);
  const assignments = inRange(offsets.assignments, from, to);
  const items = announcements + events + assignments;
  const sections = [announcements, events, assignments].filter((amount) => amount > 0).length;
  return { announcements, events, assignments, items, sections, ok: items >= MIN_ITEMS && sections >= MIN_SECTIONS };
}

let failures = 0;
for (const [classKey, definition] of Object.entries(CLASSES)) {
  const offsets = offsetsOf(CONTENT[classKey]);
  console.log(`\n${definition.name} (${classKey}): anuncios/eventos/deberes por semana (lunes-domingo) según el día de la semana del ancla`);
  for (let daysSinceMonday = 0; daysSinceMonday < DAYS_IN_WEEK; daysSinceMonday++) {
    const current = weekCoverage(offsets, -daysSinceMonday);
    const next = weekCoverage(offsets, DAYS_IN_WEEK - daysSinceMonday);
    const format = (week) => `${week.announcements}/${week.events}/${week.assignments} = ${String(week.items).padStart(2)} ${week.ok ? 'ok' : 'FALTA'}`;
    console.log(`  ancla en ${WEEKDAY_NAMES[daysSinceMonday].padEnd(9)} semana actual ${format(current)} | siguiente ${format(next)}`);
    if (!current.ok || !next.ok) failures++;
  }
}

console.log(failures === 0 ? '\nCobertura correcta en los siete casos de cada clase.' : `\n${failures} casos sin cobertura suficiente.`);
process.exit(failures === 0 ? 0 : 1);
