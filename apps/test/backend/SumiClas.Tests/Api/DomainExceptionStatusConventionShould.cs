namespace SumiClas.Tests.Api;

using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using SumiClas.Api.Http;
using SumiClas.Classes.Domain;
using SumiClas.Enrollments.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Users.Domain;

[TestFixture]
public class DomainExceptionStatusConventionShould
{
    private static readonly string[] BadRequestVocabulary = ["Required", "Invalid"];

    [TestCase(typeof(ClassExceptions.NotFound), StatusCodes.Status404NotFound)]
    [TestCase(typeof(UserExceptions.ClassAccessForbidden), StatusCodes.Status403Forbidden)]
    [TestCase(typeof(UserExceptions.SchoolPublishForbidden), StatusCodes.Status403Forbidden)]
    [TestCase(typeof(SharedExceptions.AlreadyExists), StatusCodes.Status409Conflict)]
    [TestCase(typeof(EnrollmentExceptions.AlreadyConfirmed), StatusCodes.Status409Conflict)]
    [TestCase(typeof(UserExceptions.EmailAlreadyRegistered), StatusCodes.Status409Conflict)]
    [TestCase(typeof(SharedExceptions.ConcurrencyConflict), StatusCodes.Status409Conflict)]
    [TestCase(typeof(AuthenticationExceptions.Unauthenticated), StatusCodes.Status401Unauthorized)]
    [TestCase(typeof(AuthenticationExceptions.InvalidCredentials), StatusCodes.Status401Unauthorized)]
    [TestCase(typeof(AuthenticationExceptions.RefreshTokenInvalid), StatusCodes.Status401Unauthorized)]
    [TestCase(typeof(ClassExceptions.NameRequired), StatusCodes.Status400BadRequest)]
    [TestCase(typeof(EnrollmentExceptions.SameClass), StatusCodes.Status400BadRequest)]
    public void give_each_suffix_its_status(Type exception, int expected)
    {
        DomainExceptionConvention.StatusOf(exception).Should().Be(expected);
    }

    [Test]
    public void give_the_status_of_the_exception_that_was_thrown()
    {
        DomainExceptionConvention.StatusOf(new ClassExceptions.NotFound())
            .Should().Be(StatusCodes.Status404NotFound);
    }

    [Test]
    public void make_the_status_follow_the_end_of_the_name_and_not_a_part_of_it()
    {
        DomainExceptionConvention.StatusOf(typeof(NotFoundSomething)).Should().Be(StatusCodes.Status400BadRequest);
    }

    [Test]
    public void send_to_the_default_400_only_the_exceptions_named_for_it_or_approved_by_the_catalog()
    {
        IEnumerable<string> offenders = DefaultBadRequests()
            .Where(entry => !BadRequestVocabulary.Any(word => entry.Type.Name.Contains(word, StringComparison.Ordinal))
                && !BadRequestExceptionCatalog.Codes.Contains(entry.Code))
            .Select(entry => entry.Code);

        offenders.Should().BeEmpty(
            "an exception that falls in the default 400 must say Required or Invalid in its name, or be approved in "
            + "BadRequestExceptionCatalog; a NotFound, Forbidden or AlreadyExists that does not follow the "
            + "naming convention would otherwise become a 400 silently.");
    }

    [Test]
    public void keep_the_catalog_free_of_exceptions_that_no_longer_need_it()
    {
        List<string> inDefault = DefaultBadRequests()
            .Where(entry => !BadRequestVocabulary.Any(word => entry.Type.Name.Contains(word, StringComparison.Ordinal)))
            .Select(entry => entry.Code)
            .ToList();

        BadRequestExceptionCatalog.Codes.Where(code => !inDefault.Contains(code)).Should().BeEmpty();
    }

    private static IEnumerable<(Type Type, string Code)> DefaultBadRequests()
        => typeof(DomainException).Assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && typeof(DomainException).IsAssignableFrom(type))
            .Where(type => DomainExceptionConvention.StatusOf(type) == StatusCodes.Status400BadRequest)
            .Select(type => (type, ((DomainException)Activator.CreateInstance(type)!).Code));

    private sealed class NotFoundSomething() : DomainException("Sample.NotFoundSomething", "Sample.");
}
