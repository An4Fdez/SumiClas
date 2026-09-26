import { useMemo, useState } from 'react';
import { BookOpen, Plus } from 'lucide-react';
import { Reveal } from '@/components/layout/Reveal';
import { AssignmentCard } from '@/components/app/AssignmentCard';
import { AssignmentDetailModal } from '@/components/app/AssignmentDetailModal';
import { AssignmentForm, type NewAssignment } from '@/components/app/AssignmentForm';
import { SubjectFilterDropdown } from '@/components/app/SubjectFilterDropdown';
import { useChildContext } from '@/hooks/useChildContext';
import { badgeColorClasses } from '@/lib/colorClasses';
import { getGuardianInfo } from '@/lib/auth';
import { roleMeta } from '@/lib/roles';
import { isSameCalendarDay, formatFullDay } from '@/lib/date';
import {
  availableSubjectsMock,
  findSubject,
  assignmentCommentsMock,
  createSubject,
  assignmentsMock,
  type SubjectInfo,
  type AssignmentComment,
  type Assignment,
} from '@/data/homeworkMock';

type StatusFilter = 'pending' | 'done' | null;

function dayLabel(date: Date): string {
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  const cmp = new Date(date);
  cmp.setHours(0, 0, 0, 0);
  const diffDays = Math.round((cmp.getTime() - today.getTime()) / 86_400_000);

  if (diffDays === 0) return 'Hoy';
  if (diffDays === 1) return 'Mañana';
  if (diffDays === -1) return 'Ayer';
  if (diffDays < 0) return `Atrasado · ${formatFullDay(date)}`;
  return formatFullDay(date);
}

export default function Homework() {
  const { children, currentChild, currentRole } = useChildContext();

  const [createdAssignments, setCreatedAssignments] = useState<Assignment[]>([]);
  const [createdComments, setCreatedComments] = useState<AssignmentComment[]>([]);
  const [createdSubjects, setCreatedSubjects] = useState<SubjectInfo[]>([]);
  const [done, setDone] = useState<Set<string>>(new Set());
  const [subjectFilters, setSubjectFilters] = useState<Set<string>>(new Set());
  const [statusFilter, setStatusFilter] = useState<StatusFilter>(null);
  const [openAssignment, setOpenAssignment] = useState<Assignment | null>(null);
  const [formOpen, setFormOpen] = useState(false);

  const allComments = useMemo(
    () => [...assignmentCommentsMock, ...createdComments],
    [createdComments]
  );

  const commentsForAssignment = (assignmentId: string) => allComments.filter((c) => c.assignmentId === assignmentId);

  const allSubjects = useMemo(
    () => [...availableSubjectsMock, ...createdSubjects],
    [createdSubjects]
  );

  const visibleAssignments = useMemo(() => {
    const myChildIds = new Set(children.map((c) => c.id));
    return [...assignmentsMock, ...createdAssignments]
      .filter((a) => myChildIds.has(a.childId))
      .filter((a) => (subjectFilters.size === 0 ? true : subjectFilters.has(a.subjectId)))
      .filter((a) => {
        if (statusFilter === 'pending') return !done.has(a.id);
        if (statusFilter === 'done') return done.has(a.id);
        return true;
      })
      .sort((a, b) => new Date(a.dueDate).getTime() - new Date(b.dueDate).getTime());
  }, [children, createdAssignments, subjectFilters, statusFilter, done]);

  const groupsByDay = useMemo(() => {
    const groups: { date: Date; assignments: Assignment[] }[] = [];
    for (const assignment of visibleAssignments) {
      const date = new Date(assignment.dueDate);
      const group = groups.find((g) => isSameCalendarDay(g.date, date));
      if (group) group.assignments.push(assignment);
      else groups.push({ date, assignments: [assignment] });
    }
    return groups;
  }, [visibleAssignments]);

  const toggleDone = (id: string) =>
    setDone((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });

  const canPublishOfficial = currentRole === 'classRepresentative';
  const canCreate = !!currentChild;

  const handleCreateAssignment = (data: NewAssignment) => {
    if (!currentChild) return;
    const created: Assignment = {
      id: `local-${Date.now()}`,
      childId: currentChild.id,
      subjectId: data.subjectId,
      title: data.title,
      description: data.description || undefined,
      dueDate: new Date(data.dueDate).toISOString(),
      authorName: getGuardianInfo()?.name ?? 'Tú',
      authorRole: canPublishOfficial ? roleMeta.classRepresentative.label : 'Familia',
      isProposal: !canPublishOfficial,
    };
    setCreatedAssignments((prev) => [...prev, created]);
    setFormOpen(false);
  };

  const handleCreateSubject = (name: string): SubjectInfo => {
    const created = createSubject(name, allSubjects);
    if (!allSubjects.some((s) => s.id === created.id)) {
      setCreatedSubjects((prev) => [...prev, created]);
    }
    return created;
  };

  const handleAddComment = (text: string) => {
    if (!openAssignment) return;
    const created: AssignmentComment = {
      id: `local-${Date.now()}`,
      assignmentId: openAssignment.id,
      authorName: getGuardianInfo()?.name ?? 'Tú',
      authorRole: canPublishOfficial ? roleMeta.classRepresentative.label : 'Familia',
      text,
      date: new Date().toISOString(),
    };
    setCreatedComments((prev) => [...prev, created]);
  };

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
            onClick={() => setFormOpen(true)}
            className="inline-flex shrink-0 items-center gap-1.5 rounded-xl bg-gradient-to-r from-brand-500 to-brand-600 px-3.5 py-2.5 text-sm font-semibold text-white shadow-sm shadow-brand-500/25"
          >
            <Plus className="h-4 w-4" />
            Nuevo
          </button>
        )}
      </div>

      <div className="mt-6 flex flex-wrap items-center justify-between gap-2">
        <SubjectFilterDropdown
          subjects={allSubjects}
          selected={subjectFilters}
          onChange={setSubjectFilters}
        />

        <div className="flex flex-wrap items-center gap-2">
          <button
            type="button"
            onClick={() => setStatusFilter((v) => (v === 'pending' ? null : 'pending'))}
            className={`rounded-full px-3.5 py-1.5 text-sm font-semibold transition-colors ${
              statusFilter === 'pending'
                ? 'bg-slate-900 text-white dark:bg-white dark:text-slate-900'
                : 'bg-slate-100 text-slate-600 hover:bg-slate-200 dark:bg-slate-800 dark:text-slate-300 dark:hover:bg-slate-700'
            }`}
          >
            Pendientes
          </button>
          <button
            type="button"
            onClick={() => setStatusFilter((v) => (v === 'done' ? null : 'done'))}
            className={`rounded-full px-3.5 py-1.5 text-sm font-semibold transition-colors ${
              statusFilter === 'done'
                ? 'bg-slate-900 text-white dark:bg-white dark:text-slate-900'
                : 'bg-slate-100 text-slate-600 hover:bg-slate-200 dark:bg-slate-800 dark:text-slate-300 dark:hover:bg-slate-700'
            }`}
          >
            Hechas
          </button>
        </div>
      </div>

      {groupsByDay.length === 0 ? (
        <div className="mt-8 rounded-2xl border border-dashed border-slate-300 bg-white/60 p-8 text-center text-sm text-slate-500 dark:border-slate-700 dark:bg-slate-900/60 dark:text-slate-400">
          No hay deberes pendientes.
        </div>
      ) : (
        <div className="mt-6 space-y-6">
          {groupsByDay.map((group) => (
            <Reveal key={group.date.toISOString()}>
              <h3 className="text-sm font-bold text-slate-500 dark:text-slate-400">{dayLabel(group.date)}</h3>
              <div className="mt-2 space-y-2">
                {group.assignments.map((assignment) => (
                  <AssignmentCard
                    key={assignment.id}
                    assignment={assignment}
                    subject={findSubject(assignment.subjectId, allSubjects)}
                    done={done.has(assignment.id)}
                    commentCount={commentsForAssignment(assignment.id).length}
                    onOpen={() => setOpenAssignment(assignment)}
                    onToggleDone={() => toggleDone(assignment.id)}
                  />
                ))}
              </div>
            </Reveal>
          ))}
        </div>
      )}

      {openAssignment && (
        <AssignmentDetailModal
          assignment={openAssignment}
          subject={findSubject(openAssignment.subjectId, allSubjects)}
          comments={commentsForAssignment(openAssignment.id)}
          done={done.has(openAssignment.id)}
          onToggleDone={() => toggleDone(openAssignment.id)}
          onAddComment={handleAddComment}
          onClose={() => setOpenAssignment(null)}
        />
      )}

      {formOpen && currentChild && (
        <AssignmentForm
          canPublishOfficial={canPublishOfficial}
          className={currentChild.className}
          subjects={allSubjects}
          onCreateSubject={handleCreateSubject}
          onSubmit={handleCreateAssignment}
          onClose={() => setFormOpen(false)}
        />
      )}
    </div>
  );
}
