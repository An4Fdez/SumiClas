namespace SumiClas.Tests.Shared.Application;

using MediatR;
using SumiClas.Shared.Application;

public static class UnitOfWorkCommandMother
{
    public static SampleCommand Sample() => new();

    /// <summary>A command that returns a value and throws to fail.</summary>
    public sealed record SampleCommand : ICommand, IRequest<int>;
}
