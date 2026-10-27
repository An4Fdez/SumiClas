import { LOGO_GRADIENT_CLASS } from '@/lib/colorClasses';

/** Rounded-square avatar with the child's initial: a ring in the logo's stroke colors around a calm fill, with the same corner family as the screen icons. */
export function ChildAvatar({ name }: { name: string }) {
  return (
    <div className={`h-8 w-8 shrink-0 rounded-lg p-0.5 ${LOGO_GRADIENT_CLASS}`}>
      <div className="grid h-full w-full place-items-center rounded-md bg-white font-logo text-[15px] font-semibold leading-none text-slate-900 dark:bg-slate-900 dark:text-white">
        {name.charAt(0)}
      </div>
    </div>
  );
}
