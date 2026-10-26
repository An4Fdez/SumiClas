namespace SumiClas.Tests.Api;

/// <summary>
/// The domain exceptions that fall in the default 400 of <c>DomainExceptionConvention</c> without carrying
/// <c>Required</c> or <c>Invalid</c> in their name. Adding one here is the review that a new rule violation is
/// meant to be a bad request and not a 404, 403 or 409 whose name does not follow the convention.
/// </summary>
public static class BadRequestExceptionCatalog
{
    public static readonly IReadOnlyCollection<string> Codes =
    [
        "Assignment.SubjectNotInClass",
        "Authentication.PasswordTooWeak",
        "CalendarEvent.UseCreateBirthdayForBirthdays",
        "ClassExpense.HasPayments",
        "ClassExpense.NotAParticipant",
        "ClassExpense.ParticipantNotInClass",
        "ClassExpense.TotalAmountMustBePositive",
        "Enrollment.DifferentSchool",
        "Enrollment.SameClass",
        "Enrollment.SameSchoolYear",
        "SchoolYear.Archived",
        "Session.RefreshTokenReused",
        "Student.GuardianAlreadyLinked",
        "Student.GuardianNotLinked",
        "User.LastClassRepresentativeCannotResign",
        "User.MaxClassRepresentativesReached",
        "User.NoRoleAssignedInClass",
        "User.NotClassRepresentative",
    ];
}
