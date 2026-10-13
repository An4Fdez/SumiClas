namespace SumiClas.Tests.Sessions.Domain;

using AwesomeAssertions;
using SumiClas.Sessions.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class SessionShould : UnitTestCase
{
    [Test]
    public void have_a_refresh_grace_of_10_seconds()
    {
        Session.RefreshGrace.Should().Be(TimeSpan.FromSeconds(10));
    }

    [Test]
    public void have_a_refresh_token_lifetime_of_14_days()
    {
        RefreshToken.Lifetime.Should().Be(TimeSpan.FromDays(14));
    }

    [Test]
    public void keep_the_fields_and_create_the_first_token_when_started_with_valid_data()
    {
        Guid userId = GuidMother.Random();
        string firstHash = StringMother.Random("hash");

        Result<Session> result = Session.Create(userId, firstHash, this.Clock.Object);

        result.IsSuccess.Should().BeTrue();
        Session session = result.Value;
        session.Id.Should().NotBe(Guid.Empty);
        session.UserId.Should().Be(userId);
        session.CreatedAt.Should().Be(Now);
        session.RevokedAt.Should().BeNull();
        RefreshToken token = session.RefreshTokens.Single();
        token.Id.Should().NotBe(Guid.Empty);
        token.Id.Should().NotBe(session.Id);
        token.TokenHash.Should().Be(firstHash);
        token.CreatedAt.Should().Be(Now);
        token.ExpiresAt.Should().Be(Now + RefreshToken.Lifetime);
        token.ReplacedAt.Should().BeNull();
        token.ReplacedById.Should().BeNull();
    }

    [Test]
    public void assign_a_different_id_on_each_start()
    {
        Guid userId = GuidMother.Random();
        string firstHash = StringMother.Random("hash");

        Session first = Session.Create(userId, firstHash, this.Clock.Object).Value;
        Session second = Session.Create(userId, firstHash, this.Clock.Object).Value;

        first.Id.Should().NotBe(Guid.Empty);
        second.Id.Should().NotBe(first.Id);
    }

    [Test]
    public void return_user_required_when_started_with_an_empty_user()
    {
        string firstHash = StringMother.Random("hash");

        Result<Session> result = Session.Create(Guid.Empty, firstHash, this.Clock.Object);

        result.Error.Should().Be(SessionErrors.UserRequired);
    }

    [TestCase("")]
    [TestCase("   ")]
    public void return_token_hash_required_when_started_with_a_blank_hash(string hash)
    {
        Guid userId = GuidMother.Random();

        Result<Session> result = Session.Create(userId, hash, this.Clock.Object);

        result.Error.Should().Be(SessionErrors.TokenHashRequired);
    }

    [Test]
    public void replace_the_active_token_and_add_the_successor_when_rotating_it()
    {
        string firstHash = StringMother.Random("hash");
        string secondHash = StringMother.Random("hash");
        Session session = this.NewSession(GuidMother.Random(), firstHash);
        Guid firstId = session.RefreshTokens.Single().Id;
        this.AdvanceClock(TimeSpan.FromMinutes(5));

        Result<SessionRotation> result = this.Rotate(session, firstHash, secondHash);

        result.IsSuccess.Should().BeTrue();
        SessionRotation.Rotated? rotated = result.Value as SessionRotation.Rotated;
        rotated.Should().NotBeNull();
        rotated!.NewToken.Id.Should().NotBe(Guid.Empty);
        rotated.NewToken.Id.Should().NotBe(firstId);
        rotated.NewToken.Id.Should().NotBe(session.Id);
        rotated.NewToken.TokenHash.Should().Be(secondHash);
        rotated.NewToken.ExpiresAt.Should().Be(Now + TimeSpan.FromMinutes(5) + RefreshToken.Lifetime);
        RefreshToken old = session.RefreshTokens.Single(token => token.Id == firstId);
        old.ReplacedAt.Should().Be(Now + TimeSpan.FromMinutes(5));
        old.ReplacedById.Should().Be(rotated.NewToken.Id);
        session.RefreshTokens.Should().HaveCount(2);
        session.RevokedAt.Should().BeNull();
    }

    [TestCase("", "new-hash")]
    [TestCase("old-hash", " ")]
    public void return_token_hash_required_when_rotating_with_a_blank_hash(string hash, string newHash)
    {
        Session session = this.NewSession(GuidMother.Random(), StringMother.Random("hash"));

        Result<SessionRotation> result = this.Rotate(session, hash, newHash);

        result.Error.Should().Be(SessionErrors.TokenHashRequired);
        session.RefreshTokens.Should().HaveCount(1);
    }

    [Test]
    public void be_invalid_and_change_nothing_when_rotating_an_unknown_hash()
    {
        Session session = this.NewSession(GuidMother.Random(), StringMother.Random("hash"));
        string unknownHash = StringMother.Random("hash");
        string secondHash = StringMother.Random("hash");

        Result<SessionRotation> result = this.Rotate(session, unknownHash, secondHash);

        result.Value.Should().BeOfType<SessionRotation.Invalid>();
        session.RefreshTokens.Should().HaveCount(1);
        session.RefreshTokens.Single().ReplacedAt.Should().BeNull();
        session.RevokedAt.Should().BeNull();
    }

    [Test]
    public void be_invalid_and_change_nothing_when_rotating_an_expired_token()
    {
        string firstHash = StringMother.Random("hash");
        string secondHash = StringMother.Random("hash");
        Session session = this.NewSession(GuidMother.Random(), firstHash);
        this.AdvanceClock(RefreshToken.Lifetime + TimeSpan.FromSeconds(1));

        Result<SessionRotation> result = this.Rotate(session, firstHash, secondHash);

        result.Value.Should().BeOfType<SessionRotation.Invalid>();
        session.RefreshTokens.Should().HaveCount(1);
        session.RefreshTokens.Single().ReplacedAt.Should().BeNull();
        session.RevokedAt.Should().BeNull();
    }

    [Test]
    public void be_invalid_and_change_nothing_when_rotating_a_token_replaced_within_the_grace()
    {
        string firstHash = StringMother.Random("hash");
        string secondHash = StringMother.Random("hash");
        string thirdHash = StringMother.Random("hash");
        Session session = this.NewSession(GuidMother.Random(), firstHash);
        this.Rotate(session, firstHash, secondHash);
        this.AdvanceClock(Session.RefreshGrace / 2);

        Result<SessionRotation> result = this.Rotate(session, firstHash, thirdHash);

        result.Value.Should().BeOfType<SessionRotation.Invalid>();
        session.RevokedAt.Should().BeNull();
        session.RefreshTokens.Should().HaveCount(2);
        session.RefreshTokens.Single(token => token.TokenHash == secondHash).IsActive(this.Clock.Object)
            .Should().BeTrue();
    }

    [Test]
    public void be_invalid_without_revoking_when_rotating_a_token_replaced_exactly_at_the_grace_limit()
    {
        string firstHash = StringMother.Random("hash");
        string secondHash = StringMother.Random("hash");
        string thirdHash = StringMother.Random("hash");
        Session session = this.NewSession(GuidMother.Random(), firstHash);
        this.Rotate(session, firstHash, secondHash);
        this.AdvanceClock(Session.RefreshGrace);

        Result<SessionRotation> result = this.Rotate(session, firstHash, thirdHash);

        result.Value.Should().BeOfType<SessionRotation.Invalid>();
        session.RevokedAt.Should().BeNull();
    }

    [Test]
    public void revoke_the_session_and_report_reuse_when_rotating_a_token_replaced_outside_the_grace()
    {
        string firstHash = StringMother.Random("hash");
        string secondHash = StringMother.Random("hash");
        string thirdHash = StringMother.Random("hash");
        TimeSpan pastGrace = Session.RefreshGrace + TimeSpan.FromSeconds(50);
        Session session = this.NewSession(GuidMother.Random(), firstHash);
        this.Rotate(session, firstHash, secondHash);
        this.AdvanceClock(pastGrace);

        Result<SessionRotation> result = this.Rotate(session, firstHash, thirdHash);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<SessionRotation.Reused>();
        session.RevokedAt.Should().Be(Now + pastGrace);
        session.IsActive(this.Clock.Object).Should().BeFalse();
        session.RefreshTokens.Should().HaveCount(2);
    }

    [Test]
    public void reject_the_successor_when_rotating_after_the_session_was_revoked_by_reuse()
    {
        string firstHash = StringMother.Random("hash");
        string secondHash = StringMother.Random("hash");
        string thirdHash = StringMother.Random("hash");
        string fourthHash = StringMother.Random("hash");
        Session session = this.NewSession(GuidMother.Random(), firstHash);
        this.Rotate(session, firstHash, secondHash);
        this.AdvanceClock(Session.RefreshGrace + TimeSpan.FromSeconds(50));
        this.Rotate(session, firstHash, thirdHash);

        Result<SessionRotation> result = this.Rotate(session, secondHash, fourthHash);

        result.Value.Should().BeOfType<SessionRotation.Invalid>();
        session.RefreshTokens.Should().HaveCount(2);
    }

    [Test]
    public void be_invalid_and_change_nothing_when_rotating_on_a_revoked_session()
    {
        string firstHash = StringMother.Random("hash");
        string secondHash = StringMother.Random("hash");
        Session session = this.NewSession(GuidMother.Random(), firstHash);
        session.Revoke(this.Clock.Object);

        Result<SessionRotation> result = this.Rotate(session, firstHash, secondHash);

        result.Value.Should().BeOfType<SessionRotation.Invalid>();
        session.RefreshTokens.Should().HaveCount(1);
        session.RefreshTokens.Single().ReplacedAt.Should().BeNull();
    }

    [Test]
    public void set_last_rotated_at_to_the_clock_time_when_rotating()
    {
        string firstHash = StringMother.Random("hash");
        string secondHash = StringMother.Random("hash");
        Session session = this.NewSession(GuidMother.Random(), firstHash);
        session.LastRotatedAt.Should().BeNull();
        this.AdvanceClock(TimeSpan.FromMinutes(5));

        this.Rotate(session, firstHash, secondHash);

        session.LastRotatedAt.Should().Be(Now + TimeSpan.FromMinutes(5));
    }

    [Test]
    public void update_last_rotated_at_each_time_when_rotating_successively()
    {
        string firstHash = StringMother.Random("hash");
        string secondHash = StringMother.Random("hash");
        string thirdHash = StringMother.Random("hash");
        Session session = this.NewSession(GuidMother.Random(), firstHash);
        this.AdvanceClock(TimeSpan.FromMinutes(5));
        this.Rotate(session, firstHash, secondHash);
        this.AdvanceClock(TimeSpan.FromMinutes(10));

        this.Rotate(session, secondHash, thirdHash);

        session.LastRotatedAt.Should().Be(Now + TimeSpan.FromMinutes(10));
    }

    [Test]
    public void leave_last_rotated_at_unchanged_when_the_rotation_is_invalid()
    {
        string firstHash = StringMother.Random("hash");
        string secondHash = StringMother.Random("hash");
        string thirdHash = StringMother.Random("hash");
        string unknownHash = StringMother.Random("hash");
        Session session = this.NewSession(GuidMother.Random(), firstHash);

        this.Rotate(session, unknownHash, secondHash);
        session.LastRotatedAt.Should().BeNull();

        this.AdvanceClock(TimeSpan.FromMinutes(5));
        this.Rotate(session, firstHash, secondHash);
        DateTimeOffset? rotatedAt = session.LastRotatedAt;
        this.AdvanceClock(Session.RefreshGrace / 2 + TimeSpan.FromMinutes(5));
        this.Rotate(session, firstHash, thirdHash);

        session.LastRotatedAt.Should().Be(rotatedAt);
    }

    [Test]
    public void leave_last_rotated_at_unchanged_when_the_rotation_is_a_reuse()
    {
        string firstHash = StringMother.Random("hash");
        string secondHash = StringMother.Random("hash");
        string thirdHash = StringMother.Random("hash");
        Session session = this.NewSession(GuidMother.Random(), firstHash);
        this.Rotate(session, firstHash, secondHash);
        DateTimeOffset? rotatedAt = session.LastRotatedAt;
        this.AdvanceClock(Session.RefreshGrace + TimeSpan.FromSeconds(50));

        Result<SessionRotation> result = this.Rotate(session, firstHash, thirdHash);

        result.Value.Should().BeOfType<SessionRotation.Reused>();
        session.LastRotatedAt.Should().Be(rotatedAt);
    }

    [Test]
    public void prune_tokens_replaced_long_ago_but_keep_the_current_and_the_just_replaced_ones()
    {
        string firstHash = StringMother.Random("hash");
        string secondHash = StringMother.Random("hash");
        string thirdHash = StringMother.Random("hash");
        Session session = this.NewSession(GuidMother.Random(), firstHash);
        this.AdvanceClock(TimeSpan.FromHours(1));
        this.Rotate(session, firstHash, secondHash);
        this.AdvanceClock(TimeSpan.FromHours(26));

        Result<SessionRotation> result = this.Rotate(session, secondHash, thirdHash);

        result.Value.Should().BeOfType<SessionRotation.Rotated>();
        session.RefreshTokens.Select(token => token.Id).Should().BeEquivalentTo(
            new[] { TokenOf(session, secondHash).Id, TokenOf(session, thirdHash).Id });
    }

    [Test]
    public void keep_tokens_replaced_within_the_retention()
    {
        string firstHash = StringMother.Random("hash");
        string secondHash = StringMother.Random("hash");
        string thirdHash = StringMother.Random("hash");
        Session session = this.NewSession(GuidMother.Random(), firstHash);
        this.AdvanceClock(TimeSpan.FromHours(1));
        this.Rotate(session, firstHash, secondHash);
        this.AdvanceClock(TimeSpan.FromHours(1) + Session.ReplacedTokenRetention - TimeSpan.FromHours(1));

        this.Rotate(session, secondHash, thirdHash);

        session.RefreshTokens.Should().HaveCount(3);
    }

    [Test]
    public void prune_expired_tokens_when_rotating()
    {
        string firstHash = StringMother.Random("hash");
        string secondHash = StringMother.Random("hash");
        string thirdHash = StringMother.Random("hash");
        Session session = this.NewSession(GuidMother.Random(), firstHash);
        this.AdvanceClock(RefreshToken.Lifetime - TimeSpan.FromHours(1));
        this.Rotate(session, firstHash, secondHash);
        this.AdvanceClock(RefreshToken.Lifetime + TimeSpan.FromHours(1));

        Result<SessionRotation> result = this.Rotate(session, secondHash, thirdHash);

        result.Value.Should().BeOfType<SessionRotation.Rotated>();
        session.RefreshTokens.Select(token => token.Id).Should().BeEquivalentTo(
            new[] { TokenOf(session, secondHash).Id, TokenOf(session, thirdHash).Id });
    }

    [Test]
    public void be_invalid_instead_of_reused_when_replaying_an_old_token_after_pruning()
    {
        string firstHash = StringMother.Random("hash");
        string secondHash = StringMother.Random("hash");
        string thirdHash = StringMother.Random("hash");
        string fourthHash = StringMother.Random("hash");
        Session session = this.NewSession(GuidMother.Random(), firstHash);
        this.AdvanceClock(TimeSpan.FromHours(1));
        this.Rotate(session, firstHash, secondHash);
        this.AdvanceClock(TimeSpan.FromHours(26));
        this.Rotate(session, secondHash, thirdHash);

        Result<SessionRotation> result = this.Rotate(session, firstHash, fourthHash);

        result.Value.Should().BeOfType<SessionRotation.Invalid>();
        session.RevokedAt.Should().BeNull();
    }

    [Test]
    public void keep_the_first_date_when_revoked_more_than_once()
    {
        Session session = this.NewSession(GuidMother.Random(), StringMother.Random("hash"));
        session.Revoke(this.Clock.Object);
        this.AdvanceClock(TimeSpan.FromHours(1));

        session.Revoke(this.Clock.Object);

        session.RevokedAt.Should().Be(Now);
    }

    [Test]
    public void be_active_when_fresh_and_inactive_after_being_revoked()
    {
        Session session = this.NewSession(GuidMother.Random(), StringMother.Random("hash"));
        session.IsActive(this.Clock.Object).Should().BeTrue();

        session.Revoke(this.Clock.Object);

        session.IsActive(this.Clock.Object).Should().BeFalse();
    }

    [Test]
    public void match_only_the_hashes_of_its_tokens()
    {
        string firstHash = StringMother.Random("hash");
        string otherHash = StringMother.Random("hash");
        Session session = this.NewSession(GuidMother.Random(), firstHash);

        session.HasToken(firstHash).Should().BeTrue();
        session.HasToken(otherHash).Should().BeFalse();
    }

    [Test]
    public void consider_a_refresh_token_active_just_before_expiry_and_inactive_at_expiry()
    {
        RefreshToken token = this.NewSession(GuidMother.Random(), StringMother.Random("hash")).RefreshTokens.Single();

        this.AdvanceClock(RefreshToken.Lifetime - TimeSpan.FromSeconds(1));
        token.IsActive(this.Clock.Object).Should().BeTrue();

        this.AdvanceClock(RefreshToken.Lifetime);
        token.IsActive(this.Clock.Object).Should().BeFalse();
    }

    [Test]
    public void consider_a_refresh_token_inactive_after_being_replaced()
    {
        string firstHash = StringMother.Random("hash");
        string secondHash = StringMother.Random("hash");
        Session session = this.NewSession(GuidMother.Random(), firstHash);
        this.Rotate(session, firstHash, secondHash);

        TokenOf(session, firstHash).IsActive(this.Clock.Object).Should().BeFalse();
    }

    [Test]
    public void not_report_a_never_replaced_refresh_token_as_replaced_within_the_grace()
    {
        RefreshToken token = this.NewSession(GuidMother.Random(), StringMother.Random("hash")).RefreshTokens.Single();

        token.WasReplacedWithin(this.Clock.Object, Session.RefreshGrace).Should().BeFalse();
    }

    [Test]
    public void report_a_replaced_refresh_token_as_replaced_within_the_grace_only_while_the_window_lasts()
    {
        string firstHash = StringMother.Random("hash");
        string secondHash = StringMother.Random("hash");
        TimeSpan pastGrace = Session.RefreshGrace + TimeSpan.FromSeconds(50);
        Session session = this.NewSession(GuidMother.Random(), firstHash);
        this.Rotate(session, firstHash, secondHash);
        RefreshToken old = TokenOf(session, firstHash);

        this.AdvanceClock(TimeSpan.FromSeconds(3));
        old.WasReplacedWithin(this.Clock.Object, Session.RefreshGrace).Should().BeTrue();

        this.AdvanceClock(pastGrace);
        old.WasReplacedWithin(this.Clock.Object, Session.RefreshGrace).Should().BeFalse();
    }

    [Test]
    public void filter_by_id_user_and_token_hash_in_the_specification_where_null_means_no_filter()
    {
        Guid userId = GuidMother.Random();
        string firstHash = StringMother.Random("hash");
        string secondHash = StringMother.Random("hash");
        string otherHash = StringMother.Random("hash");
        Session session = this.NewSession(userId, firstHash);
        Session other = SessionMother.With(tokenHash: otherHash);
        this.Rotate(session, firstHash, secondHash);
        Session[] sessions = [session, other];

        sessions.Count(candidate => Matches(SessionSpecificationMother.Unfiltered(), candidate)).Should().Be(2);
        sessions.Single(candidate => Matches(SessionSpecificationMother.WithId(session.Id), candidate))
            .Should().BeSameAs(session);
        sessions.Single(candidate => Matches(SessionSpecificationMother.WithUserId(userId), candidate))
            .Should().BeSameAs(session);
        sessions.Single(candidate => Matches(SessionSpecificationMother.WithTokenHash(secondHash), candidate))
            .Should().BeSameAs(session);
        sessions.Single(candidate => Matches(SessionSpecificationMother.WithTokenHash(otherHash), candidate))
            .Should().BeSameAs(other);
        SessionSpecification conflicting =
            SessionSpecificationMother.With(id: session.Id, tokenHash: otherHash);
        sessions.Count(candidate => Matches(conflicting, candidate)).Should().Be(0);
    }

    private static RefreshToken TokenOf(Session session, string hash)
        => session.RefreshTokens.Single(token => token.TokenHash == hash);

    private static bool Matches(SessionSpecification specification, Session candidate)
        => specification.Criteria.Compile()(candidate);

    private Session NewSession(Guid userId, string firstHash)
        => Session.Create(userId, firstHash, this.Clock.Object).Value;

    private Result<SessionRotation> Rotate(Session session, string hash, string newHash)
        => session.Rotate(hash, newHash, this.Clock.Object);
}
