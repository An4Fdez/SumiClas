namespace SumiClas.Tests.Shared.Infrastructure.Persistence;

using AwesomeAssertions;
using SumiClas.Announcements.Domain;
using SumiClas.Announcements.Infrastructure;
using SumiClas.Shared.Infrastructure.Persistence;
using SumiClas.Tests.Announcements.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Users.Domain;
using SumiClas.Users.Domain;
using SumiClas.Users.Infrastructure;

/// <summary>
/// The specifications translated to SQL by a real PostgreSQL: the filters that decide who sees what must select exactly
/// the rows they describe, including the ones that go through a child collection.
/// </summary>
[TestFixture]
public class SpecificationsShould
{
    [Test]
    public async Task select_only_the_announcements_of_a_class_when_filtering_by_scope_and_class()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        Guid classId = GuidMother.Random();
        Announcement ofTheClass = AnnouncementMother.ForClass(classId, "Of the class", UnitTestCase.Now);
        await SaveAsync(database, ofTheClass);
        await SaveAsync(database, AnnouncementMother.ForClass(GuidMother.Random(), "Other class", UnitTestCase.Now));
        await SaveAsync(database, AnnouncementMother.SchoolWide(GuidMother.Random(), "School", UnitTestCase.Now));

        IReadOnlyCollection<Announcement> found = await SearchAsync(
            database,
            new AnnouncementSpecification(scope: AnnouncementScope.Class, classId: classId));

        found.Select(announcement => announcement.Id).Should().Equal(ofTheClass.Id);
    }

    [Test]
    public async Task select_only_the_announcements_of_a_school_when_filtering_by_scope_and_school()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        Guid schoolId = GuidMother.Random();
        Announcement ofTheSchool = AnnouncementMother.SchoolWide(schoolId, "Of the school", UnitTestCase.Now);
        await SaveAsync(database, ofTheSchool);
        await SaveAsync(database, AnnouncementMother.SchoolWide(GuidMother.Random(), "Other school", UnitTestCase.Now));
        await SaveAsync(database, AnnouncementMother.ForClass(GuidMother.Random(), "A class", UnitTestCase.Now));

        IReadOnlyCollection<Announcement> found = await SearchAsync(
            database,
            new AnnouncementSpecification(scope: AnnouncementScope.School, schoolId: schoolId));

        found.Select(announcement => announcement.Id).Should().Equal(ofTheSchool.Id);
    }

    [Test]
    public async Task select_only_the_announcements_read_by_a_user_through_the_receipts()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        Guid schoolId = GuidMother.Random();
        Guid reader = GuidMother.Random();
        Announcement read = AnnouncementMother.ReadBy(schoolId, reader, UnitTestCase.Now);
        await SaveAsync(database, read);
        await SaveAsync(database, AnnouncementMother.ReadBy(schoolId, GuidMother.Random(), UnitTestCase.Now));
        await SaveAsync(database, AnnouncementMother.SchoolWide(schoolId, "Unread", UnitTestCase.Now));

        IReadOnlyCollection<Announcement> found = await SearchAsync(
            database,
            new AnnouncementSpecification(readByUserId: reader));

        found.Select(announcement => announcement.Id).Should().Equal(read.Id);
    }

    [Test]
    public async Task select_the_users_that_hold_a_role_in_a_class_through_the_child_roles()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        Guid classId = GuidMother.Random();
        User representative = UserMother.Representative(classId);
        User participant = UserMother.Participant(classId);
        await SaveAsync(database, representative, participant, UserMother.Participant(GuidMother.Random()));

        IReadOnlyCollection<User> found = await SearchAsync(database, new UserSpecification(classId: classId));

        found.Select(user => user.Id).Should().BeEquivalentTo([representative.Id, participant.Id]);
    }

    [Test]
    public async Task select_only_the_representatives_of_a_class()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        Guid classId = GuidMother.Random();
        User representative = UserMother.Representative(classId);
        await SaveAsync(
            database,
            representative,
            UserMother.Participant(classId),
            UserMother.Representative(GuidMother.Random()));

        IReadOnlyCollection<User> found = await SearchAsync(
            database,
            new UserSpecification(classId: classId, classRole: ClassRoleType.ClassRepresentative));

        found.Select(user => user.Id).Should().Equal(representative.Id);
    }

    [Test]
    public async Task not_mix_the_class_and_the_role_of_two_different_assignments_of_the_same_user()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        Guid representedClass = GuidMother.Random();
        Guid attendedClass = GuidMother.Random();
        User both = UserMother.With(classRoles:
        [
            ClassRoleAssignmentMother.Representative(representedClass),
            ClassRoleAssignmentMother.Participant(attendedClass),
        ]);
        await SaveAsync(database, both);

        IReadOnlyCollection<User> found = await SearchAsync(
            database,
            new UserSpecification(classId: attendedClass, classRole: ClassRoleType.ClassRepresentative));

        found.Should().BeEmpty("the user represents one class and only attends the other.");
    }

    [Test]
    public async Task select_only_the_users_whose_ids_are_in_the_list()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        User first = UserMother.Random();
        User second = UserMother.Random();
        await SaveAsync(database, first, second, UserMother.Random());

        IReadOnlyCollection<User> found = await SearchAsync(
            database,
            new UserSpecification(ids: [first.Id, second.Id]));

        found.Select(user => user.Id).Should().BeEquivalentTo([first.Id, second.Id]);
    }

    private static async Task SaveAsync(TestDatabase database, params User[] users)
    {
        await using SumiClasDbContext context = database.NewContext();
        context.Users.AddRange(users);
        await context.SaveChangesAsync();
    }

    private static async Task SaveAsync(TestDatabase database, Announcement announcement)
    {
        await using SumiClasDbContext context = database.NewContext();
        context.Announcements.Add(announcement);
        await context.SaveChangesAsync();
    }

    private static async Task<IReadOnlyCollection<Announcement>> SearchAsync(
        TestDatabase database,
        AnnouncementSpecification specification)
    {
        await using SumiClasDbContext context = database.NewContext();
        return await new AnnouncementRepository(context).SearchAsync(specification);
    }

    private static async Task<IReadOnlyCollection<User>> SearchAsync(
        TestDatabase database,
        UserSpecification specification)
    {
        await using SumiClasDbContext context = database.NewContext();
        return await new UserRepository(context).SearchAsync(specification);
    }
}
