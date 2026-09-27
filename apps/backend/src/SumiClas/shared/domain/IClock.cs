namespace SumiClas.Shared.Domain;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
