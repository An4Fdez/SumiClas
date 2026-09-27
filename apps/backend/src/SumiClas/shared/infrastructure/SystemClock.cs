using SumiClas.Shared.Domain;

namespace SumiClas.Shared.Infrastructure;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
