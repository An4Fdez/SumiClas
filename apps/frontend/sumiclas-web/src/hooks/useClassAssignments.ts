import { useCallback, useEffect, useRef, useState } from 'react';
import { useHomeworkRepositories } from '@/hooks/useHomeworkRepositories';
import { getApiErrorMessage } from '@/lib/apiError';
import type { Assignment } from '@/modules/assignments/domain/Assignment';
import type { AssignmentComment } from '@/modules/assignments/domain/AssignmentComment';

type State =
  | { status: 'idle' }
  | { status: 'loading' }
  | { status: 'ready'; items: Assignment[]; nextCursor?: string }
  | { status: 'error'; message: string };

type CommentAuthor = Pick<AssignmentComment, 'authorId' | 'authorName' | 'authorRole'>;

function mapComments(state: State, assignmentId: string, change: (comments: AssignmentComment[]) => AssignmentComment[]): State {
  if (state.status !== 'ready') return state;
  return {
    ...state,
    items: state.items.map((item) => (item.id === assignmentId ? { ...item, comments: change(item.comments) } : item)),
  };
}

/**
 * The assignments of one class, ordered by due date, loaded page by page. `generation` changes whenever the class
 * or the list is reloaded, so a late response or a late error of a previous load never reaches the current one.
 */
export function useClassAssignments(classId: string | undefined) {
  const { assignments: repository } = useHomeworkRepositories();
  const [state, setState] = useState<State>({ status: 'idle' });
  const [reloadKey, setReloadKey] = useState(0);
  const [loadingMore, setLoadingMore] = useState(false);
  const [loadMoreError, setLoadMoreError] = useState<string | null>(null);
  const generation = useRef(0);
  const pendingComments = useRef(0);

  useEffect(() => {
    const mine = ++generation.current;
    setLoadingMore(false);
    setLoadMoreError(null);
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

  const addComment = useCallback(
    async (assignmentId: string, text: string, author: CommentAuthor) => {
      const mine = generation.current;
      const pending: AssignmentComment = {
        id: `pending-${++pendingComments.current}`,
        ...author,
        text,
        createdAt: new Date(),
      };
      setState((prev) => mapComments(prev, assignmentId, (comments) => [...comments, pending]));
      try {
        await repository.addComment(assignmentId, text);
      } catch (error) {
        if (generation.current === mine) {
          setState((prev) => mapComments(prev, assignmentId, (comments) => comments.filter((c) => c.id !== pending.id)));
        }
        throw error;
      }
    },
    [repository],
  );

  return { state, reload, loadMore, loadingMore, loadMoreError, addComment };
}
