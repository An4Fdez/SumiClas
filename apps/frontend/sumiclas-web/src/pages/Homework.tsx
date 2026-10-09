import { useMemo, useRef, useState } from 'react';
import { BookOpen, Plus } from 'lucide-react';
import { Reveal } from '@/components/layout/Reveal';
import { AssignmentCard } from '@/components/app/AssignmentCard';
import { AssignmentDetailModal } from '@/components/app/AssignmentDetailModal';
import { AssignmentForm, type AssignmentFormData } from '@/components/app/AssignmentForm';
import { SubjectFilterDropdown } from '@/components/app/SubjectFilterDropdown';
import { useChildContext } from '@/hooks/useChildContext';
import { useClassAssignments } from '@/hooks/useClassAssignments';
import { useClassSubjects } from '@/hooks/useClassSubjects';
import { useDoneAssignments } from '@/hooks/useDoneAssignments';
import { useHomeworkRepositories } from '@/hooks/useHomeworkRepositories';
import { useUserRepository } from '@/hooks/useUserRepository';
import { useWizardIds } from '@/hooks/useWizardIds';
import { getApiErrorStatus, UserFacingError } from '@/lib/apiError';
import { badgeColorClasses } from '@/lib/colorClasses';
import { formatDayHeading, isSameCalendarDay } from '@/lib/date';
import { normalizeText } from '@/lib/text';
import type { Assignment } from '@/modules/assignments/domain/Assignment';
import type { Subject } from '@/modules/subjects/domain/Subject';
import type { Profile } from '@/modules/users/domain/Profile';

type StatusFilter = 'pending' | 'done' | null;

const TRANSIENT_CLIENT_ERRORS = [408, 429];
const NO_CLASS_ERROR = 'Este alumno todavía no está en ninguna clase.';
const SECURE_CONTEXT_ERROR = 'Tu navegador no permite crear entradas fuera de una conexión segura (HTTPS).';

const filterButtonClasses = (active: boolean) =>
  `rounded-full px-3.5 py-1.5 text-sm font-semibold transition-colors ${
    active
      ? 'bg-slate-900 text-white dark:bg-white dark:text-slate-900'
      : 'bg-slate-100 text-slate-600 hover:bg-slate-200 dark:bg-slate-800 dark:text-slate-300 dark:hover:bg-slate-700'
  }`;

export default function Homework() {
  const { currentChild, currentRole } = useChildContext();
  const classId = currentChild?.classId;
  const { assignments: assignmentRepository, subjects: subjectRepository } = useHomeworkRepositories();
  const userRepository = useUserRepository();
  const ids = useWizardIds();
  const { state, reload, loadMore, loadingMore, loadMoreError, addComment } = useClassAssignments(classId);
  const subjectsList = useClassSubjects(classId);
  const { done, toggle: toggleDone } = useDoneAssignments();
  const profile = useRef<Profile | null>(null);

  const [subjectFilters, setSubjectFilters] = useState<Set<string>>(new Set());
  const [statusFilter, setStatusFilter] = useState<StatusFilter>(null);
  const [openAssignmentId, setOpenAssignmentId] = useState<string | null>(null);
  const [formForClass, setFormForClass] = useState<string | null>(null);

  const loaded = useMemo(() => (state.status === 'ready' ? state.items : []), [state]);
  const hasMore = state.status === 'ready' && !!state.nextCursor;
  const subjects = useMemo(
    () => (subjectsList.state.status === 'ready' ? subjectsList.state.subjects : []),
    [subjectsList.state],
  );
  const openAssignment = loaded.find((a) => a.id === openAssignmentId) ?? null;
  const formOpen = !!classId && formForClass === classId;

  const visibleAssignments = useMemo(
    () =>
      loaded
        .filter((a) => subjectFilters.size === 0 || subjectFilters.has(a.subjectId))
        .filter((a) => {
          if (statusFilter === 'pending') return !done.has(a.id);
          if (statusFilter === 'done') return done.has(a.id);
          return true;
        }),
    [loaded, subjectFilters, statusFilter, done],
  );

  const groupsByDay = useMemo(() => {
    const groups: { date: Date; assignments: Assignment[] }[] = [];
    for (const assignment of visibleAssignments) {
      const group = groups.find((g) => isSameCalendarDay(g.date, assignment.dueDate));
      if (group) group.assignments.push(assignment);
      else groups.push({ date: assignment.dueDate, assignments: [assignment] });
    }
    return groups;
  }, [visibleAssignments]);

  const canPublishOfficial = currentRole === 'classRepresentative';
  const canCreate = !!classId && subjectsList.state.status === 'ready';

  const takeId = (key: string) => {
    try {
      return ids.take(key);
    } catch {
      throw new UserFacingError(SECURE_CONTEXT_ERROR);
    }
  };

  const settleId = (key: string, error: unknown) => {
    const status = getApiErrorStatus(error);
    if (status !== undefined && status < 500 && !TRANSIENT_CLIENT_ERRORS.includes(status)) ids.consume(key);
    return status;
  };

  const handleCreateAssignment = async (data: AssignmentFormData) => {
    if (!classId) throw new UserFacingError(NO_CLASS_ERROR);
    const key = `assignment:${classId}:${JSON.stringify(data)}`;
    const id = takeId(key);
    const [year, month, day] = data.dueDate.split('-').map(Number);
    try {
      await assignmentRepository.create({
        id,
        classId,
        subjectId: data.subjectId,
        title: data.title,
        description: data.description || undefined,
        dueDate: new Date(year, month - 1, day),
      });
    } catch (error) {
      if (settleId(key, error) === 409) {
        reload();
        throw new UserFacingError('Es posible que la tarea ya se haya creado. Revisa la lista antes de crear otra.');
      }
      throw error;
    }
    ids.consume(key);
    setFormForClass(null);
    reload();
  };

  const handleCreateSubject = async (name: string): Promise<Subject> => {
    if (!classId) throw new UserFacingError(NO_CLASS_ERROR);
    const existing = subjects.find((s) => normalizeText(s.label) === normalizeText(name));
    if (existing) return existing;
    const key = `subject:${classId}:${normalizeText(name)}`;
    const id = takeId(key);
    try {
      await subjectRepository.create({ id, classId, label: name });
    } catch (error) {
      if (settleId(key, error) === 409) {
        subjectsList.reload();
        throw new UserFacingError('Es posible que la asignatura ya se haya creado. Revisa la lista antes de crear otra.');
      }
      throw error;
    }
    ids.consume(key);
    const created: Subject = { id, classId, label: name };
    subjectsList.add(created);
    return created;
  };

  const handleAddComment = async (assignmentId: string, text: string) => {
    if (!profile.current) profile.current = await userRepository.getCurrent();
    const me = profile.current;
    await addComment(assignmentId, text, {
      authorId: me.id,
      authorName: me.fullName,
      authorRole: currentRole ?? undefined,
    });
  };

  const emptyMessage = hasMore
    ? 'Ninguna tarea de las cargadas coincide con los filtros. Pulsa «Cargar más» para buscar en más.'
    : loaded.length > 0
      ? 'Ninguna tarea coincide con los filtros.'
      : 'No hay deberes por ahora.';

  return (
    <div>
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <div
            className={`grid h-12 w-12 place-items-center rounded-xl ${badgeColorClasses.mint.bg} ${badgeColorClasses.mint.text}`}
          >
            <BookOpen className="h-6 w-6" />
          </div>
          <h1 className="mt-4 font-display text-2xl font-extrabold tracking-tight text-slate-900 dark:text-white sm:text-3xl">
            Deberes
          </h1>
          <p className="mt-2 truncate text-slate-600 dark:text-slate-400">
            Pizarra organizada por día y asignatura, con hilo corto de comentarios para dudas puntuales.
          </p>
        </div>

        {canCreate && (
          <button
            type="button"
            onClick={() => setFormForClass(classId ?? null)}
            className="inline-flex shrink-0 items-center gap-1.5 rounded-xl bg-gradient-to-r from-brand-500 to-brand-600 px-3.5 py-2.5 text-sm font-semibold text-white shadow-sm shadow-brand-500/25"
          >
            <Plus className="h-4 w-4" />
            Nuevo
          </button>
        )}
      </div>

      {!classId ? (
        <div className="mt-8 rounded-2xl border border-dashed border-slate-300 bg-white/60 p-8 text-center text-sm text-slate-500 dark:border-slate-700 dark:bg-slate-900/60 dark:text-slate-400">
          {currentChild
            ? `${currentChild.studentName} todavía no está en ninguna clase, así que no hay deberes que mostrar.`
            : 'Todavía no hay ningún alumno.'}
        </div>
      ) : (
        <>
          {subjectsList.state.status === 'error' && (
            <div
              role="alert"
              className="mt-4 rounded-2xl border border-rose-200 bg-rose-50/60 p-3 text-sm text-slate-700 dark:border-rose-700/40 dark:bg-rose-900/15 dark:text-slate-300"
            >
              {subjectsList.state.message}{' '}
              <button
                type="button"
                onClick={subjectsList.reload}
                className="font-semibold text-brand-600 hover:text-brand-700 dark:text-brand-400"
              >
                Reintentar
              </button>
            </div>
          )}

          <div className="mt-6 flex flex-wrap items-center justify-between gap-2">
            <SubjectFilterDropdown subjects={subjects} selected={subjectFilters} onChange={setSubjectFilters} />

            <div className="flex flex-wrap items-center gap-2">
              <button
                type="button"
                onClick={() => setStatusFilter((v) => (v === 'pending' ? null : 'pending'))}
                className={filterButtonClasses(statusFilter === 'pending')}
              >
                Pendientes
              </button>
              <button
                type="button"
                onClick={() => setStatusFilter((v) => (v === 'done' ? null : 'done'))}
                className={filterButtonClasses(statusFilter === 'done')}
              >
                Hechas
              </button>
            </div>
          </div>

          {state.status === 'loading' || state.status === 'idle' ? (
            <p className="mt-8 text-sm text-slate-500 dark:text-slate-400">Cargando los deberes…</p>
          ) : state.status === 'error' ? (
            <div
              role="alert"
              className="mt-8 rounded-2xl border border-rose-200 bg-rose-50/60 p-4 dark:border-rose-700/40 dark:bg-rose-900/15"
            >
              <p className="text-sm text-slate-700 dark:text-slate-300">{state.message}</p>
              <button
                type="button"
                onClick={reload}
                className="mt-3 text-sm font-semibold text-brand-600 hover:text-brand-700 dark:text-brand-400 dark:hover:text-brand-300"
              >
                Reintentar
              </button>
            </div>
          ) : (
            <>
              {groupsByDay.length === 0 ? (
                <div className="mt-8 rounded-2xl border border-dashed border-slate-300 bg-white/60 p-8 text-center text-sm text-slate-500 dark:border-slate-700 dark:bg-slate-900/60 dark:text-slate-400">
                  {emptyMessage}
                </div>
              ) : (
                <div className="mt-6 space-y-6">
                  {groupsByDay.map((group) => (
                    <Reveal key={group.date.toISOString()}>
                      <h3 className="text-sm font-bold text-slate-500 dark:text-slate-400">
                        {formatDayHeading(group.date)}
                      </h3>
                      <div className="mt-2 space-y-2">
                        {group.assignments.map((assignment) => (
                          <AssignmentCard
                            key={assignment.id}
                            assignment={assignment}
                            done={done.has(assignment.id)}
                            onOpen={() => setOpenAssignmentId(assignment.id)}
                            onToggleDone={() => toggleDone(assignment.id)}
                          />
                        ))}
                      </div>
                    </Reveal>
                  ))}
                </div>
              )}

              {hasMore && (
                <div className="mt-6 text-center">
                  <button
                    type="button"
                    onClick={() => void loadMore()}
                    disabled={loadingMore}
                    className="rounded-xl border border-slate-200 px-4 py-2 text-sm font-semibold text-slate-600 transition-colors hover:bg-slate-50 disabled:opacity-60 dark:border-slate-700 dark:text-slate-300 dark:hover:bg-slate-800"
                  >
                    {loadingMore ? 'Cargando…' : 'Cargar más'}
                  </button>
                  {loadMoreError && (
                    <p role="alert" className="mt-2 text-xs font-semibold text-rose-600 dark:text-rose-400">
                      {loadMoreError}
                    </p>
                  )}
                </div>
              )}
            </>
          )}
        </>
      )}

      {openAssignment && (
        <AssignmentDetailModal
          assignment={openAssignment}
          done={done.has(openAssignment.id)}
          onToggleDone={() => toggleDone(openAssignment.id)}
          onAddComment={(text) => handleAddComment(openAssignment.id, text)}
          onClose={() => setOpenAssignmentId(null)}
        />
      )}

      {formOpen && currentChild && (
        <AssignmentForm
          canPublishOfficial={canPublishOfficial}
          className={currentChild.className}
          subjects={subjects}
          onCreateSubject={handleCreateSubject}
          onSubmit={handleCreateAssignment}
          onClose={() => setFormForClass(null)}
        />
      )}
    </div>
  );
}
