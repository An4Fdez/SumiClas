using SumiClas.Shared.Domain;

namespace SumiClas.Subjects.Domain;

public sealed class SubjectSpecification : Specification<Subject>
{
    public SubjectSpecification(Guid? id = null, Guid? classId = null, string? label = null)
    {
        if (id is { } subjectId) AddCriteria(subject => subject.Id == subjectId);
        if (classId is { } schoolClass) AddCriteria(subject => subject.ClassId == schoolClass);
        if (label is not null) AddCriteria(subject => subject.Label == label);
    }
}
