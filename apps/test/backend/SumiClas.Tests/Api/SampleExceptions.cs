namespace SumiClas.Tests.Api;

using SumiClas.Shared.Domain;

/// <summary>Domain exceptions that belong to no module, only to the tests of the API plumbing.</summary>
public static class SampleExceptions
{
    public const string NOT_FOUND_CODE = "Sample.NotFound";
    public const string NOT_FOUND_MESSAGE = "Nothing was found.";
    public const string FORBIDDEN_CODE = "Sample.Forbidden";
    public const string FORBIDDEN_MESSAGE = "Not allowed.";

    public sealed class NotFound() : DomainException(NOT_FOUND_CODE, NOT_FOUND_MESSAGE);

    public sealed class Forbidden() : DomainException(FORBIDDEN_CODE, FORBIDDEN_MESSAGE);
}
