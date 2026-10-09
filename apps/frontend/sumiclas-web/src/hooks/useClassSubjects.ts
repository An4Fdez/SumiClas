import { useCallback, useEffect, useRef, useState } from 'react';
import { useHomeworkRepositories } from '@/hooks/useHomeworkRepositories';
import { getApiErrorMessage } from '@/lib/apiError';
import type { Subject } from '@/modules/subjects/domain/Subject';

type State =
  | { status: 'idle' }
  | { status: 'loading' }
  | { status: 'ready'; subjects: Subject[] }
  | { status: 'error'; message: string };

/**
 * The subjects of one class. They are not paged and have no defaults: the class delegate creates them.
 * `generation` keeps a late response or error of a previous class from reaching the current one.
 */
export function useClassSubjects(classId: string | undefined) {
  const { subjects: repository } = useHomeworkRepositories();
  const [state, setState] = useState<State>({ status: 'idle' });
  const [reloadKey, setReloadKey] = useState(0);
  const generation = useRef(0);

  useEffect(() => {
    const mine = ++generation.current;
    if (!classId) {
      setState({ status: 'idle' });
      return;
    }
    setState({ status: 'loading' });
    repository
      .listByClass(classId)
      .then((subjects) => {
        if (generation.current === mine) setState({ status: 'ready', subjects });
      })
      .catch((error: unknown) => {
        if (generation.current === mine) setState({ status: 'error', message: getApiErrorMessage(error) });
      });
    return () => {
      generation.current += 1;
    };
  }, [classId, repository, reloadKey]);

  const reload = useCallback(() => setReloadKey((key) => key + 1), []);

  const add = useCallback((subject: Subject) => {
    setState((prev) =>
      prev.status !== 'ready' || prev.subjects.some((known) => known.id === subject.id)
        ? prev
        : { status: 'ready', subjects: [...prev.subjects, subject] },
    );
  }, []);

  return { state, reload, add };
}
