import { Sparkles } from 'lucide-react';

export function Logo({ className = '' }: { className?: string }) {
  return (
    <div className={`flex items-center gap-2 ${className}`}>
      <div className="relative grid h-9 w-9 place-items-center rounded-xl bg-gradient-to-br from-brand-400 via-mint-400 to-rose-400 shadow-lg shadow-brand-500/20">
        <Sparkles className="h-5 w-5 text-white" strokeWidth={2.5} />
        <span className="absolute -right-0.5 -top-0.5 h-2.5 w-2.5 rounded-full bg-sun-400 ring-2 ring-white dark:ring-slate-900" />
      </div>
      <span
        className="font-logo text-[1.45rem] font-semibold leading-none tracking-tight text-slate-900 dark:text-white"
        style={{ fontVariationSettings: '"SOFT" 50, "opsz" 100', letterSpacing: '-0.02em' }}
      >
        Sumi<span className="text-brand-500 italic dark:text-brand-400">Clas</span>
      </span>
    </div>
  );
}
