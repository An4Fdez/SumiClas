import { useCallback, useEffect, useRef, useState } from 'react';
import { useClassExpenseRepository } from '@/hooks/useClassExpenseRepository';
import { getApiErrorMessage } from '@/lib/apiError';
import type { ClassExpense } from '@/modules/classExpenses/domain/ClassExpense';

type State =
  | { status: 'idle' }
  | { status: 'loading' }
  | { status: 'ready'; items: ClassExpense[]; nextCursor?: string }
  | { status: 'error'; message: string };

function withPaid(state: State, expenseId: string, guardianId: string, hasPaid: boolean): State {
  if (state.status !== 'ready') return state;
  return {
    ...state,
    items: state.items.map((expense) =>
      expense.id === expenseId
        ? {
            ...expense,
            participants: expense.participants.map((p) => (p.guardianId === guardianId ? { ...p, hasPaid } : p)),
          }
        : expense,
    ),
  };
}

/**
 * The expenses of one class, newest first, loaded page by page. `generation` changes whenever the class or the
 * list is reloaded, so a late response or a late error of a previous load never reaches the current one.
 */
export function useClassExpenses(classId: string | undefined) {
  const repository = useClassExpenseRepository();
  const [state, setState] = useState<State>({ status: 'idle' });
  const [reloadKey, setReloadKey] = useState(0);
  const [loadingMore, setLoadingMore] = useState(false);
  const [loadMoreError, setLoadMoreError] = useState<string | null>(null);
  const [paymentError, setPaymentError] = useState<string | null>(null);
  const generation = useRef(0);
  const paying = useRef(new Set<string>());

  useEffect(() => {
    const mine = ++generation.current;
    setLoadingMore(false);
    setLoadMoreError(null);
    setPaymentError(null);
    paying.current.clear();
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

  const setPaid = useCallback(
    (expenseId: string, guardianId: string, paid: boolean) => {
      const key = `${expenseId}:${guardianId}`;
      if (paying.current.has(key)) return;
      paying.current.add(key);
      const mine = generation.current;
      setPaymentError(null);
      setState((prev) => withPaid(prev, expenseId, guardianId, paid));
      const request = paid ? repository.markAsPaid(expenseId, guardianId) : repository.markAsUnpaid(expenseId, guardianId);
      request
        .catch((error: unknown) => {
          if (generation.current !== mine) return;
          setState((prev) => withPaid(prev, expenseId, guardianId, !paid));
          setPaymentError(getApiErrorMessage(error));
        })
        .finally(() => paying.current.delete(key));
    },
    [repository],
  );

  return { state, reload, loadMore, loadingMore, loadMoreError, setPaid, paymentError };
}
