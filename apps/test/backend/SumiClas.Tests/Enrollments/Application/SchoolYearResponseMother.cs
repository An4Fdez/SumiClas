namespace SumiClas.Tests.Enrollments.Application;

using SumiClas.SchoolYears.Application;
using SumiClas.Tests.Shared;

public static class SchoolYearResponseMother
{
    public const string ACTIVE = "Active";
    public const string ARCHIVED = "Archived";

    public static SchoolYearResponse Active(Guid id, Guid schoolId)
        => With(id, schoolId, new DateOnly(2026, 9, 1), new DateOnly(2027, 6, 30), ACTIVE);

    public static SchoolYearResponse Archived(Guid id, Guid schoolId)
        => With(id, schoolId, new DateOnly(2025, 9, 1), new DateOnly(2026, 6, 30), ARCHIVED);

    private static SchoolYearResponse With(
        Guid id,
        Guid schoolId,
        DateOnly startDate,
        DateOnly endDate,
        string status)
        => new(id, schoolId, StringMother.Random("year"), startDate, endDate, status);
}
