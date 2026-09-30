using SumiClas.Shared.Domain;

namespace SumiClas.Api.Http;

public static class ResultExtensions
{
    /// <summary>
    /// Maps a failed <see cref="Result"/> to a standard ProblemDetails response.
    /// Codes ending in ".NotFound" map to 404, codes ending in "AlreadyRegistered" or "AlreadyExists" map to 409,
    /// and every other domain error is a validation failure (400).
    /// </summary>
    public static IResult ToProblem(this Result result)
    {
        var error = result.Error ?? throw new InvalidOperationException("A successful result has no error to map.");

        var statusCode = error.Code switch
        {
            var code when code.EndsWith(".NotFound", StringComparison.Ordinal) => StatusCodes.Status404NotFound,
            var code when code.EndsWith("AlreadyRegistered", StringComparison.Ordinal)
                || code.EndsWith("AlreadyExists", StringComparison.Ordinal) => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest,
        };

        return Results.Problem(
            title: error.Code,
            detail: error.Message,
            statusCode: statusCode,
            extensions: new Dictionary<string, object?> { ["code"] = error.Code });
    }
}
