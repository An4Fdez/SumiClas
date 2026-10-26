export function Logo({ className = '' }: { className?: string }) {
  return (
    <div className={`flex items-center gap-2 ${className}`}>
      <img src="/logo-sumiclas.png" alt="" aria-hidden="true" className="h-9 w-auto select-none" draggable={false} />
      <span className="font-logo text-[1.45rem] font-semibold leading-none tracking-[-0.02em] text-slate-900 [font-variation-settings:'SOFT'_50,'opsz'_100] dark:text-white">
        Sumi<span className="text-brand-500 italic dark:text-brand-400">Clas</span>
      </span>
    </div>
  );
}
