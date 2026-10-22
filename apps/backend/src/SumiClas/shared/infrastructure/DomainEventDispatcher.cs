using Microsoft.Extensions.DependencyInjection;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Shared.Infrastructure.Persistence;

namespace SumiClas.Shared.Infrastructure;

public sealed class DomainEventDispatcher(SumiClasDbContext dbContext, IServiceProvider serviceProvider) : IDomainEventDispatcher
{
    public async Task DispatchAsync(CancellationToken ct = default)
    {
        while (true)
        {
            var aggregates = dbContext.ChangeTracker.Entries<AggregateRoot>()
                .Select(entry => entry.Entity)
                .Where(aggregate => aggregate.DomainEvents.Count > 0)
                .ToList();
            if (aggregates.Count == 0)
            {
                return;
            }

            var domainEvents = aggregates.SelectMany(aggregate => aggregate.DomainEvents).ToList();
            foreach (var aggregate in aggregates)
            {
                aggregate.ClearDomainEvents();
            }

            foreach (var domainEvent in domainEvents)
            {
                var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
                var handle = handlerType.GetMethod(nameof(IDomainEventHandler<DomainEventBase>.Handle))!;

                foreach (var handler in serviceProvider.GetServices(handlerType))
                {
                    await (Task)handle.Invoke(handler, [domainEvent, ct])!;
                }
            }
        }
    }
}
