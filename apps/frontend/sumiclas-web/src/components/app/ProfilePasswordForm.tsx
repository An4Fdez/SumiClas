import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '@/hooks/useAuth';
import { useUserRepository } from '@/hooks/useUserRepository';
import { getApiErrorMessage } from '@/lib/apiError';
import { PASSWORD_CHANGED_NOTICE, setLoginNotice } from '@/lib/auth';
import { MAX_PASSWORD_LENGTH, MIN_PASSWORD_LENGTH } from '@/lib/validation';
import { PasswordField } from './PasswordField';

type FieldErrors = { current?: string; next?: string; confirm?: string };

export function ProfilePasswordForm() {
  const navigate = useNavigate();
  const userRepository = useUserRepository();
  const { signOutLocally } = useAuth();
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [errors, setErrors] = useState<FieldErrors>({});
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    if (saving) return;
    const found: FieldErrors = {};
    if (!currentPassword) found.current = 'Introduce tu contraseña actual.';
    if (newPassword.length < MIN_PASSWORD_LENGTH) {
      found.next = `La contraseña nueva debe tener al menos ${MIN_PASSWORD_LENGTH} caracteres.`;
    }
    if (newPassword !== confirmPassword) found.confirm = 'Las contraseñas no coinciden.';
    setErrors(found);
    setSubmitError(null);
    if (Object.keys(found).length > 0) return;

    setSaving(true);
    try {
      await userRepository.changePassword(currentPassword, newPassword);
    } catch (err) {
      setSubmitError(getApiErrorMessage(err));
      setSaving(false);
      return;
    }
    setLoginNotice(PASSWORD_CHANGED_NOTICE);
    navigate('/login', { replace: true });
    signOutLocally();
  };

  return (
    <form onSubmit={handleSubmit} noValidate className="space-y-4">
      <PasswordField
        id="profile-current-password"
        label="Contraseña actual"
        value={currentPassword}
        onChange={setCurrentPassword}
        autoComplete="current-password"
        maxLength={MAX_PASSWORD_LENGTH}
        error={errors.current}
      />
      <PasswordField
        id="profile-new-password"
        label="Contraseña nueva"
        value={newPassword}
        onChange={setNewPassword}
        autoComplete="new-password"
        maxLength={MAX_PASSWORD_LENGTH}
        error={errors.next}
        hint={`Entre ${MIN_PASSWORD_LENGTH} y ${MAX_PASSWORD_LENGTH} caracteres.`}
      />
      <PasswordField
        id="profile-confirm-password"
        label="Repite la contraseña nueva"
        value={confirmPassword}
        onChange={setConfirmPassword}
        autoComplete="new-password"
        maxLength={MAX_PASSWORD_LENGTH}
        error={errors.confirm}
      />
      <p className="text-xs text-slate-500 dark:text-slate-400">
        Al cambiarla se cerrarán tus sesiones en todos los dispositivos y tendrás que volver a iniciar sesión.
      </p>
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
        disabled={saving}
        className="rounded-xl bg-gradient-to-r from-brand-500 to-brand-600 px-4 py-2.5 text-sm font-semibold text-white shadow-sm shadow-brand-500/25 disabled:cursor-not-allowed disabled:opacity-50"
      >
        {saving ? 'Cambiando…' : 'Cambiar contraseña'}
      </button>
    </form>
  );
}
