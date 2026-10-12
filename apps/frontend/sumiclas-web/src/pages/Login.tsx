import { useState, type FormEvent } from 'react';
import { Link, Navigate, useNavigate } from 'react-router-dom';
import { Mail, Lock, ArrowRight, Eye, EyeOff } from 'lucide-react';
import { Reveal } from '@/components/layout/Reveal';
import { SessionLoading } from '@/components/layout/SessionLoading';
import { useAuth } from '@/hooks/useAuth';
import { getApiErrorMessage } from '@/lib/apiError';
import { setGuardianInfo } from '@/lib/auth';
import { isValidEmail } from '@/lib/validation';

// The login response only has the full name: show the first two words (name and first surname), like Register.
function displayName(fullName: string) {
  return fullName.trim().split(/\s+/).slice(0, 2).join(' ');
}

export default function Login() {
  const navigate = useNavigate();
  const { status, login } = useAuth();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  if (status === 'loading') return <SessionLoading />;
  if (status === 'authenticated') return <Navigate to="/home" replace />;

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    if (submitting) return;
    if (!isValidEmail(email) || !password) {
      setError('Introduce tu correo electrónico y tu contraseña.');
      return;
    }
    setError(null);
    setSubmitting(true);
    try {
      const session = await login({ email: email.trim(), password });
      // The role is only asked at sign-up and the API does not return it: fall back to the generic one.
      setGuardianInfo({ name: displayName(session.fullName), role: 'guardian' });
      navigate('/home');
    } catch (err) {
      setError(getApiErrorMessage(err));
      setSubmitting(false);
    }
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
              Bienvenido de nuevo
            </span>
            <h1 className="mt-5 font-display text-2xl font-extrabold tracking-tight text-slate-900 dark:text-white sm:text-3xl">
              Inicia sesión en tu{' '}
              <span className="text-gradient bg-gradient-to-r from-brand-500 via-mint-500 to-rose-500">
                clase
              </span>
            </h1>
            <p className="mt-2 text-sm text-slate-600 dark:text-slate-400">
              Accede a avisos, calendario y deberes de tu colegio.
            </p>
          </div>

          <form className="mt-8 space-y-4" onSubmit={handleSubmit} noValidate>
            <div>
              <label
                htmlFor="email"
                className="mb-1.5 block text-sm font-semibold text-slate-700 dark:text-slate-300"
              >
                Correo electrónico
              </label>
              <div className="flex items-center gap-2 rounded-xl border border-slate-200 bg-slate-50 px-3 py-2.5 transition-colors focus-within:border-brand-400 focus-within:bg-white focus-within:ring-2 focus-within:ring-brand-100 dark:border-slate-700 dark:bg-slate-800/60 dark:focus-within:border-brand-500 dark:focus-within:bg-slate-900 dark:focus-within:ring-brand-900/40">
                <Mail className="h-4 w-4 shrink-0 text-slate-400" />
                <input
                  id="email"
                  type="email"
                  autoComplete="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  placeholder="tu@email.com"
                  className="w-full bg-transparent text-sm text-slate-700 placeholder:text-slate-400 focus:outline-none dark:text-slate-200"
                />
              </div>
            </div>

            <div>
              <div className="mb-1.5 flex items-center justify-between">
                <label
                  htmlFor="password"
                  className="block text-sm font-semibold text-slate-700 dark:text-slate-300"
                >
                  Contraseña
                </label>
                <Link
                  to="/forgot-password"
                  className="text-xs font-semibold text-brand-600 hover:text-brand-700 dark:text-brand-400 dark:hover:text-brand-300"
                >
                  ¿La olvidaste?
                </Link>
              </div>
              <div className="flex items-center gap-2 rounded-xl border border-slate-200 bg-slate-50 px-3 py-2.5 transition-colors focus-within:border-brand-400 focus-within:bg-white focus-within:ring-2 focus-within:ring-brand-100 dark:border-slate-700 dark:bg-slate-800/60 dark:focus-within:border-brand-500 dark:focus-within:bg-slate-900 dark:focus-within:ring-brand-900/40">
                <Lock className="h-4 w-4 shrink-0 text-slate-400" />
                <input
                  id="password"
                  type={showPassword ? 'text' : 'password'}
                  autoComplete="current-password"
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
            </div>

            {error && (
              <p
                role="alert"
                className="rounded-xl border border-rose-200 bg-rose-50 px-3.5 py-2.5 text-sm text-rose-700 dark:border-rose-700/40 dark:bg-rose-900/20 dark:text-rose-300"
              >
                {error}
              </p>
            )}

            <button
              type="submit"
              disabled={submitting}
              className="mt-2 flex w-full items-center justify-center gap-1.5 rounded-xl bg-gradient-to-r from-brand-500 to-brand-600 px-4 py-3 text-sm font-semibold text-white shadow-lg shadow-brand-500/25 transition-transform hover:scale-[1.02] active:scale-95 disabled:opacity-60 disabled:hover:scale-100"
            >
              {submitting ? 'Iniciando sesión…' : 'Iniciar sesión'}
              {!submitting && <ArrowRight className="h-4 w-4" />}
            </button>
          </form>

          <p className="mt-6 text-center text-sm text-slate-600 dark:text-slate-400">
            ¿No tienes cuenta?{' '}
            <Link
              to="/register"
              className="font-semibold text-brand-600 hover:text-brand-700 dark:text-brand-400 dark:hover:text-brand-300"
            >
              Regístrate
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
