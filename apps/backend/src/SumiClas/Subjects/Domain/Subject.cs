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

    private Subject(Guid id, Guid classId, string label) : base(id)
    {
        ClassId = classId;
        Label = label;
    }

    public static Result<Subject> Create(Guid classId, string label)
    {
        var validation = Validate(classId, label);
        if (validation.IsFailure)
        {
            return Result<Subject>.Failure(validation.Error!);
        }

        var subject = new Subject(Guid.NewGuid(), classId, label.Trim());
        return Result<Subject>.Success(subject);
    }

    private static Result Validate(Guid classId, string label)
    {
        if (classId == Guid.Empty)
        {
            return Result.Failure(SubjectErrors.ClassRequired);
        }
        if (string.IsNullOrWhiteSpace(label))
        {
            return Result.Failure(SubjectErrors.LabelRequired);
        }
        return Result.Success();
    }
}
