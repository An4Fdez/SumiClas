namespace SumiClas.SchoolYears.Application;

public sealed record SchoolYearResponse(Guid Id, Guid SchoolId, string Label, DateOnly StartDate, DateOnly EndDate, string Status);
