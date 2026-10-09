using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Students.Domain;

namespace SumiClas.Students.Application;

public static class StudentGuardianFinderByStudents
{
    /// <summary>Returns only the ids of the guardians of the given students: no data about the students leaves this module.</summary>
    public sealed record Query(IReadOnlyCollection<Guid> StudentIds) : IQuery<IReadOnlyCollection<Guid>>;

    public sealed class Handler(IStudentRepository studentRepository) : IRequestHandler<Query, IReadOnlyCollection<Guid>>
    {
        public async Task<IReadOnlyCollection<Guid>> Handle(Query query, CancellationToken ct)
        {
            if (query.StudentIds.Count == 0)
            {
                return [];
            }

            var students = await studentRepository.SearchAsync(new StudentSpecification(ids: query.StudentIds), ct);
            return students
                .SelectMany(student => student.Guardians.Select(link => link.GuardianId))
                .Distinct()
                .ToList();
        }
    }
}
