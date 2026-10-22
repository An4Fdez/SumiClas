namespace SumiClas.Tests.Api;

using AwesomeAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SumiClas.Api.Http;
using SumiClas.Classes.Domain;
using SumiClas.Schools.Domain;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Students.Domain;
using SumiClas.Users.Domain;

[TestFixture]
public class DomainExceptionProfilesShould
{
    [Test]
    public async Task declare_the_conflicts_of_saving_a_command_as_409()
    {
        await using WebApplication app = WebApplication.CreateSlimBuilder().Build();
        app.MapGet("/sample", () => "sample").ConflictsOnSave();

        DeclaredBy(app).Should().BeEquivalentTo(
        [
            new DomainExceptionStatus(typeof(SharedExceptions.AlreadyExists), StatusCodes.Status409Conflict),
            new DomainExceptionStatus(typeof(SharedExceptions.ConcurrencyConflict), StatusCodes.Status409Conflict),
        ]);
    }

    [Test]
    public async Task declare_a_missing_class_as_404()
    {
        await using WebApplication app = WebApplication.CreateSlimBuilder().Build();
        app.MapGet("/sample", () => "sample").RequiresClass();

        DeclaredBy(app).Should().BeEquivalentTo(
        [
            new DomainExceptionStatus(typeof(ClassExceptions.NotFound), StatusCodes.Status404NotFound),
        ]);
    }

    [Test]
    public async Task declare_a_missing_school_as_404()
    {
        await using WebApplication app = WebApplication.CreateSlimBuilder().Build();
        app.MapGet("/sample", () => "sample").RequiresSchool();

        DeclaredBy(app).Should().BeEquivalentTo(
        [
            new DomainExceptionStatus(typeof(SchoolExceptions.NotFound), StatusCodes.Status404NotFound),
        ]);
    }

    [Test]
    public async Task declare_a_missing_school_year_as_404_and_an_archived_one_as_400()
    {
        await using WebApplication app = WebApplication.CreateSlimBuilder().Build();
        app.MapGet("/sample", () => "sample").RequiresActiveSchoolYear();

        DeclaredBy(app).Should().BeEquivalentTo(
        [
            new DomainExceptionStatus(typeof(SchoolYearExceptions.NotFound), StatusCodes.Status404NotFound),
            new DomainExceptionStatus(typeof(SchoolYearExceptions.Archived), StatusCodes.Status400BadRequest),
        ]);
    }

    [Test]
    public async Task declare_an_unidentified_user_as_401()
    {
        await using WebApplication app = WebApplication.CreateSlimBuilder().Build();
        app.MapGet("/sample", () => "sample").RequiresSession();

        DeclaredBy(app).Should().BeEquivalentTo(
        [
            new DomainExceptionStatus(
                typeof(AuthenticationExceptions.Unauthenticated),
                StatusCodes.Status401Unauthorized),
        ]);
    }

    [Test]
    public async Task declare_a_missing_user_as_404()
    {
        await using WebApplication app = WebApplication.CreateSlimBuilder().Build();
        app.MapGet("/sample", () => "sample").RequiresUser();

        DeclaredBy(app).Should().BeEquivalentTo(
        [
            new DomainExceptionStatus(typeof(UserExceptions.NotFound), StatusCodes.Status404NotFound),
        ]);
    }

    [Test]
    public async Task declare_a_user_outside_the_class_as_403()
    {
        await using WebApplication app = WebApplication.CreateSlimBuilder().Build();
        app.MapGet("/sample", () => "sample").RequiresClassMembership();

        DeclaredBy(app).Should().BeEquivalentTo(
        [
            new DomainExceptionStatus(typeof(UserExceptions.ClassAccessForbidden), StatusCodes.Status403Forbidden),
        ]);
    }

    [Test]
    public async Task declare_a_user_who_is_not_a_delegate_as_403()
    {
        await using WebApplication app = WebApplication.CreateSlimBuilder().Build();
        app.MapGet("/sample", () => "sample").RequiresClassRepresentative();

        DeclaredBy(app).Should().BeEquivalentTo(
        [
            new DomainExceptionStatus(
                typeof(UserExceptions.ClassRepresentativeForbidden),
                StatusCodes.Status403Forbidden),
        ]);
    }

    [Test]
    public async Task declare_a_missing_student_as_404_and_a_user_who_is_not_their_guardian_as_403()
    {
        await using WebApplication app = WebApplication.CreateSlimBuilder().Build();
        app.MapGet("/sample", () => "sample").RequiresStudentGuardian();

        DeclaredBy(app).Should().BeEquivalentTo(
        [
            new DomainExceptionStatus(typeof(StudentExceptions.NotFound), StatusCodes.Status404NotFound),
            new DomainExceptionStatus(
                typeof(StudentExceptions.GuardianAccessForbidden),
                StatusCodes.Status403Forbidden),
        ]);
    }

    private static IReadOnlyList<DomainExceptionStatus> DeclaredBy(WebApplication app)
        => ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(source => source.Endpoints)
            .Single()
            .Metadata
            .GetOrderedMetadata<DomainExceptionStatus>();
}
