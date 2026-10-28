import type { ReactNode } from 'react';
import { LAST_UPDATED } from '@/lib/legal';

export type LegalSection = {
  title: string;
  paragraphs?: ReactNode[];
  items?: ReactNode[];
};

export function LegalPage({ title, sections }: { title: string; sections: LegalSection[] }) {
  return (
    <section className="mx-auto max-w-3xl px-4 pb-16 pt-28 sm:px-6 lg:px-8">
      <h1 className="font-display text-3xl font-extrabold tracking-tight text-slate-900 dark:text-white sm:text-4xl">
        {title}
      </h1>
      <p className="mt-2 text-sm text-slate-500 dark:text-slate-400">Última actualización: {LAST_UPDATED}</p>

      {sections.map((section) => (
        <div key={section.title} className="mt-8">
          <h2 className="font-display text-xl font-bold text-slate-900 dark:text-white">{section.title}</h2>
          {section.paragraphs?.map((paragraph, index) => (
            <p key={index} className="mt-3 text-sm leading-relaxed text-slate-600 dark:text-slate-300">
              {paragraph}
            </p>
          ))}
          {section.items && (
            <ul className="mt-3 list-disc space-y-2 pl-5 text-sm leading-relaxed text-slate-600 marker:text-slate-400 dark:text-slate-300 dark:marker:text-slate-500">
              {section.items.map((item, index) => (
                <li key={index}>{item}</li>
              ))}
            </ul>
          )}
        </div>
      ))}
    </section>
  );
}
