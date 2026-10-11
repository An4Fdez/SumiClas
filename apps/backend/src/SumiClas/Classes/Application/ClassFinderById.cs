using MediatR;
using SumiClas.Classes.Domain.Services;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;

namespace SumiClas.Classes.Application;

public static class ClassFinderById
{
    public sealed record Query(Guid ClassId) : IQuery<Result<ClassResponse>>;

    public sealed class Finder(ClassByIdFinder finder) : IUseCase
    {
        public async Task<Result<ClassResponse>> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            var found = await finder.FindAsync(query.ClassId, ct);
            if (found.IsFailure)
            {
                return Result<ClassResponse>.Failure(found.Error!);
            }

            var schoolClass = found.Value;
            return Result<ClassResponse>.Success(new ClassResponse(schoolClass.Id, schoolClass.SchoolYearId, schoolClass.Name));
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, Result<ClassResponse>>
    {
        public Task<Result<ClassResponse>> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
