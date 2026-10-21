using SumiClas.Shared.Domain;

namespace SumiClas.Subjects.Domain;

/// <summary>
/// Not a closed catalog: a class representative/teacher can create a new one when
/// creating an Assignment (Assignments module). Scoped per Class (recreated each
/// SchoolYear along with its Class), not shared across the whole school — see
/// AGENTS.md for the reasoning.
/// </summary>
public sealed class Subject : AggregateRoot
{
    public Guid ClassId { get; private set; }
    public string Label { get; private set; }

    public Subject(Guid id, Guid classId, string label) : base(id)
    {
        ClassId = classId;
        Label = label;
    }

    public static Subject Create(Guid id, Guid classId, string label)
    {
        Validate(id, classId, label);

        return new Subject(id, classId, label.Trim());
    }

    private static void Validate(Guid id, Guid classId, string label)
    {
        if (id == Guid.Empty)
        {
            throw new SubjectExceptions.IdRequired();
        }
        if (classId == Guid.Empty)
        {
            throw new SubjectExceptions.ClassRequired();
        }
        if (string.IsNullOrWhiteSpace(label))
        {
            throw new SubjectExceptions.LabelRequired();
        }
    }
}
