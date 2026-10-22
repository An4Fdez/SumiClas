using MediatR;
using SumiClas.Schools.Domain;
using SumiClas.Schools.Domain.Services;
using SumiClas.Shared.Application;

namespace SumiClas.Schools.Application;

public static class SchoolFinderById
{
    public sealed record Query(Guid SchoolId) : IQuery<SchoolResponse>;

    public sealed class Finder(SchoolFinder finder, IQueryBus queryBus) : IUseCase
    {
        public async Task<SchoolResponse> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            School school = await finder.FindByIdAsync(query.SchoolId, ct);

            IReadOnlyCollection<SchoolResponse> responses = await SchoolResponseFactory.CreateAsync(queryBus, [school], ct);
            return responses.Single();
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, SchoolResponse>
    {
        public Task<SchoolResponse> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
