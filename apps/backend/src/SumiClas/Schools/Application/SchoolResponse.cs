namespace SumiClas.Schools.Application;

public sealed record SchoolResponse(
    Guid Id,
    string Name,
    string OfficialCode,
    Guid LocalityId,
    Guid? PostalCodeId,
    string? Address,
    string? Phone);
