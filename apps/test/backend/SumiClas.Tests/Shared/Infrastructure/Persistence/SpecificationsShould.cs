namespace SumiClas.Tests.Shared.Infrastructure.Persistence;

using AwesomeAssertions;
using SumiClas.Announcements.Domain;
using SumiClas.Announcements.Infrastructure;
using SumiClas.Assignments.Domain;
using SumiClas.Assignments.Infrastructure;
using SumiClas.ClassExpenses.Domain;
using SumiClas.ClassExpenses.Infrastructure;
using SumiClas.LostItems.Domain;
using SumiClas.LostItems.Infrastructure;
using SumiClas.Shared.Infrastructure.Persistence;
using SumiClas.Tests.Announcements.Domain;
using SumiClas.Tests.Assignments.Domain;
using SumiClas.Tests.ClassExpenses.Domain;
using SumiClas.Tests.LostItems.Domain;
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
    private const int ONE_SECOND = 1;
    private const int ONE_DAY = 1;

    private static readonly DateOnly Today = DateOnly.FromDateTime(UnitTestCase.Now.UtcDateTime);

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
            AnnouncementSpecificationMother.ForClass(classId));

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
            AnnouncementSpecificationMother.ForSchool(schoolId));

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
            AnnouncementSpecificationMother.ReadBy(reader));

        found.Select(announcement => announcement.Id).Should().Equal(read.Id);
    }

    [Test]
    public async Task select_only_the_announcements_with_no_read_receipt_of_a_user_when_filtering_by_unread()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        Guid schoolId = GuidMother.Random();
        Guid reader = GuidMother.Random();
        Announcement unread = AnnouncementMother.SchoolWide(schoolId, "Unread", UnitTestCase.Now);
        Announcement readByOthers = AnnouncementMother.ReadBy(schoolId, GuidMother.Random(), UnitTestCase.Now);
        await SaveAsync(database, unread);
        await SaveAsync(database, readByOthers);
        await SaveAsync(database, AnnouncementMother.ReadBy(schoolId, reader, UnitTestCase.Now));

        IReadOnlyCollection<Announcement> found = await SearchAsync(
            database,
            AnnouncementSpecificationMother.UnreadBy(reader));

        found.Select(announcement => announcement.Id).Should().BeEquivalentTo([unread.Id, readByOthers.Id]);
    }

    [Test]
    public async Task select_only_the_announcements_created_at_a_moment_or_later_when_filtering_by_created_from()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        Guid schoolId = GuidMother.Random();
        Announcement atTheEdge = AnnouncementMother.SchoolWide(schoolId, "At the edge", UnitTestCase.Now);
        Announcement after = AnnouncementMother.SchoolWide(schoolId, "After", UnitTestCase.Now.AddSeconds(ONE_SECOND));
        await SaveAsync(database, atTheEdge);
        await SaveAsync(database, after);
        Announcement before = AnnouncementMother.SchoolWide(
            schoolId,
            "Before",
            UnitTestCase.Now.AddSeconds(-ONE_SECOND));
        await SaveAsync(database, before);

        IReadOnlyCollection<Announcement> found = await SearchAsync(
            database,
            AnnouncementSpecificationMother.CreatedFrom(UnitTestCase.Now));

        found.Select(announcement => announcement.Id).Should().BeEquivalentTo([atTheEdge.Id, after.Id]);
    }

    [Test]
    public async Task select_only_the_announcements_created_strictly_before_a_moment_when_filtering_by_created_before()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        Guid schoolId = GuidMother.Random();
        Announcement justBefore = AnnouncementMother.SchoolWide(
            schoolId,
            "Just before",
            UnitTestCase.Now.AddSeconds(-ONE_SECOND));
        await SaveAsync(database, justBefore);
        await SaveAsync(database, AnnouncementMother.SchoolWide(schoolId, "At the edge", UnitTestCase.Now));
        await SaveAsync(
            database,
            AnnouncementMother.SchoolWide(schoolId, "After", UnitTestCase.Now.AddSeconds(ONE_SECOND)));

        IReadOnlyCollection<Announcement> found = await SearchAsync(
            database,
            AnnouncementSpecificationMother.CreatedBefore(UnitTestCase.Now));

        found.Select(announcement => announcement.Id).Should().Equal(justBefore.Id);
    }

    [Test]
    public async Task select_only_the_assignments_due_on_a_day_or_later_when_filtering_by_due_from()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        Assignment onTheDay = AssignmentMother.With(dueDate: Today);
        Assignment after = AssignmentMother.With(dueDate: Today.AddDays(ONE_DAY));
        await SaveAsync(database, onTheDay);
        await SaveAsync(database, after);
        await SaveAsync(database, AssignmentMother.With(dueDate: Today.AddDays(-ONE_DAY)));

        IReadOnlyCollection<Assignment> found = await SearchAsync(
            database,
            AssignmentSpecificationMother.DueFrom(Today));

        found.Select(assignment => assignment.Id).Should().BeEquivalentTo([onTheDay.Id, after.Id]);
    }

    [Test]
    public async Task select_only_the_assignments_due_on_a_day_or_earlier_when_filtering_by_due_to()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        Assignment onTheDay = AssignmentMother.With(dueDate: Today);
        Assignment before = AssignmentMother.With(dueDate: Today.AddDays(-ONE_DAY));
        await SaveAsync(database, onTheDay);
        await SaveAsync(database, before);
        await SaveAsync(database, AssignmentMother.With(dueDate: Today.AddDays(ONE_DAY)));

        IReadOnlyCollection<Assignment> found = await SearchAsync(
            database,
            AssignmentSpecificationMother.DueTo(Today));

        found.Select(assignment => assignment.Id).Should().BeEquivalentTo([onTheDay.Id, before.Id]);
    }

    [Test]
    public async Task select_only_the_assignments_a_user_has_not_marked_as_done_when_filtering_by_not_done()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        Guid user = GuidMother.Random();
        Assignment notDone = AssignmentMother.With();
        Assignment doneByOthers = AssignmentMother.DoneBy(GuidMother.Random(), GuidMother.Random());
        await SaveAsync(database, notDone);
        await SaveAsync(database, doneByOthers);
        await SaveAsync(database, AssignmentMother.DoneBy(GuidMother.Random(), user));

        IReadOnlyCollection<Assignment> found = await SearchAsync(
            database,
            AssignmentSpecificationMother.NotDoneBy(user));

        found.Select(assignment => assignment.Id).Should().BeEquivalentTo([notDone.Id, doneByOthers.Id]);
    }

    [Test]
    public async Task select_only_the_expenses_a_guardian_takes_part_in_without_having_paid()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        Guid guardian = GuidMother.Random();
        ClassExpense owed = ClassExpenseMother.With(participants:
        [
            ExpenseParticipantMother.Unpaid(guardian),
            ExpenseParticipantMother.Paid(GuidMother.Random()),
        ]);
        await SaveAsync(database, owed);
        await SaveAsync(database, ClassExpenseMother.With(participants: [ExpenseParticipantMother.Paid(guardian)]));
        await SaveAsync(database, ClassExpenseMother.With(participants:
        [
            ExpenseParticipantMother.Paid(guardian),
            ExpenseParticipantMother.Unpaid(GuidMother.Random()),
        ]));
        await SaveAsync(database, ClassExpenseMother.With());

        IReadOnlyCollection<ClassExpense> found = await SearchAsync(
            database,
            ClassExpenseSpecificationMother.OwedBy(guardian));

        found.Select(expense => expense.Id).Should().Equal(owed.Id);
    }

    [Test]
    public async Task select_only_the_lost_items_created_from_an_instant_on_when_filtering_by_created_from()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        LostItem atTheEdge = LostItemMother.With(createdAt: UnitTestCase.Now);
        LostItem after = LostItemMother.With(createdAt: UnitTestCase.Now.AddSeconds(ONE_SECOND));
        await SaveAsync(database, atTheEdge);
        await SaveAsync(database, after);
        await SaveAsync(database, LostItemMother.With(createdAt: UnitTestCase.Now.AddSeconds(-ONE_SECOND)));

        IReadOnlyCollection<LostItem> found = await SearchAsync(
            database,
            LostItemSpecificationMother.CreatedFrom(UnitTestCase.Now));

        found.Select(item => item.Id).Should().BeEquivalentTo([atTheEdge.Id, after.Id]);
    }

    [Test]
    public async Task select_the_users_that_hold_a_role_in_a_class_through_the_child_roles()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        Guid classId = GuidMother.Random();
        User representative = UserMother.Representative(classId);
        User participant = UserMother.Participant(classId);
        await SaveAsync(database, representative, participant, UserMother.Participant(GuidMother.Random()));

        IReadOnlyCollection<User> found = await SearchAsync(database, UserSpecificationMother.InClass(classId));

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
            UserSpecificationMother.RepresentativesOfClass(classId));

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
            UserSpecificationMother.RepresentativesOfClass(attendedClass));

        found.Should().BeEmpty("the user represents one class and only attends the other.");
    }

    [Test]
    public async Task select_only_the_representatives_of_any_of_the_classes_in_the_list()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        Guid firstClass = GuidMother.Random();
        Guid secondClass = GuidMother.Random();
        User ofFirst = UserMother.Representative(firstClass);
        User ofSecond = UserMother.Representative(secondClass);
        await SaveAsync(
            database,
            ofFirst,
            ofSecond,
            UserMother.Participant(firstClass),
            UserMother.Representative(GuidMother.Random()));

        IReadOnlyCollection<User> found = await SearchAsync(
            database,
            UserSpecificationMother.RepresentativesOfAny([firstClass, secondClass]));

        found.Select(user => user.Id).Should().BeEquivalentTo([ofFirst.Id, ofSecond.Id]);
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
            UserSpecificationMother.WithIds([first.Id, second.Id]));

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

    private static async Task SaveAsync(TestDatabase database, Assignment assignment)
    {
        await using SumiClasDbContext context = database.NewContext();
        context.Assignments.Add(assignment);
        await context.SaveChangesAsync();
    }

    private static async Task SaveAsync(TestDatabase database, ClassExpense expense)
    {
        await using SumiClasDbContext context = database.NewContext();
        context.ClassExpenses.Add(expense);
        await context.SaveChangesAsync();
    }

    private static async Task SaveAsync(TestDatabase database, LostItem lostItem)
    {
        await using SumiClasDbContext context = database.NewContext();
        context.LostItems.Add(lostItem);
        await context.SaveChangesAsync();
    }

    private static async Task<IReadOnlyCollection<LostItem>> SearchAsync(
        TestDatabase database,
        LostItemSpecification specification)
    {
        await using SumiClasDbContext context = database.NewContext();
        return await new LostItemRepository(context).SearchAsync(specification);
    }

    private static async Task<IReadOnlyCollection<ClassExpense>> SearchAsync(
        TestDatabase database,
        ClassExpenseSpecification specification)
    {
        await using SumiClasDbContext context = database.NewContext();
        return await new ClassExpenseRepository(context).SearchAsync(specification);
    }

    private static async Task<IReadOnlyCollection<Assignment>> SearchAsync(
        TestDatabase database,
        AssignmentSpecification specification)
    {
        await using SumiClasDbContext context = database.NewContext();
        return await new AssignmentRepository(context).SearchAsync(specification);
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
