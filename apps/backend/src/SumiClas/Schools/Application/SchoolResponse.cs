namespace SumiClas.Schools.Application;

public sealed record SchoolResponse(Guid Id, string Name, string OfficialCode, string? Address);
