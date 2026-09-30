using MediatR;
using SumiClas.Shared.Application;

namespace SumiClas.Shared.Infrastructure;

public sealed class MediatRQueryBus(ISender sender) : IQueryBus
{
    public Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken ct = default)
        => sender.Send(query, ct);
}
