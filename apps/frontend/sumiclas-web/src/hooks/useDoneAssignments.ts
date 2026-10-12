import { useCallback, useState } from 'react';
import { useAuth } from '@/hooks/useAuth';

const KEY_PREFIX = 'sumiclas-homework-done:';

function read(key: string): Set<string> {
  try {
    const parsed: unknown = JSON.parse(localStorage.getItem(key) ?? '[]');
    return new Set(Array.isArray(parsed) ? parsed.filter((id): id is string => typeof id === 'string') : []);
  } catch {
    return new Set();
  }
}

/**
 * "Hecho" marks are a per-device convenience: the API has no such state, so they live in localStorage keyed by
 * the current user and the assignment id.
 */
export function useDoneAssignments() {
  const { user } = useAuth();
  const key = `${KEY_PREFIX}${user?.userId ?? 'anonymous'}`;
  const [done, setDone] = useState(() => read(key));

  const toggle = useCallback(
    (assignmentId: string) => {
      setDone((prev) => {
        const next = new Set(prev);
        if (next.has(assignmentId)) next.delete(assignmentId);
        else next.add(assignmentId);
        try {
          localStorage.setItem(key, JSON.stringify([...next]));
        } catch {
          // localStorage unavailable — the mark only lasts until the page is closed
        }
        return next;
      });
    },
    [key],
  );

  return { done, toggle };
}
