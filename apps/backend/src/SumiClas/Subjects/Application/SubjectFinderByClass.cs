using MediatR;
using SumiClas.Classes.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Subjects.Domain;

namespace SumiClas.Subjects.Application;

public static class SubjectFinderByClass
{
    public sealed record Query(Guid ClassId) : IQuery<Result<IReadOnlyCollection<SubjectResponse>>>;

    public sealed class Finder(ISubjectRepository repository, IQueryBus queryBus) : IUseCase
    {
        public async Task<Result<IReadOnlyCollection<SubjectResponse>>> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            var schoolClass = await queryBus.Send(new ClassFinderById.Query(query.ClassId), ct);
            if (schoolClass.IsFailure)
            {
                return Result<IReadOnlyCollection<SubjectResponse>>.Failure(schoolClass.Error!);
            }

            var subjects = await repository.SearchAsync(new SubjectSpecification(classId: query.ClassId), ct);
            IReadOnlyCollection<SubjectResponse> response = subjects
                .OrderBy(subject => subject.Label)
                .Select(subject => new SubjectResponse(subject.Id, subject.ClassId, subject.Label))
                .ToList();

            return Result<IReadOnlyCollection<SubjectResponse>>.Success(response);
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, Result<IReadOnlyCollection<SubjectResponse>>>
    {
        public Task<Result<IReadOnlyCollection<SubjectResponse>>> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
