using SumiClas.Shared.Domain;

namespace SumiClas.Shared.Application;

/// <summary>
/// Reacts to a domain event inside the same transaction as the command that raised it.
/// An exception aborts the whole unit of work: nothing is committed.
/// Name implementations "&lt;WhatItDoes&gt;On&lt;Event&gt;", e.g. UserClassFirstRepresentativeAssignOnClassCreate.
/// Handlers must not save: they only modify tracked entities, and the unit of work saves everything at once.
/// </summary>
public interface IDomainEventHandler<in TEvent> where TEvent : DomainEventBase
{
    Task Handle(TEvent domainEvent, CancellationToken ct = default);
}
