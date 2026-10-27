import { useState, type FormEvent } from 'react';
import { Check } from 'lucide-react';
import { useChildContext } from '@/hooks/useChildContext';
import { useUserRepository } from '@/hooks/useUserRepository';
import { getApiErrorMessage } from '@/lib/apiError';
import { getGuardianInfo, setGuardianInfo, toGuardianDisplayName } from '@/lib/auth';
import { MAX_FULL_NAME_LENGTH } from '@/lib/validation';
import { TextField } from './TextField';

const SAVED_MESSAGE = 'Nombre actualizado.';
const EMPTY_NAME_MESSAGE = 'Indica tu nombre.';

export function ProfileNameForm() {
  const userRepository = useUserRepository();
  const { currentUserName, currentUserEmail, refresh } = useChildContext();
  const [fullName, setFullName] = useState(currentUserName ?? '');
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [saved, setSaved] = useState(false);

  const trimmed = fullName.trim();
  const unchanged = trimmed === (currentUserName ?? '');

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    if (saving || unchanged) return;
    if (!trimmed) {
      setError(EMPTY_NAME_MESSAGE);
      return;
    }
    setError(null);
    setSaved(false);
    setSaving(true);
    try {
      await userRepository.changeName(trimmed);
      setGuardianInfo({ name: toGuardianDisplayName(trimmed), role: getGuardianInfo()?.role ?? 'guardian' });
      await refresh();
      setFullName(trimmed);
      setSaved(true);
    } catch (err) {
      setError(getApiErrorMessage(err));
    } finally {
      setSaving(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} noValidate className="space-y-4">
      <TextField
        id="profile-full-name"
        label="Nombre completo"
        value={fullName}
        onChange={(value) => {
          setFullName(value);
          setSaved(false);
          setError(null);
        }}
        autoComplete="name"
        maxLength={MAX_FULL_NAME_LENGTH}
        required
        error={error ?? undefined}
      />
      {currentUserEmail && (
        <TextField
          id="profile-email"
          type="email"
          label="Correo electrónico"
          value={currentUserEmail}
          readOnly
          hint="De momento el correo no se puede cambiar."
        />
      )}
      <div className="flex flex-wrap items-center gap-3">
        <button
          type="submit"
          disabled={saving || unchanged}
          className="rounded-xl bg-gradient-to-r from-brand-500 to-brand-600 px-4 py-2.5 text-sm font-semibold text-white shadow-sm shadow-brand-500/25 disabled:cursor-not-allowed disabled:opacity-50"
        >
          {saving ? 'Guardando…' : 'Guardar nombre'}
        </button>
        {saved && (
          <p role="status" className="inline-flex items-center gap-1.5 text-sm font-semibold text-mint-700 dark:text-mint-300">
            <Check className="h-4 w-4" />
            {SAVED_MESSAGE}
          </p>
        )}
      </div>
    </form>
  );
}
