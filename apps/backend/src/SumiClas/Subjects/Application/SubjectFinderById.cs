using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Subjects.Domain.Services;

namespace SumiClas.Subjects.Application;

public static class SubjectFinderById
{
    public sealed record Query(Guid SubjectId) : IQuery<SubjectResponse>;

    public sealed class Finder(SubjectByIdFinder finder) : IUseCase
    {
        public async Task<SubjectResponse> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            var subject = await finder.FindAsync(query.SubjectId, ct);
            return new SubjectResponse(subject.Id, subject.ClassId, subject.Label);
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, SubjectResponse>
    {
        public Task<SubjectResponse> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
