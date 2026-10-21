using MediatR;
using SumiClas.Classes.Domain.Services;
using SumiClas.Shared.Application;

namespace SumiClas.Classes.Application;

public static class ClassFinderById
{
    public sealed record Query(Guid ClassId) : IQuery<ClassResponse>;

    public sealed class Finder(ClassByIdFinder finder) : IUseCase
    {
        public async Task<ClassResponse> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            var schoolClass = await finder.FindAsync(query.ClassId, ct);
            return new ClassResponse(schoolClass.Id, schoolClass.SchoolYearId, schoolClass.Name);
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, ClassResponse>
    {
        public Task<ClassResponse> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
