using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Subjects.Domain.Services;

namespace SumiClas.Subjects.Application;

public static class SubjectFinderById
{
    public sealed record Query(Guid SubjectId) : IQuery<Result<SubjectResponse>>;

    public sealed record SubjectResponse(Guid Id, Guid ClassId, string Label);

    public sealed class Handler(SubjectByIdFinder finder) : IRequestHandler<Query, Result<SubjectResponse>>
    {
        public async Task<Result<SubjectResponse>> Handle(Query query, CancellationToken ct)
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
}
