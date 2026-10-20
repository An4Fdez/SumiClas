namespace SumiClas.Shared.Domain;

/// <summary>
/// An expected failure of the domain. It carries only the stable code the clients branch on and a message written for
/// the user; the HTTP status is declared by each endpoint, not here. Every module defines its own exceptions, as nested
/// classes of its static <c>&lt;Module&gt;Exceptions</c> class, each one inheriting directly from this type.
/// </summary>
public abstract class DomainException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
