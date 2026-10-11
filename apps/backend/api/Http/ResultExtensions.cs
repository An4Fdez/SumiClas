using SumiClas.Shared.Domain;

namespace SumiClas.Api.Http;

public static class ResultExtensions
{
    /// <summary>
    /// Maps a failed <see cref="Result"/> to a standard ProblemDetails response.
    /// Codes ending in "NotFound" map to 404, codes ending in "AlreadyRegistered", "AlreadyExists" or "AlreadyConfirmed" map to 409,
    /// codes ending in "Unauthenticated", "InvalidCredentials" or "RefreshTokenInvalid" map to 401, codes ending in "Forbidden" map to 403, and every other domain error is a validation failure (400).
    /// </summary>
    public static IResult ToNoContent(this Result result)
        => result.IsSuccess ? Results.NoContent() : result.ToProblem();

    public static IResult ToOk<T>(this Result<T> result)
        => result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();

    public static IResult ToCreated(this Result result)
        => result.IsSuccess ? Results.StatusCode(StatusCodes.Status201Created) : result.ToProblem();

    public static IResult ToProblem(this Result result)
    {
        var error = result.Error ?? throw new InvalidOperationException("A successful result has no error to map.");

        var statusCode = error.Code switch
        {
            var code when code.EndsWith("NotFound", StringComparison.Ordinal) => StatusCodes.Status404NotFound,
            var code when code.EndsWith("AlreadyRegistered", StringComparison.Ordinal)
                || code.EndsWith("AlreadyExists", StringComparison.Ordinal)
                || code.EndsWith("AlreadyConfirmed", StringComparison.Ordinal) => StatusCodes.Status409Conflict,
            var code when code.EndsWith("Unauthenticated", StringComparison.Ordinal)
                || code.EndsWith("InvalidCredentials", StringComparison.Ordinal)
                || code.EndsWith("RefreshTokenInvalid", StringComparison.Ordinal) => StatusCodes.Status401Unauthorized,
            var code when code.EndsWith("Forbidden", StringComparison.Ordinal) => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status400BadRequest,
        };

        return Results.Problem(
            title: error.Code,
            detail: error.Message,
            statusCode: statusCode,
            extensions: new Dictionary<string, object?> { ["code"] = error.Code });
    }
}
