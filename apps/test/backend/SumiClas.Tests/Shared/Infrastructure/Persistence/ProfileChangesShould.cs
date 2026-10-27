namespace SumiClas.Tests.Shared.Infrastructure.Persistence;

using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SumiClas.Sessions.Application;
using SumiClas.Sessions.Domain;
using SumiClas.Sessions.Infrastructure;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Shared.Domain.Events;
using SumiClas.Shared.Infrastructure;
using SumiClas.Shared.Infrastructure.Persistence;
using SumiClas.Students.Domain;
using SumiClas.Tests.Sessions.Domain;
using SumiClas.Tests.Students.Domain;
using SumiClas.Tests.Users.Domain;
using SumiClas.Users.Domain;

/// <summary>
/// The profile edits over a real PostgreSQL: replacing the name of a stored student writes the new columns (and clears
/// the second surname), and a password change closes, through the domain event the dispatcher pulls, every session of
/// that user and no other.
/// </summary>
[TestFixture]
public class ProfileChangesShould : UnitTestCase
{
    private const string NEW_HASH = "new-hash";

    [Test]
    public async Task store_the_new_name_of_a_student_and_clear_the_second_surname_that_is_dropped()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        Student student = new(
            GuidMother.Random(),
            StudentNameMother.With("Lucia", "Perez", "Gil"),
            [GuardianLinkMother.Mother(GuidMother.Random())]);
        await using (SumiClasDbContext seed = database.NewContext())
        {
            seed.Students.Add(student);
            await seed.SaveChangesAsync();
        }

        await using (SumiClasDbContext change = database.NewContext())
        {
            Student stored = await change.Students.SingleAsync(candidate => candidate.Id == student.Id);
            stored.Rename(StudentNameMother.With("Marta", "Ruiz", null));
            await change.SaveChangesAsync();
        }

        await using SumiClasDbContext read = database.NewContext();
        Student reloaded = await read.Students.SingleAsync(candidate => candidate.Id == student.Id);
        reloaded.Name.Should().Be(StudentNameMother.With("Marta", "Ruiz", null));
    }

    [Test]
    public async Task revoke_every_session_of_the_user_who_changes_the_password_and_no_other()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        User user = UserMother.Random();
        Session first = SessionMother.With(user.Id);
        Session second = SessionMother.With(user.Id);
        Session ofAnotherUser = SessionMother.With();
        await using (SumiClasDbContext seed = database.NewContext())
        {
            seed.Users.Add(user);
            seed.Sessions.AddRange(first, second, ofAnotherUser);
            await seed.SaveChangesAsync();
        }

        await using (SumiClasDbContext change = database.NewContext())
        {
            User stored = await change.Users.SingleAsync(candidate => candidate.Id == user.Id);
            stored.ChangePassword(NEW_HASH, this.Clock.Object);
            await DispatcherOf(change, this.Clock.Object).DispatchAsync();
            await change.SaveChangesAsync();
        }

        await using SumiClasDbContext read = database.NewContext();
        (await read.Users.SingleAsync(candidate => candidate.Id == user.Id)).PasswordHash.Should().Be(NEW_HASH);
        (await RevokedAtAsync(read, first.Id)).Should().Be(Now);
        (await RevokedAtAsync(read, second.Id)).Should().Be(Now);
        (await RevokedAtAsync(read, ofAnotherUser.Id)).Should().BeNull();
    }

    private static DomainEventDispatcher DispatcherOf(SumiClasDbContext context, IClock clock)
    {
        ServiceCollection services = new();
        services.AddScoped<IDomainEventHandler<UserPasswordChangedDomainEvent>>(
            _ => new SessionRevokeAllOnUserPasswordChange(new SessionRepository(context), clock));
        return new DomainEventDispatcher(context, services.BuildServiceProvider());
    }

    private static async Task<DateTimeOffset?> RevokedAtAsync(SumiClasDbContext context, Guid sessionId)
        => (await context.Sessions.SingleAsync(session => session.Id == sessionId)).RevokedAt;
}
