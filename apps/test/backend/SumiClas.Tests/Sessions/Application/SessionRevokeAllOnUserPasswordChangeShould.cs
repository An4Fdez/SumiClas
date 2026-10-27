namespace SumiClas.Tests.Sessions.Application;

using AwesomeAssertions;
using SumiClas.Sessions.Application;
using SumiClas.Sessions.Domain;
using SumiClas.Shared.Domain.Events;
using SumiClas.Tests.Sessions.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class SessionRevokeAllOnUserPasswordChangeShould : SessionsUnitTestCase
{
    private readonly SessionRevokeAllOnUserPasswordChange _handler;

    public SessionRevokeAllOnUserPasswordChangeShould()
    {
        this._handler = new SessionRevokeAllOnUserPasswordChange(this.Repository.Object, this.Clock.Object);
    }

    [Test]
    public async Task revoke_every_session_of_the_user_and_none_of_the_others()
    {
        Guid userId = GuidMother.Random();
        Session first = SessionMother.With(userId);
        Session second = SessionMother.With(userId);
        Session ofAnotherUser = SessionMother.With();
        TimeSpan elapsed = TimeSpan.FromMinutes(1);
        this.ShouldSearchSessions(first, second, ofAnotherUser);
        this.AdvanceClock(elapsed);

        await this._handler.Handle(new UserPasswordChangedDomainEvent(userId, this.Clock.Object));

        first.RevokedAt.Should().Be(Now + elapsed);
        second.RevokedAt.Should().Be(Now + elapsed);
        ofAnotherUser.RevokedAt.Should().BeNull();
    }
}
