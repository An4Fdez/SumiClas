export function Logo({ className = '' }: { className?: string }) {
  return (
    <div className={`flex items-center gap-2 ${className}`}>
      <div className="relative grid h-9 w-9 place-items-center overflow-hidden rounded-2xl bg-gradient-to-br from-brand-400 via-mint-400 to-rose-400 shadow-lg shadow-brand-500/20 ring-1 ring-inset ring-white/25">
        <div className="pointer-events-none absolute inset-0 bg-gradient-to-br from-white/35 via-white/0 to-black/10" />
        <span className="font-display text-[15px] font-black leading-none tracking-[-0.03em] text-white">SC</span>
      </div>
      <span className="font-logo text-[1.45rem] font-semibold leading-none tracking-[-0.02em] text-slate-900 [font-variation-settings:'SOFT'_50,'opsz'_100] dark:text-white">
        Sumi<span className="text-brand-500 italic dark:text-brand-400">Clas</span>
      </span>
    </div>
  );
}
