namespace SumiClas.Tests.Shared;

using Moq;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;

public abstract class UnitTestCase
{
    public static readonly DateTimeOffset Now = new(2026, 10, 10, 12, 0, 0, TimeSpan.Zero);

    protected Mock<IClock> Clock { get; } = new();

    protected Mock<IQueryBus> QueryBus { get; } = new();

    protected Mock<IUserProvider> UserProvider { get; } = new();

    [SetUp]
    public void BaseSetUp()
    {
        this.Clock.Reset();
        this.QueryBus.Reset();
        this.UserProvider.Reset();
        this.Clock.Setup(clock => clock.UtcNow).Returns(Now);
    }

    protected void AdvanceClock(TimeSpan elapsedSinceNow)
        => this.Clock.Setup(clock => clock.UtcNow).Returns(Now + elapsedSinceNow);

    protected void ShouldResolveCurrentUser(Guid userId)
        => this.UserProvider
            .Setup(provider => provider.GetCurrentUserId())
            .Returns(userId);

    protected void ShouldHaveNoCurrentUser()
        => this.UserProvider
            .Setup(provider => provider.GetCurrentUserId())
            .Throws(new AuthenticationExceptions.Unauthenticated());
}
