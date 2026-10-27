import type { ReactNode } from 'react';
import { UserRound } from 'lucide-react';
import { ProfileChildrenSection } from '@/components/app/ProfileChildrenSection';
import { ProfileNameForm } from '@/components/app/ProfileNameForm';
import { ProfilePasswordForm } from '@/components/app/ProfilePasswordForm';

function ProfileCard({ title, children }: { title: string; children: ReactNode }) {
  return (
    <section className="mt-6 rounded-2xl border border-slate-200 bg-white p-5 dark:border-slate-700 dark:bg-slate-900">
      <h2 className="mb-4 font-display text-lg font-bold text-slate-900 dark:text-white">{title}</h2>
      {children}
    </section>
  );
}

export default function Profile() {
  return (
    <div className="max-w-2xl">
      <div className="grid h-12 w-12 place-items-center rounded-xl bg-brand-50 text-brand-600 dark:bg-brand-900/30 dark:text-brand-300">
        <UserRound className="h-6 w-6" />
      </div>
      <h1 className="mt-4 font-display text-2xl font-extrabold tracking-tight text-slate-900 dark:text-white sm:text-3xl">
        Mi perfil
      </h1>
      <p className="mt-2 text-slate-600 dark:text-slate-400">Tus datos, tu contraseña y los datos de tus hijos/as.</p>

      <ProfileCard title="Mis datos">
        <ProfileNameForm />
      </ProfileCard>
      <ProfileCard title="Cambiar contraseña">
        <ProfilePasswordForm />
      </ProfileCard>
      <ProfileCard title="Mis hijos">
        <ProfileChildrenSection />
      </ProfileCard>
    </div>
  );
}
