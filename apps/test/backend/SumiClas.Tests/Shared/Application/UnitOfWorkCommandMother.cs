namespace SumiClas.Tests.Shared.Application;

using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;

public static class UnitOfWorkCommandMother
{
    public static PlainCommand Plain() => new();

    public static ValueCommand Value() => new();

    public static ConvertedCommand Converted() => new();

    public sealed record PlainCommand : ICommand, IRequest<Result>;

    public sealed record ValueCommand : ICommand, IRequest<Result<int>>;

    /// <summary>A command of a module already moved to exceptions: it returns its value and throws to fail.</summary>
    public sealed record ConvertedCommand : ICommand, IRequest<int>;
}
