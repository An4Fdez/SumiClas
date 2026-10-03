using MediatR;
using SumiClas.Classes.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Subjects.Domain;

namespace SumiClas.Subjects.Application;

public static class SubjectFinderByClass
{
    public sealed record Query(Guid ClassId) : IQuery<Result<IReadOnlyCollection<SubjectResponse>>>;

    public sealed class Handler(ISubjectRepository subjectRepository, IQueryBus queryBus)
        : IRequestHandler<Query, Result<IReadOnlyCollection<SubjectResponse>>>
    {
        public async Task<Result<IReadOnlyCollection<SubjectResponse>>> Handle(Query query, CancellationToken ct)
        {
            var schoolClass = await queryBus.Send(new ClassFinderById.Query(query.ClassId), ct);
            if (schoolClass.IsFailure)
            {
                return Result<IReadOnlyCollection<SubjectResponse>>.Failure(schoolClass.Error!);
            }

            var subjects = await subjectRepository.SearchAsync(new SubjectSpecification(classId: query.ClassId), ct);
            IReadOnlyCollection<SubjectResponse> response = subjects
                .OrderBy(subject => subject.Label)
                .Select(subject => new SubjectResponse(subject.Id, subject.ClassId, subject.Label))
                .ToList();

            return Result<IReadOnlyCollection<SubjectResponse>>.Success(response);
        }
    }
}
