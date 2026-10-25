namespace SumiClas.Api.Http;

/// <summary>
/// The codes of the errors the API answers that are not domain exceptions. They belong to the API layer: they describe
/// what went wrong around the request (infrastructure, malformed input), not a rule of the business.
/// </summary>
public static class ApiErrorCodes
{
    public const string UnexpectedError = "Shared.UnexpectedError";
    public const string ServiceUnavailable = "Shared.ServiceUnavailable";
    public const string BadRequest = "Shared.BadRequest";
    public const string WeeklySummaryUnavailable = "WeeklySummary.Unavailable";
    public const string AuthenticationTooManyRequests = "Authentication.TooManyRequests";
}
