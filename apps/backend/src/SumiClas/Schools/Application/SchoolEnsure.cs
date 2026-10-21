using MediatR;
using SumiClas.Schools.Domain;
using SumiClas.Schools.Domain.Services;
using SumiClas.Shared.Application;

namespace SumiClas.Schools.Application;

public static class SchoolEnsure
{
    /// <summary>
    /// Completes when the school exists and throws <see cref="SchoolExceptions.NotFound"/> otherwise. Other modules
    /// ask this before they rely on a school.
    /// </summary>
    public sealed record Query(Guid SchoolId) : IQuery<Unit>;

    public sealed class Ensure(SchoolFinder finder) : IUseCase
    {
        public async Task<Unit> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            await finder.FindByIdAsync(query.SchoolId, ct);
            return Unit.Value;
        }
    }

    public sealed class Handler(Ensure ensure) : IRequestHandler<Query, Unit>
    {
        public Task<Unit> Handle(Query query, CancellationToken ct)
            => ensure.ExecuteAsync(query, ct);
    }
}
