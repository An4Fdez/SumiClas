namespace SumiClas.Shared.Domain;

/// <summary>
/// Temporary bridge while the modules move from <see cref="Result"/> to exceptions: a failed result that code already
/// moved to exceptions had to consume. It keeps the error, and the API answers it with the status the old mapping gave.
/// </summary>
public sealed class ResultFailureException(Error error) : DomainException(error.Code, error.Message)
{
    public Error Error { get; } = error;
}
