namespace SumiClas.Tests.SchoolYears.Domain;

using SumiClas.SchoolYears.Domain;
using SumiClas.Tests.Shared;

public static class SchoolYearMother
{
    private static readonly DateOnly Start = new(2026, 9, 1);
    private static readonly DateOnly End = new(2027, 6, 30);

    public static SchoolYear Active(Guid id)
        => With(id, GuidMother.Random(), StringMother.Random("year"), Start, End, SchoolYearStatus.Active);

    public static SchoolYear Archived(Guid id)
        => With(id, GuidMother.Random(), StringMother.Random("year"), Start, End, SchoolYearStatus.Archived);

    public static SchoolYear Starting(Guid schoolId, DateOnly startDate, SchoolYearStatus status)
        => With(GuidMother.Random(), schoolId, StringMother.Random("year"), startDate, End, status);

    public static SchoolYear Detailed(
        Guid id,
        Guid schoolId,
        string label,
        DateOnly startDate,
        DateOnly endDate,
        SchoolYearStatus status)
        => With(id, schoolId, label, startDate, endDate, status);

    private static SchoolYear With(
        Guid id,
        Guid schoolId,
        string label,
        DateOnly startDate,
        DateOnly endDate,
        SchoolYearStatus status)
        => new(id, schoolId, label, startDate, endDate, status);
}
