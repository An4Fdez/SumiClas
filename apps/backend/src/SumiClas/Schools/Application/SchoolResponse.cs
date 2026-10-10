namespace SumiClas.Schools.Application;

public sealed record SchoolResponse(
    Guid Id,
    string Name,
    string OfficialCode,
    Guid ProvinceId,
    string ProvinceName,
    Guid LocalityId,
    string LocalityName,
    Guid? PostalCodeId,
    string? PostalCode,
    string? Address,
    string? Phone);
