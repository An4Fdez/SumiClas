namespace SumiClas.Tests.SchoolYears.Application;

using SumiClas.SchoolYears.Application;

public static class SchoolYearCreateCommandMother
{
    public static SchoolYearCreate.Command With(
        Guid id,
        Guid schoolId,
        string label,
        DateOnly startDate,
        DateOnly endDate)
        => new(id, schoolId, label, startDate, endDate);
}
