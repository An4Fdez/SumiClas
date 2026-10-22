using System.Reflection;
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
            List<AggregateRoot> aggregates = dbContext.ChangeTracker.Entries<AggregateRoot>()
                .Select(entry => entry.Entity)
                .Where(aggregate => aggregate.DomainEvents.Count > 0)
                .ToList();
            if (aggregates.Count == 0)
            {
                return;
            }

            List<DomainEventBase> domainEvents = aggregates.SelectMany(aggregate => aggregate.DomainEvents).ToList();
            foreach (AggregateRoot aggregate in aggregates)
            {
                aggregate.ClearDomainEvents();
            }

            foreach (DomainEventBase domainEvent in domainEvents)
            {
                Type handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
                MethodInfo handle = handlerType.GetMethod(nameof(IDomainEventHandler<DomainEventBase>.Handle))!;

                foreach (object? handler in serviceProvider.GetServices(handlerType))
                {
                    await (Task)handle.Invoke(handler, [domainEvent, ct])!;
                }
            }
        }
    }
}
