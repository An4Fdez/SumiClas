using SumiClas.Shared.Domain;

namespace SumiClas.Api.Http;

public static class ResultExtensions
{
    /// <summary>
    /// Maps a failed <see cref="Result"/> to a standard ProblemDetails response.
    /// Every domain error is a validation failure (400) for now; map specific codes here once others exist (404, 409...).
    /// </summary>
    public static IResult ToProblem(this Result result)
    {
        var error = result.Error ?? throw new InvalidOperationException("A successful result has no error to map.");

        return Results.Problem(
            title: error.Code,
            detail: error.Message,
            statusCode: StatusCodes.Status400BadRequest,
            extensions: new Dictionary<string, object?> { ["code"] = error.Code });
    }
}
