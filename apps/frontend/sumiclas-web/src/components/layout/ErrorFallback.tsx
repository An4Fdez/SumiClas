import { useEffect } from 'react';

// The theme is applied by the components that use useTheme; when the crash comes before any of them mounted, the
// fallback must still honour the stored theme. It cannot fail itself, so storage and media errors are swallowed.
function applyStoredTheme() {
  try {
    const stored = window.localStorage.getItem('sumiclas-theme');
    const dark = stored ? stored === 'dark' : window.matchMedia('(prefers-color-scheme: dark)').matches;
    document.documentElement.classList.toggle('dark', dark);
  } catch {
    // keep the default theme
  }
}

export function ErrorFallback({ onRetry, fullScreen = false }: { onRetry: () => void; fullScreen?: boolean }) {
  useEffect(applyStoredTheme, []);

  return (
    <div className={fullScreen ? 'grid min-h-screen place-items-center bg-slate-50 px-4 dark:bg-slate-950' : 'px-4 pb-16 pt-28'}>
      <div
        role="alert"
        className="mx-auto w-full max-w-md rounded-2xl border border-rose-200 bg-rose-50 p-6 text-center dark:border-rose-700/40 dark:bg-rose-900/20"
      >
        <h2 className="text-lg font-bold text-rose-800 dark:text-rose-200">Algo ha salido mal</h2>
        <p className="mt-2 text-sm text-rose-700 dark:text-rose-300">
          Ha ocurrido un error al mostrar esta pantalla. Puedes volver a intentarlo o ir al inicio.
        </p>
        <div className="mt-4 flex justify-center gap-2">
          <button
            type="button"
            onClick={onRetry}
            className="rounded-xl bg-brand-600 px-4 py-2 text-sm font-semibold text-white transition-colors hover:bg-brand-700"
          >
            Reintentar
          </button>
          <a
            href="/"
            className="rounded-xl border border-slate-200 px-4 py-2 text-sm font-semibold text-slate-600 transition-colors hover:bg-white dark:border-slate-700 dark:text-slate-300 dark:hover:bg-slate-800"
          >
            Ir al inicio
          </a>
        </div>
      </div>
    </div>
  );
}
