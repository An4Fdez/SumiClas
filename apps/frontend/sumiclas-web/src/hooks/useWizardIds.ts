import { useCallback, useMemo, useRef } from 'react';

/**
 * The client generates every id and the API answers 500 on a duplicate id, so a retry must reuse the id of the
 * failed attempt (take) and an id must be discarded once its creation succeeded (consume).
 * crypto.randomUUID can throw outside secure contexts: call take() inside a try/catch.
 */
export function useWizardIds() {
  const pending = useRef(new Map<string, string>());

  const take = useCallback((key: string) => {
    let id = pending.current.get(key);
    if (!id) {
      id = crypto.randomUUID();
      pending.current.set(key, id);
    }
    return id;
  }, []);

  const consume = useCallback((key: string) => {
    pending.current.delete(key);
  }, []);

  return useMemo(() => ({ take, consume }), [take, consume]);
}

export type WizardIds = ReturnType<typeof useWizardIds>;
