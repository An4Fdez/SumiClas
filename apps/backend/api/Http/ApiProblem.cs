namespace SumiClas.Api.Http;

/// <summary>The standard ProblemDetails response of the API: the code is the title and the <c>code</c> extension.</summary>
public static class ApiProblem
{
    public static IResult Create(string code, string detail, int statusCode)
        => Results.Problem(
            title: code,
            detail: detail,
            statusCode: statusCode,
            extensions: new Dictionary<string, object?> { ["code"] = code });
}
