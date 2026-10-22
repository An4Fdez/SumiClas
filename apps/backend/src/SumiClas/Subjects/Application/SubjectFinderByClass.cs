using MediatR;
using SumiClas.Classes.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;
using SumiClas.Subjects.Domain;

namespace SumiClas.Subjects.Application;

public static class SubjectFinderByClass
{
    public sealed record Query(Guid ClassId) : IQuery<IReadOnlyCollection<SubjectResponse>>;

    public sealed class Finder(ISubjectRepository repository, IQueryBus queryBus, IUserProvider userProvider) : IUseCase
    {
        public async Task<IReadOnlyCollection<SubjectResponse>> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            await queryBus.Send(new ClassFinderById.Query(query.ClassId), ct);

            var current = userProvider.GetCurrentUserId();

            await queryBus.Send(new UserClassAccessEnsure.Query(current, query.ClassId), ct);

            var subjects = await repository.SearchAsync(new SubjectSpecification(classId: query.ClassId), ct);
            return subjects
                .OrderBy(subject => subject.Label)
                .Select(subject => new SubjectResponse(subject.Id, subject.ClassId, subject.Label))
                .ToList();
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, IReadOnlyCollection<SubjectResponse>>
    {
        public Task<IReadOnlyCollection<SubjectResponse>> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
