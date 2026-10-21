using MediatR;
using SumiClas.Classes.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;
using SumiClas.Subjects.Domain;

namespace SumiClas.Subjects.Application;

public static class SubjectFinderByClass
{
    public sealed record Query(Guid ClassId) : IQuery<Result<IReadOnlyCollection<SubjectResponse>>>;

    public sealed class Finder(ISubjectRepository repository, IQueryBus queryBus, IUserProvider userProvider) : IUseCase
    {
        public async Task<Result<IReadOnlyCollection<SubjectResponse>>> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            await queryBus.Send(new ClassFinderById.Query(query.ClassId), ct);

            var current = userProvider.GetCurrentUserId();
            if (current.IsFailure)
            {
                return Result<IReadOnlyCollection<SubjectResponse>>.Failure(current.Error!);
            }

            var access = await queryBus.Send(new UserClassAccessEnsure.Query(current.Value, query.ClassId), ct);
            if (access.IsFailure)
            {
                return Result<IReadOnlyCollection<SubjectResponse>>.Failure(access.Error!);
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
