import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { MapPin, LayoutGrid, type LucideIcon } from 'lucide-react';
import { Reveal } from '@/components/layout/Reveal';
import { StepIndicator } from '@/components/onboarding/StepIndicator';
import { SchoolSearchStep } from '@/components/onboarding/SchoolSearchStep';
import { ClassStep } from '@/components/onboarding/ClassStep';
import { StudentStep } from '@/components/onboarding/StudentStep';
import { colegiosMock, clasesMock, alumnosMock, type Colegio, type Clase } from '@/data/onboardingMock';
import { setAuthenticated, setTutorInfo, type TutorRole } from '@/lib/auth';

const stepMeta = [
  {
    title: 'Busca tu colegio',
    text: 'Selecciónalo del listado oficial de centros para verificarlo automáticamente.',
  },
  {
    title: 'Elige tu clase',
    text: 'Únete a la clase de tu hijo o crea una nueva si todavía no existe.',
  },
  {
    title: 'Añade a tu hijo/a',
    text: 'Solo se mostrará su nombre e inicial del apellido al resto de familias.',
  },
];

export default function Onboarding() {
  const navigate = useNavigate();
  const [step, setStep] = useState(1);
  const [colegio, setColegio] = useState<Colegio | null>(null);
  const [clase, setClase] = useState<Clase | null>(null);
  const [customClases, setCustomClases] = useState<Clase[]>([]);
  const [claseEsNueva, setClaseEsNueva] = useState(false);

  const clasesDelColegio = colegio
    ? [...clasesMock, ...customClases].filter((c) => c.colegioId === colegio.id)
    : [];

  const alumnosDeLaClase = clase ? alumnosMock.filter((a) => a.claseId === clase.id) : [];

  const handleSelectColegio = (c: Colegio) => {
    setColegio(c);
    setClase(null);
    setStep(2);
  };

  const handleChangeColegio = () => {
    setColegio(null);
    setClase(null);
    setStep(1);
  };

  const handleSelectClase = (c: Clase) => {
    setClase(c);
    setClaseEsNueva(false);
    setStep(3);
  };

  const handleChangeClase = () => {
    setClase(null);
    setStep(2);
  };

  const handleCreateClase = (nombre: string) => {
    if (!colegio) return;
    const nueva: Clase = { id: `custom-${Date.now()}`, colegioId: colegio.id, nombre };
    setCustomClases((prev) => [...prev, nueva]);
    setClase(nueva);
    setClaseEsNueva(true);
    setStep(3);
  };

  const handleConfirmAlumno = (data: {
    nombre: string;
    apellido: string;
    tutorNombre: string;
    tutorRol: TutorRole;
    unirseComoSegundoTutor: boolean;
  }) => {
    setTutorInfo({ nombre: data.tutorNombre, rol: data.tutorRol });
    setAuthenticated(true);
    navigate('/inicio', {
      state: {
        colegio: colegio?.nombre,
        clase: clase?.nombre,
        alumno: `${data.nombre} ${data.apellido.charAt(0)}.`,
        segundoTutor: data.unirseComoSegundoTutor,
        rolAsignado: claseEsNueva ? 'delegado' : 'participante',
      },
    });
  };

  return (
    <section className="relative overflow-hidden px-4 pb-80 pt-28 sm:px-6 lg:px-8">
      <div className="pointer-events-none absolute inset-0 -z-10">
        <div className="absolute inset-0 grid-pattern opacity-60 dark:opacity-30" />
        <div className="absolute -left-24 top-10 h-72 w-72 rounded-full bg-brand-300/40 blur-3xl animate-blob-1 dark:bg-brand-600/20" />
        <div className="absolute -right-16 top-40 h-80 w-80 rounded-full bg-mint-300/40 blur-3xl animate-blob-2 dark:bg-mint-600/20" />
        <div className="absolute bottom-10 left-1/3 h-64 w-64 rounded-full bg-rose-300/30 blur-3xl animate-blob-1 dark:bg-rose-600/15" />
      </div>

      <div className="mx-auto max-w-4xl">
        <Reveal className="text-center">
          <span className="inline-flex items-center gap-2 rounded-full border border-brand-200 bg-brand-50 px-3 py-1 text-sm font-semibold text-brand-700 dark:border-brand-700/40 dark:bg-brand-900/30 dark:text-brand-300">
            Primeros pasos
          </span>
          <h1 className="mt-5 font-display text-3xl font-extrabold tracking-tight text-slate-900 dark:text-white sm:text-4xl">
            Vamos a dar de alta a tu hijo/a
          </h1>
        </Reveal>

        <Reveal delay={80} className="mt-8">
          <StepIndicator current={step} />
        </Reveal>

        <Reveal delay={140} className="relative mt-10">
          <div className="absolute -inset-8 -z-10 rounded-[3rem] bg-gradient-to-br from-brand-400/20 via-mint-400/20 to-rose-400/20 blur-2xl" />

          <div className="rounded-3xl border border-slate-200 bg-white p-8 shadow-2xl shadow-slate-900/5 dark:border-slate-700 dark:bg-slate-900 sm:p-12">
            {colegio && step > 1 && (
              <SelectionSummary
                icon={MapPin}
                title={colegio.nombre}
                subtitle={`${colegio.direccion}, ${colegio.localidad}`}
                color="brand"
                onChange={handleChangeColegio}
              />
            )}
            {clase && step > 2 && (
              <SelectionSummary
                icon={LayoutGrid}
                title={`Clase ${clase.nombre}`}
                subtitle={colegio?.nombre}
                color="mint"
                onChange={handleChangeClase}
              />
            )}

            <h2 className="font-display text-lg font-bold text-slate-900 dark:text-white">
              {stepMeta[step - 1].title}
            </h2>
            <p className="mt-1 text-sm text-slate-600 dark:text-slate-400">{stepMeta[step - 1].text}</p>

            <div className="mt-6">
              {step === 1 && <SchoolSearchStep colegios={colegiosMock} onSelect={handleSelectColegio} />}
              {step === 2 && (
                <ClassStep clases={clasesDelColegio} onSelect={handleSelectClase} onCreate={handleCreateClase} />
              )}
              {step === 3 && <StudentStep alumnosEnClase={alumnosDeLaClase} onConfirm={handleConfirmAlumno} />}
            </div>
          </div>
        </Reveal>
      </div>
    </section>
  );
}

function SelectionSummary({
  icon: Icon,
  title,
  subtitle,
  color,
  onChange,
}: {
  icon: LucideIcon;
  title: string;
  subtitle?: string;
  color: 'brand' | 'mint';
  onChange: () => void;
}) {
  const wrapStyles =
    color === 'brand'
      ? 'border-brand-200 bg-brand-50/50 dark:border-brand-700/40 dark:bg-brand-900/15'
      : 'border-mint-200 bg-mint-50/50 dark:border-mint-700/40 dark:bg-mint-900/15';
  const iconStyles =
    color === 'brand' ? 'from-brand-400 via-mint-400 to-rose-400' : 'from-mint-400 via-brand-400 to-rose-400';

  return (
    <div className={`mb-3 flex items-center justify-between gap-3 rounded-2xl border p-3.5 ${wrapStyles}`}>
      <div className="flex min-w-0 items-center gap-3">
        <div
          className={`grid h-9 w-9 shrink-0 place-items-center rounded-xl bg-gradient-to-br text-white ${iconStyles}`}
        >
          <Icon className="h-4 w-4" />
        </div>
        <div className="min-w-0">
          <p className="truncate text-sm font-bold text-slate-900 dark:text-white">{title}</p>
          {subtitle && <p className="truncate text-xs text-slate-500 dark:text-slate-400">{subtitle}</p>}
        </div>
      </div>
      <button
        type="button"
        onClick={onChange}
        className="shrink-0 text-xs font-semibold text-brand-600 hover:text-brand-700 dark:text-brand-400 dark:hover:text-brand-300"
      >
        Cambiar
      </button>
    </div>
  );
}
