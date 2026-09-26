import { Link } from 'react-router-dom';
import { Logo } from './Logo';

const cols = [
  {
    title: 'Producto',
    links: [
      { label: 'Cómo funciona', href: '#how-it-works' },
      { label: 'Funcionalidades', href: '#features' },
      { label: 'Opiniones', href: '#testimonials' },
      { label: 'Preguntas frecuentes', href: '#faq' },
    ],
  },
  {
    title: 'Legal',
    links: [
      { label: 'Política de privacidad', href: '#privacy' },
      { label: 'Términos y condiciones', href: '#terms' },
      { label: 'Política de cookies', href: '#cookies' },
    ],
  },
  {
    title: 'Contacto',
    links: [
      { label: 'hola@sumiclas.es', href: 'mailto:hola@sumiclas.es' },
      { label: 'Soporte', href: '#support' },
      { label: 'Iniciar sesión', href: '/login' },
    ],
  },
];

export function Footer() {
  return (
    <footer className="border-t border-slate-200 bg-slate-50 dark:border-slate-800 dark:bg-slate-950">
      <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
        <div className="grid gap-8 lg:grid-cols-[1.5fr_1fr_1fr_1fr]">
          <div>
            <Logo />
            <p className="mt-3 max-w-xs text-sm leading-relaxed text-slate-600 dark:text-slate-400">
              La app que sustituye al grupo de WhatsApp del cole. Avisos, calendario, deberes, gastos y un resumen semanal con IA.
            </p>
          </div>

          {cols.map((col) => (
            <div key={col.title}>
              <h3 className="font-display text-sm font-bold uppercase tracking-wider text-slate-900 dark:text-white">
                {col.title}
              </h3>
              <ul className="mt-3 space-y-2">
                {col.links.map((l) => (
                  <li key={l.label}>
                    {l.href.startsWith('/') ? (
                      <Link
                        to={l.href}
                        className="text-sm text-slate-600 transition-colors hover:text-brand-600 dark:text-slate-400 dark:hover:text-brand-400"
                      >
                        {l.label}
                      </Link>
                    ) : (
                      <a
                        href={l.href}
                        className="text-sm text-slate-600 transition-colors hover:text-brand-600 dark:text-slate-400 dark:hover:text-brand-400"
                      >
                        {l.label}
                      </a>
                    )}
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </div>

        <div className="mt-8 flex flex-col items-center justify-between gap-3 border-t border-slate-200 pt-5 dark:border-slate-800 sm:flex-row">
          <p className="text-xs text-slate-500 dark:text-slate-500">
            © {new Date().getFullYear()} SumiClas. Hecho con cariño para las familias del cole.
          </p>
          <p className="text-xs text-slate-400 dark:text-slate-600">
            Los datos de ejemplo son ficticios.
          </p>
        </div>
      </div>
    </footer>
  );
}
