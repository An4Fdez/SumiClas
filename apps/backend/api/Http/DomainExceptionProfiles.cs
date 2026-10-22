using SumiClas.Classes.Domain;
using SumiClas.Schools.Domain;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Users.Domain;

namespace SumiClas.Api.Http;

/// <summary>
/// Groups of <see cref="DomainExceptionEndpointExtensions.Maps{TException}"/> declarations that many endpoints share,
/// each one named after the reason the endpoint can raise them.
/// </summary>
public static class DomainExceptionProfiles
{
    /// <summary>The endpoint acts on behalf of the current user, so nobody may be identified.</summary>
    public static RouteHandlerBuilder RequiresSession(this RouteHandlerBuilder builder)
        => builder.Maps<AuthenticationExceptions.Unauthenticated>(StatusCodes.Status401Unauthorized);

    /// <summary>The endpoint relies on a user other than the current one, who may not exist.</summary>
    public static RouteHandlerBuilder RequiresUser(this RouteHandlerBuilder builder)
        => builder.Maps<UserExceptions.NotFound>(StatusCodes.Status404NotFound);

    /// <summary>The endpoint reads or changes the data of a class, so the user must belong to it.</summary>
    public static RouteHandlerBuilder RequiresClassMembership(this RouteHandlerBuilder builder)
        => builder.Maps<UserExceptions.ClassAccessForbidden>(StatusCodes.Status403Forbidden);

    /// <summary>The endpoint does what only a delegate of the class may do.</summary>
    public static RouteHandlerBuilder RequiresClassRepresentative(this RouteHandlerBuilder builder)
        => builder.Maps<UserExceptions.ClassRepresentativeForbidden>(StatusCodes.Status403Forbidden);

    /// <summary>The unit of work raises these when saving a command: a duplicated key or a lost race.</summary>
    public static RouteHandlerBuilder ConflictsOnSave(this RouteHandlerBuilder builder)
        => builder
            .Maps<SharedExceptions.AlreadyExists>(StatusCodes.Status409Conflict)
            .Maps<SharedExceptions.ConcurrencyConflict>(StatusCodes.Status409Conflict);

    /// <summary>The endpoint relies on a school, so the school may not exist.</summary>
    public static RouteHandlerBuilder RequiresSchool(this RouteHandlerBuilder builder)
        => builder.Maps<SchoolExceptions.NotFound>(StatusCodes.Status404NotFound);

    /// <summary>The endpoint writes to something that hangs off a school year: it must exist and not be archived.</summary>
    public static RouteHandlerBuilder RequiresActiveSchoolYear(this RouteHandlerBuilder builder)
        => builder
            .Maps<SchoolYearExceptions.NotFound>(StatusCodes.Status404NotFound)
            .Maps<SchoolYearExceptions.Archived>(StatusCodes.Status400BadRequest);

    /// <summary>The endpoint reads or changes a class, so the class may not exist.</summary>
    public static RouteHandlerBuilder RequiresClass(this RouteHandlerBuilder builder)
        => builder.Maps<ClassExceptions.NotFound>(StatusCodes.Status404NotFound);
}
