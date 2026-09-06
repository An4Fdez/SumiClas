import { useState, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Mail, Lock, ArrowRight, Eye, EyeOff, UserRound, CheckCircle2, AlertCircle } from 'lucide-react';
import { Reveal } from '@/components/layout/Reveal';
import { setAuthenticated, setTutorInfo, type TutorRole } from '@/lib/auth';
import { isValidEmail, isPasswordPwned, MIN_PASSWORD_LENGTH, RECOMMENDED_PASSWORD_LENGTH } from '@/lib/validation';

const rolOptions: { value: TutorRole; label: string }[] = [
  { value: 'madre', label: 'Madre' },
  { value: 'padre', label: 'Padre' },
  { value: 'tutor', label: 'Tutor/a legal' },
];

export default function Registro() {
  const navigate = useNavigate();
  const [nombre, setNombre] = useState('');
  const [apellidos, setApellidos] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [password2, setPassword2] = useState('');
  const [rol, setRol] = useState<TutorRole | null>(null);
  const [aceptaTerminos, setAceptaTerminos] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [emailTouched, setEmailTouched] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [comprobando, setComprobando] = useState(false);

  const emailValido = isValidEmail(email);
  const longitudOk = password.length >= MIN_PASSWORD_LENGTH;
  const longitudRecomendada = password.length >= RECOMMENDED_PASSWORD_LENGTH;
  const passwordsCoinciden = password.length > 0 && password === password2;

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    const nuevosErrores: Record<string, string> = {};

    if (!nombre.trim()) nuevosErrores.nombre = 'Indica tu nombre.';
    if (!apellidos.trim()) nuevosErrores.apellidos = 'Indica tus apellidos.';
    if (!isValidEmail(email)) nuevosErrores.email = 'El email no parece válido.';
    if (!longitudOk) nuevosErrores.password = `La contraseña debe tener al menos ${MIN_PASSWORD_LENGTH} caracteres.`;
    if (!passwordsCoinciden) nuevosErrores.password2 = 'Las contraseñas no coinciden.';
    if (!rol) nuevosErrores.rol = 'Indica tu relación con el alumno/a.';
    if (!aceptaTerminos) nuevosErrores.terminos = 'Debes aceptar los términos y la política de privacidad.';

    if (Object.keys(nuevosErrores).length > 0) {
      setErrors(nuevosErrores);
      return;
    }

    setComprobando(true);
    const pwned = await isPasswordPwned(password);
    setComprobando(false);

    if (pwned) {
      setErrors({ password: 'Esta contraseña aparece en filtraciones conocidas. Elige otra distinta.' });
      return;
    }

    setErrors({});
    const primerApellido = apellidos.trim().split(/\s+/)[0] ?? '';
    setTutorInfo({ nombre: `${nombre.trim()} ${primerApellido}`.trim(), rol: rol as TutorRole });
    setAuthenticated(true);
    navigate('/onboarding');
  };

  return (
    <section className="relative flex min-h-screen items-center justify-center overflow-hidden px-4 pb-16 pt-28 sm:px-6 lg:px-8">
      <div className="pointer-events-none absolute inset-0 -z-10">
        <div className="absolute inset-0 grid-pattern opacity-60 dark:opacity-30" />
        <div className="absolute -left-24 top-10 h-72 w-72 rounded-full bg-brand-300/40 blur-3xl animate-blob-1 dark:bg-brand-600/20" />
        <div className="absolute -right-16 top-32 h-80 w-80 rounded-full bg-mint-300/40 blur-3xl animate-blob-2 dark:bg-mint-600/20" />
        <div className="absolute left-1/3 top-1/2 h-64 w-64 rounded-full bg-rose-300/30 blur-3xl animate-blob-1 dark:bg-rose-600/15" />
        <div className="absolute -bottom-10 right-1/4 h-64 w-64 rounded-full bg-violet-300/30 blur-3xl animate-blob-2 dark:bg-violet-600/15" />
      </div>

      <Reveal className="relative w-full max-w-md">
        <div className="absolute -inset-6 -z-10 rounded-[3rem] bg-gradient-to-br from-brand-400/30 via-mint-400/30 to-rose-400/30 blur-2xl" />

        <div className="rounded-3xl border border-slate-200 bg-white p-8 shadow-2xl shadow-slate-900/5 dark:border-slate-700 dark:bg-slate-900 sm:p-10">
          <div className="text-center">
            <span className="inline-flex items-center gap-2 rounded-full border border-brand-200 bg-brand-50 px-3 py-1 text-sm font-semibold text-brand-700 dark:border-brand-700/40 dark:bg-brand-900/30 dark:text-brand-300">
              Primeros pasos
            </span>
            <h1 className="mt-5 font-display text-2xl font-extrabold tracking-tight text-slate-900 dark:text-white sm:text-3xl">
              Crea tu cuenta en{' '}
              <span className="text-gradient bg-gradient-to-r from-brand-500 via-mint-500 to-rose-500">
                clase
              </span>
            </h1>
            <p className="mt-2 text-sm text-slate-600 dark:text-slate-400">
              Después buscaremos tu colegio y darás de alta a tu hijo/a.
            </p>
          </div>

          <form className="mt-8 space-y-4" onSubmit={handleSubmit} noValidate>
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label htmlFor="nombre" className="mb-1.5 block text-sm font-semibold text-slate-700 dark:text-slate-300">
                  Nombre
                </label>
                <div className="flex items-center gap-2 rounded-xl border border-slate-200 bg-slate-50 px-3 py-2.5 transition-colors focus-within:border-brand-400 focus-within:bg-white focus-within:ring-2 focus-within:ring-brand-100 dark:border-slate-700 dark:bg-slate-800/60 dark:focus-within:border-brand-500 dark:focus-within:bg-slate-900 dark:focus-within:ring-brand-900/40">
                  <UserRound className="h-4 w-4 shrink-0 text-slate-400" />
                  <input
                    id="nombre"
                    type="text"
                    value={nombre}
                    onChange={(e) => setNombre(e.target.value)}
                    placeholder="Ana"
                    className="w-full bg-transparent text-sm text-slate-700 placeholder:text-slate-400 focus:outline-none dark:text-slate-200"
                  />
                </div>
                {errors.nombre && <p className="mt-1 text-xs text-rose-600 dark:text-rose-400">{errors.nombre}</p>}
              </div>

              <div>
                <label htmlFor="apellidos" className="mb-1.5 block text-sm font-semibold text-slate-700 dark:text-slate-300">
                  Apellidos
                </label>
                <div className="flex items-center gap-2 rounded-xl border border-slate-200 bg-slate-50 px-3 py-2.5 transition-colors focus-within:border-brand-400 focus-within:bg-white focus-within:ring-2 focus-within:ring-brand-100 dark:border-slate-700 dark:bg-slate-800/60 dark:focus-within:border-brand-500 dark:focus-within:bg-slate-900 dark:focus-within:ring-brand-900/40">
                  <UserRound className="h-4 w-4 shrink-0 text-slate-400" />
                  <input
                    id="apellidos"
                    type="text"
                    value={apellidos}
                    onChange={(e) => setApellidos(e.target.value)}
                    placeholder="Ruiz Gómez"
                    className="w-full bg-transparent text-sm text-slate-700 placeholder:text-slate-400 focus:outline-none dark:text-slate-200"
                  />
                </div>
                {errors.apellidos && <p className="mt-1 text-xs text-rose-600 dark:text-rose-400">{errors.apellidos}</p>}
              </div>
            </div>

            <div>
              <label htmlFor="email" className="mb-1.5 block text-sm font-semibold text-slate-700 dark:text-slate-300">
                Correo electrónico
              </label>
              <div
                className={`flex items-center gap-2 rounded-xl border bg-slate-50 px-3 py-2.5 transition-colors focus-within:ring-2 dark:bg-slate-800/60 ${
                  emailTouched && email && !emailValido
                    ? 'border-rose-300 focus-within:border-rose-400 focus-within:ring-rose-100 dark:border-rose-700/60'
                    : 'border-slate-200 focus-within:border-brand-400 focus-within:bg-white focus-within:ring-brand-100 dark:border-slate-700 dark:focus-within:border-brand-500 dark:focus-within:bg-slate-900 dark:focus-within:ring-brand-900/40'
                }`}
              >
                <Mail className="h-4 w-4 shrink-0 text-slate-400" />
                <input
                  id="email"
                  type="email"
                  autoComplete="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  onBlur={() => setEmailTouched(true)}
                  placeholder="tu@email.com"
                  className="w-full bg-transparent text-sm text-slate-700 placeholder:text-slate-400 focus:outline-none dark:text-slate-200"
                />
                {emailTouched && email && (
                  emailValido ? (
                    <CheckCircle2 className="h-4 w-4 shrink-0 text-mint-500" />
                  ) : (
                    <AlertCircle className="h-4 w-4 shrink-0 text-rose-500" />
                  )
                )}
              </div>
              {((emailTouched && email && !emailValido) || errors.email) && (
                <p className="mt-1 text-xs text-rose-600 dark:text-rose-400">El email no parece válido.</p>
              )}
            </div>

            <div>
              <label htmlFor="password" className="mb-1.5 block text-sm font-semibold text-slate-700 dark:text-slate-300">
                Contraseña
              </label>
              <div className="flex items-center gap-2 rounded-xl border border-slate-200 bg-slate-50 px-3 py-2.5 transition-colors focus-within:border-brand-400 focus-within:bg-white focus-within:ring-2 focus-within:ring-brand-100 dark:border-slate-700 dark:bg-slate-800/60 dark:focus-within:border-brand-500 dark:focus-within:bg-slate-900 dark:focus-within:ring-brand-900/40">
                <Lock className="h-4 w-4 shrink-0 text-slate-400" />
                <input
                  id="password"
                  type={showPassword ? 'text' : 'password'}
                  autoComplete="new-password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  placeholder="••••••••"
                  className="w-full bg-transparent text-sm text-slate-700 placeholder:text-slate-400 focus:outline-none dark:text-slate-200"
                />
                <button
                  type="button"
                  onClick={() => setShowPassword((v) => !v)}
                  aria-label={showPassword ? 'Ocultar contraseña' : 'Mostrar contraseña'}
                  className="shrink-0 text-slate-400 transition-colors hover:text-slate-600 dark:hover:text-slate-300"
                >
                  {showPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                </button>
              </div>
              {password.length > 0 && (
                <ul className="mt-1.5 space-y-0.5 text-xs">
                  <li className={longitudOk ? 'text-mint-600 dark:text-mint-400' : 'text-slate-500 dark:text-slate-400'}>
                    {longitudOk ? '✓' : '·'} Al menos {MIN_PASSWORD_LENGTH} caracteres
                  </li>
                  <li className={longitudRecomendada ? 'text-mint-600 dark:text-mint-400' : 'text-slate-400 dark:text-slate-500'}>
                    {longitudRecomendada ? '✓' : '·'} Recomendado: {RECOMMENDED_PASSWORD_LENGTH}+ caracteres
                  </li>
                </ul>
              )}
              {errors.password && <p className="mt-1 text-xs text-rose-600 dark:text-rose-400">{errors.password}</p>}
            </div>

            <div>
              <label htmlFor="password2" className="mb-1.5 block text-sm font-semibold text-slate-700 dark:text-slate-300">
                Repite la contraseña
              </label>
              <div className="flex items-center gap-2 rounded-xl border border-slate-200 bg-slate-50 px-3 py-2.5 transition-colors focus-within:border-brand-400 focus-within:bg-white focus-within:ring-2 focus-within:ring-brand-100 dark:border-slate-700 dark:bg-slate-800/60 dark:focus-within:border-brand-500 dark:focus-within:bg-slate-900 dark:focus-within:ring-brand-900/40">
                <Lock className="h-4 w-4 shrink-0 text-slate-400" />
                <input
                  id="password2"
                  type={showPassword ? 'text' : 'password'}
                  autoComplete="new-password"
                  value={password2}
                  onChange={(e) => setPassword2(e.target.value)}
                  placeholder="••••••••"
                  className="w-full bg-transparent text-sm text-slate-700 placeholder:text-slate-400 focus:outline-none dark:text-slate-200"
                />
              </div>
              {errors.password2 && <p className="mt-1 text-xs text-rose-600 dark:text-rose-400">{errors.password2}</p>}
            </div>

            <div>
              <p className="mb-1.5 text-sm font-semibold text-slate-700 dark:text-slate-300">Eres su...</p>
              <div className="grid grid-cols-3 gap-2">
                {rolOptions.map((opt) => (
                  <button
                    key={opt.value}
                    type="button"
                    onClick={() => setRol(opt.value)}
                    className={`rounded-xl border px-3 py-2 text-sm font-semibold transition-colors ${
                      rol === opt.value
                        ? 'border-brand-400 bg-brand-50 text-brand-700 dark:border-brand-500 dark:bg-brand-900/30 dark:text-brand-300'
                        : 'border-slate-200 bg-slate-50 text-slate-600 hover:border-slate-300 dark:border-slate-700 dark:bg-slate-800/60 dark:text-slate-300 dark:hover:border-slate-600'
                    }`}
                  >
                    {opt.label}
                  </button>
                ))}
              </div>
              {errors.rol && <p className="mt-1 text-xs text-rose-600 dark:text-rose-400">{errors.rol}</p>}
            </div>

            <div>
              <label className="flex items-start gap-2.5 text-sm text-slate-600 dark:text-slate-400">
                <input
                  type="checkbox"
                  checked={aceptaTerminos}
                  onChange={(e) => setAceptaTerminos(e.target.checked)}
                  className="mt-0.5 h-4 w-4 shrink-0 rounded border-slate-300 text-brand-600 focus:ring-brand-400 dark:border-slate-600 dark:bg-slate-800"
                />
                <span>
                  He leído y acepto los{' '}
                  <Link to="/terminos" className="font-semibold text-brand-600 hover:text-brand-700 dark:text-brand-400">
                    Términos de uso
                  </Link>{' '}
                  y la{' '}
                  <Link to="/privacidad" className="font-semibold text-brand-600 hover:text-brand-700 dark:text-brand-400">
                    Política de privacidad
                  </Link>
                  .
                </span>
              </label>
              {errors.terminos && <p className="mt-1 text-xs text-rose-600 dark:text-rose-400">{errors.terminos}</p>}
            </div>

            <button
              type="submit"
              disabled={comprobando}
              className="mt-2 flex w-full items-center justify-center gap-1.5 rounded-xl bg-gradient-to-r from-brand-500 to-brand-600 px-4 py-3 text-sm font-semibold text-white shadow-lg shadow-brand-500/25 transition-transform hover:scale-[1.02] active:scale-95 disabled:opacity-60 disabled:hover:scale-100"
            >
              {comprobando ? 'Comprobando contraseña…' : 'Crear cuenta'}
              {!comprobando && <ArrowRight className="h-4 w-4" />}
            </button>
          </form>

          <p className="mt-6 text-center text-sm text-slate-600 dark:text-slate-400">
            ¿Ya tienes cuenta?{' '}
            <Link to="/login" className="font-semibold text-brand-600 hover:text-brand-700 dark:text-brand-400 dark:hover:text-brand-300">
              Inicia sesión
            </Link>
          </p>
        </div>

        <p className="mt-6 text-center text-sm text-slate-500 dark:text-slate-500">
          <Link to="/" className="hover:text-slate-700 dark:hover:text-slate-300">
            ← Volver al inicio
          </Link>
        </p>
      </Reveal>
    </section>
  );
}
