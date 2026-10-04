import { useCallback, useEffect, useRef, useState } from 'react';
import { useLostItemRepository } from '@/hooks/useLostItemRepository';
import { getApiErrorMessage } from '@/lib/apiError';
import type { LostItem } from '@/modules/lostItems/domain/ILostItemRepository';

type State =
  | { status: 'idle' }
  | { status: 'loading' }
  | { status: 'ready'; items: LostItem[]; nextCursor?: string }
  | { status: 'error'; message: string };

function withRecovered(state: State, id: string, recovered: boolean): State {
  if (state.status !== 'ready') return state;
  return { ...state, items: state.items.map((item) => (item.id === id ? { ...item, recovered } : item)) };
}

/**
 * The lost and found items of one class (school-wide ones included), newest first, loaded page by page.
 * `generation` changes whenever the class or the list is reloaded, so a late response or a late error of a
 * previous load never reaches the current one.
 */
export function useClassLostItems(classId: string | undefined) {
  const repository = useLostItemRepository();
  const [state, setState] = useState<State>({ status: 'idle' });
  const [reloadKey, setReloadKey] = useState(0);
  const [loadingMore, setLoadingMore] = useState(false);
  const [loadMoreError, setLoadMoreError] = useState<string | null>(null);
  const [toggleError, setToggleError] = useState<string | null>(null);
  const generation = useRef(0);
  const toggling = useRef(new Set<string>());

  useEffect(() => {
    const mine = ++generation.current;
    setLoadingMore(false);
    setLoadMoreError(null);
    setToggleError(null);
    if (!classId) {
      setState({ status: 'idle' });
      return;
    }
    setState({ status: 'loading' });
    repository
      .listByClass(classId)
      .then((page) => {
        if (generation.current === mine) setState({ status: 'ready', items: page.items, nextCursor: page.nextCursor });
      })
      .catch((error: unknown) => {
        if (generation.current === mine) setState({ status: 'error', message: getApiErrorMessage(error) });
      });
    return () => {
      generation.current += 1;
    };
  }, [classId, repository, reloadKey]);

  const reload = useCallback(() => setReloadKey((key) => key + 1), []);

  const loadMore = useCallback(async () => {
    if (!classId || state.status !== 'ready' || !state.nextCursor || loadingMore) return;
    const mine = generation.current;
    setLoadingMore(true);
    setLoadMoreError(null);
    try {
      const page = await repository.listByClass(classId, { cursor: state.nextCursor });
      if (generation.current !== mine) return;
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
      if (generation.current === mine) setLoadMoreError(getApiErrorMessage(error));
    } finally {
      if (generation.current === mine) setLoadingMore(false);
    }
  }, [classId, repository, state, loadingMore]);

  const setRecovered = useCallback(
    (id: string, recovered: boolean) => {
      if (toggling.current.has(id)) return;
      toggling.current.add(id);
      const mine = generation.current;
      setToggleError(null);
      setState((prev) => withRecovered(prev, id, recovered));
      const request = recovered ? repository.markAsRecovered(id) : repository.markAsNotRecovered(id);
      request
        .catch((error: unknown) => {
          if (generation.current !== mine) return;
          setState((prev) => withRecovered(prev, id, !recovered));
          setToggleError(getApiErrorMessage(error));
        })
        .finally(() => toggling.current.delete(id));
    },
    [repository],
  );

  return { state, reload, loadMore, loadingMore, loadMoreError, setRecovered, toggleError };
}
