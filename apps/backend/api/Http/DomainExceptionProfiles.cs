using SumiClas.Classes.Domain;
using SumiClas.Shared.Domain;

namespace SumiClas.Api.Http;

/// <summary>
/// Groups of <see cref="DomainExceptionEndpointExtensions.Maps{TException}"/> declarations that many endpoints share,
/// each one named after the reason the endpoint can raise them.
/// </summary>
public static class DomainExceptionProfiles
{
    /// <summary>The unit of work raises these when saving a command: a duplicated key or a lost race.</summary>
    public static RouteHandlerBuilder ConflictsOnSave(this RouteHandlerBuilder builder)
        => builder
            .Maps<SharedExceptions.AlreadyExists>(StatusCodes.Status409Conflict)
            .Maps<SharedExceptions.ConcurrencyConflict>(StatusCodes.Status409Conflict);

    /// <summary>The endpoint reads or changes a class, so the class may not exist.</summary>
    public static RouteHandlerBuilder RequiresClass(this RouteHandlerBuilder builder)
        => builder.Maps<ClassExceptions.NotFound>(StatusCodes.Status404NotFound);
}
