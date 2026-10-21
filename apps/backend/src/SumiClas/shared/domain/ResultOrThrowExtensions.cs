namespace SumiClas.Shared.Domain;

/// <summary>
/// Temporary bridge: lets code already moved to exceptions consume a use case that still returns a result.
/// </summary>
public static class ResultOrThrowExtensions
{
    public static void OrThrow(this Result result)
    {
        if (result.IsFailure)
        {
            throw new ResultFailureException(result.Error!);
        }
    }

    public static T OrThrow<T>(this Result<T> result)
        => result.IsSuccess ? result.Value : throw new ResultFailureException(result.Error!);
}
