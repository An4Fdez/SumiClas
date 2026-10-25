import { useCallback, useEffect, useRef, useState } from 'react';
import { useAnnouncementRepository } from '@/hooks/useAnnouncementRepository';
import { getApiErrorMessage } from '@/lib/apiError';
import type { Announcement } from '@/modules/announcements/domain/Announcement';

type State =
  | { status: 'idle' }
  | { status: 'loading' }
  | { status: 'ready'; items: Announcement[]; nextCursor?: string }
  | { status: 'error'; message: string };

function withRead(state: State, id: string, read: boolean): State {
  if (state.status !== 'ready') return state;
  return { ...state, items: state.items.map((item) => (item.id === id ? { ...item, read } : item)) };
}

/** The announcements of one class (school-wide ones included), newest first, loaded page by page. */
export function useClassAnnouncements(classId: string | undefined) {
  const repository = useAnnouncementRepository();
  const [state, setState] = useState<State>({ status: 'idle' });
  const [reloadKey, setReloadKey] = useState(0);
  const [loadingMore, setLoadingMore] = useState(false);
  const [loadMoreError, setLoadMoreError] = useState<string | null>(null);
  const [readError, setReadError] = useState<string | null>(null);
  const currentClassId = useRef(classId);
  currentClassId.current = classId;

  useEffect(() => {
    setLoadMoreError(null);
    setReadError(null);
    if (!classId) {
      setState({ status: 'idle' });
      return;
    }
    let cancelled = false;
    setState({ status: 'loading' });
    repository
      .listByClass(classId)
      .then((page) => {
        if (!cancelled) setState({ status: 'ready', items: page.items, nextCursor: page.nextCursor });
      })
      .catch((error: unknown) => {
        if (!cancelled) setState({ status: 'error', message: getApiErrorMessage(error) });
      });
    return () => {
      cancelled = true;
    };
  }, [classId, repository, reloadKey]);

  const reload = useCallback(() => setReloadKey((key) => key + 1), []);

  const loadMore = useCallback(async () => {
    if (!classId || state.status !== 'ready' || !state.nextCursor || loadingMore) return;
    setLoadingMore(true);
    setLoadMoreError(null);
    try {
      const page = await repository.listByClass(classId, { cursor: state.nextCursor });
      if (currentClassId.current !== classId) return;
      setState((prev) =>
        prev.status !== 'ready'
          ? prev
          : {
              status: 'ready',
              items: [...prev.items, ...page.items.filter((item) => !prev.items.some((known) => known.id === item.id))],
              nextCursor: page.nextCursor,
            },
      );
    } catch (error) {
      setLoadMoreError(getApiErrorMessage(error));
    } finally {
      setLoadingMore(false);
    }
  }, [classId, repository, state, loadingMore]);

  const markRead = useCallback(
    (id: string) => {
      const requestedFor = currentClassId.current;
      setReadError(null);
      setState((prev) => withRead(prev, id, true));
      repository.markAsRead(id).catch((error: unknown) => {
        if (currentClassId.current !== requestedFor) return;
        setState((prev) => withRead(prev, id, false));
        setReadError(getApiErrorMessage(error));
      });
    },
    [repository],
  );

  return { state, reload, loadMore, loadingMore, loadMoreError, markRead, readError };
}
