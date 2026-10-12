import { useState, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Mail, Lock, ArrowRight, Eye, EyeOff, UserRound, CheckCircle2, AlertCircle } from 'lucide-react';
import { Reveal } from '@/components/layout/Reveal';
import { setGuardianInfo, type GuardianRole } from '@/lib/auth';
import { getApiErrorMessage, getApiErrorStatus } from '@/lib/apiError';
import { AccountCreatedLoginFailedError } from '@/context/AuthContext';
import { useAuth } from '@/hooks/useAuth';
import { useWizardIds } from '@/hooks/useWizardIds';
import { isValidEmail, isPasswordPwned, MIN_PASSWORD_LENGTH, RECOMMENDED_PASSWORD_LENGTH } from '@/lib/validation';

const roleOptions: { value: GuardianRole; label: string }[] = [
  { value: 'mother', label: 'Madre' },
  { value: 'father', label: 'Padre' },
  { value: 'guardian', label: 'Tutor/a legal' },
];

export default function Register() {
  const navigate = useNavigate();
  const { register } = useAuth();
  const ids = useWizardIds();
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [role, setRole] = useState<GuardianRole | null>(null);
  const [acceptsTerms, setAcceptsTerms] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [emailTouched, setEmailTouched] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [checking, setChecking] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);

  const emailValid = isValidEmail(email);
  const lengthOk = password.length >= MIN_PASSWORD_LENGTH;
  const recommendedLength = password.length >= RECOMMENDED_PASSWORD_LENGTH;
  const passwordsMatch = password.length > 0 && password === confirmPassword;

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    const newErrors: Record<string, string> = {};

    if (!firstName.trim()) newErrors.firstName = 'Indica tu nombre.';
    if (!lastName.trim()) newErrors.lastName = 'Indica tus apellidos.';
    if (!isValidEmail(email)) newErrors.email = 'El email no parece válido.';
    if (!lengthOk) newErrors.password = `La contraseña debe tener al menos ${MIN_PASSWORD_LENGTH} caracteres.`;
    if (!passwordsMatch) newErrors.confirmPassword = 'Las contraseñas no coinciden.';
    if (!role) newErrors.role = 'Indica tu relación con el alumno/a.';
    if (!acceptsTerms) newErrors.terms = 'Debes aceptar los términos y la política de privacidad.';

    if (Object.keys(newErrors).length > 0) {
      setErrors(newErrors);
      return;
    }

    setChecking(true);
    const pwned = await isPasswordPwned(password);
    setChecking(false);

    if (pwned) {
      setErrors({ password: 'Esta contraseña aparece en filtraciones conocidas. Elige otra distinta.' });
      return;
    }

    setErrors({});
    setSubmitError(null);
    setSubmitting(true);
    try {
      const userId = ids.take('account');
      await register({
        id: userId,
        fullName: `${firstName.trim()} ${lastName.trim()}`,
        email: email.trim(),
        password,
      });
      ids.consume('account');
    } catch (error) {
      if (error instanceof AccountCreatedLoginFailedError) {
        ids.consume('account');
        setSubmitError(`Tu cuenta se ha creado, pero no hemos podido iniciar sesión. ${getApiErrorMessage(error.loginError)} Prueba a iniciar sesión desde la pantalla de acceso.`);
      } else {
        // A 409 means the id or the email already exists: retrying with the same id would only repeat it.
        if (getApiErrorStatus(error) === 409) ids.consume('account');
        setSubmitError(getApiErrorMessage(error));
      }
      setSubmitting(false);
      return;
    }

    const firstLastName = lastName.trim().split(/\s+/)[0] ?? '';
    setGuardianInfo({ name: `${firstName.trim()} ${firstLastName}`.trim(), role: role as GuardianRole });
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
                <label htmlFor="firstName" className="mb-1.5 block text-sm font-semibold text-slate-700 dark:text-slate-300">
                  Nombre
                </label>
                <div className="flex items-center gap-2 rounded-xl border border-slate-200 bg-slate-50 px-3 py-2.5 transition-colors focus-within:border-brand-400 focus-within:bg-white focus-within:ring-2 focus-within:ring-brand-100 dark:border-slate-700 dark:bg-slate-800/60 dark:focus-within:border-brand-500 dark:focus-within:bg-slate-900 dark:focus-within:ring-brand-900/40">
                  <UserRound className="h-4 w-4 shrink-0 text-slate-400" />
                  <input
                    id="firstName"
                    type="text"
                    value={firstName}
                    onChange={(e) => setFirstName(e.target.value)}
                    placeholder="Ana"
                    className="w-full bg-transparent text-sm text-slate-700 placeholder:text-slate-400 focus:outline-none dark:text-slate-200"
                  />
                </div>
                {errors.firstName && <p className="mt-1 text-xs text-rose-600 dark:text-rose-400">{errors.firstName}</p>}
              </div>

              <div>
                <label htmlFor="lastName" className="mb-1.5 block text-sm font-semibold text-slate-700 dark:text-slate-300">
                  Apellidos
                </label>
                <div className="flex items-center gap-2 rounded-xl border border-slate-200 bg-slate-50 px-3 py-2.5 transition-colors focus-within:border-brand-400 focus-within:bg-white focus-within:ring-2 focus-within:ring-brand-100 dark:border-slate-700 dark:bg-slate-800/60 dark:focus-within:border-brand-500 dark:focus-within:bg-slate-900 dark:focus-within:ring-brand-900/40">
                  <UserRound className="h-4 w-4 shrink-0 text-slate-400" />
                  <input
                    id="lastName"
                    type="text"
                    value={lastName}
                    onChange={(e) => setLastName(e.target.value)}
                    placeholder="Ruiz Gómez"
                    className="w-full bg-transparent text-sm text-slate-700 placeholder:text-slate-400 focus:outline-none dark:text-slate-200"
                  />
                </div>
                {errors.lastName && <p className="mt-1 text-xs text-rose-600 dark:text-rose-400">{errors.lastName}</p>}
              </div>
            </div>

            <div>
              <label htmlFor="email" className="mb-1.5 block text-sm font-semibold text-slate-700 dark:text-slate-300">
                Correo electrónico
              </label>
              <div
                className={`flex items-center gap-2 rounded-xl border bg-slate-50 px-3 py-2.5 transition-colors focus-within:ring-2 dark:bg-slate-800/60 ${
                  emailTouched && email && !emailValid
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
                  emailValid ? (
                    <CheckCircle2 className="h-4 w-4 shrink-0 text-mint-500" />
                  ) : (
                    <AlertCircle className="h-4 w-4 shrink-0 text-rose-500" />
                  )
                )}
              </div>
              {((emailTouched && email && !emailValid) || errors.email) && (
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
                  <li className={lengthOk ? 'text-mint-600 dark:text-mint-400' : 'text-slate-500 dark:text-slate-400'}>
                    {lengthOk ? '✓' : '·'} Al menos {MIN_PASSWORD_LENGTH} caracteres
                  </li>
                  <li className={recommendedLength ? 'text-mint-600 dark:text-mint-400' : 'text-slate-400 dark:text-slate-500'}>
                    {recommendedLength ? '✓' : '·'} Recomendado: {RECOMMENDED_PASSWORD_LENGTH}+ caracteres
                  </li>
                </ul>
              )}
              {errors.password && <p className="mt-1 text-xs text-rose-600 dark:text-rose-400">{errors.password}</p>}
            </div>

            <div>
              <label htmlFor="confirmPassword" className="mb-1.5 block text-sm font-semibold text-slate-700 dark:text-slate-300">
                Repite la contraseña
              </label>
              <div className="flex items-center gap-2 rounded-xl border border-slate-200 bg-slate-50 px-3 py-2.5 transition-colors focus-within:border-brand-400 focus-within:bg-white focus-within:ring-2 focus-within:ring-brand-100 dark:border-slate-700 dark:bg-slate-800/60 dark:focus-within:border-brand-500 dark:focus-within:bg-slate-900 dark:focus-within:ring-brand-900/40">
                <Lock className="h-4 w-4 shrink-0 text-slate-400" />
                <input
                  id="confirmPassword"
                  type={showPassword ? 'text' : 'password'}
                  autoComplete="new-password"
                  value={confirmPassword}
                  onChange={(e) => setConfirmPassword(e.target.value)}
                  placeholder="••••••••"
                  className="w-full bg-transparent text-sm text-slate-700 placeholder:text-slate-400 focus:outline-none dark:text-slate-200"
                />
              </div>
              {errors.confirmPassword && <p className="mt-1 text-xs text-rose-600 dark:text-rose-400">{errors.confirmPassword}</p>}
            </div>

            <div>
              <p className="mb-1.5 text-sm font-semibold text-slate-700 dark:text-slate-300">Eres su...</p>
              <div className="grid grid-cols-3 gap-2">
                {roleOptions.map((opt) => (
                  <button
                    key={opt.value}
                    type="button"
                    onClick={() => setRole(opt.value)}
                    className={`rounded-xl border px-3 py-2 text-sm font-semibold transition-colors ${
                      role === opt.value
                        ? 'border-brand-400 bg-brand-50 text-brand-700 dark:border-brand-500 dark:bg-brand-900/30 dark:text-brand-300'
                        : 'border-slate-200 bg-slate-50 text-slate-600 hover:border-slate-300 dark:border-slate-700 dark:bg-slate-800/60 dark:text-slate-300 dark:hover:border-slate-600'
                    }`}
                  >
                    {opt.label}
                  </button>
                ))}
              </div>
              {errors.role && <p className="mt-1 text-xs text-rose-600 dark:text-rose-400">{errors.role}</p>}
            </div>

            <div>
              <label className="flex items-start gap-2.5 text-sm text-slate-600 dark:text-slate-400">
                <input
                  type="checkbox"
                  checked={acceptsTerms}
                  onChange={(e) => setAcceptsTerms(e.target.checked)}
                  className="mt-0.5 h-4 w-4 shrink-0 rounded border-slate-300 text-brand-600 focus:ring-brand-400 dark:border-slate-600 dark:bg-slate-800"
                />
                <span>
                  He leído y acepto los{' '}
                  <Link to="/terms" className="font-semibold text-brand-600 hover:text-brand-700 dark:text-brand-400">
                    Términos de uso
                  </Link>{' '}
                  y la{' '}
                  <Link to="/privacy" className="font-semibold text-brand-600 hover:text-brand-700 dark:text-brand-400">
                    Política de privacidad
                  </Link>
                  .
                </span>
              </label>
              {errors.terms && <p className="mt-1 text-xs text-rose-600 dark:text-rose-400">{errors.terms}</p>}
            </div>

            {submitError && (
              <p
                role="alert"
                className="rounded-xl border border-rose-200 bg-rose-50 px-3.5 py-2.5 text-sm text-rose-700 dark:border-rose-700/40 dark:bg-rose-900/20 dark:text-rose-300"
              >
                {submitError}
              </p>
            )}

            <button
              type="submit"
              disabled={checking || submitting}
              className="mt-2 flex w-full items-center justify-center gap-1.5 rounded-xl bg-gradient-to-r from-brand-500 to-brand-600 px-4 py-3 text-sm font-semibold text-white shadow-lg shadow-brand-500/25 transition-transform hover:scale-[1.02] active:scale-95 disabled:opacity-60 disabled:hover:scale-100"
            >
              {checking ? 'Comprobando contraseña…' : submitting ? 'Creando tu cuenta…' : 'Crear cuenta'}
              {!checking && !submitting && <ArrowRight className="h-4 w-4" />}
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
