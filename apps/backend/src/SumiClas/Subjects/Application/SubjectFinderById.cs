using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Subjects.Domain.Services;

namespace SumiClas.Subjects.Application;

public static class SubjectFinderById
{
    public sealed record Query(Guid SubjectId) : IQuery<Result<SubjectResponse>>;

    public sealed class Finder(SubjectByIdFinder finder) : IUseCase
    {
        public async Task<Result<SubjectResponse>> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            var found = await finder.FindAsync(query.SubjectId, ct);
            if (found.IsFailure)
            {
                return Result<SubjectResponse>.Failure(found.Error!);
            }

            var subject = found.Value;
            return Result<SubjectResponse>.Success(new SubjectResponse(subject.Id, subject.ClassId, subject.Label));
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, Result<SubjectResponse>>
    {
        public Task<Result<SubjectResponse>> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
