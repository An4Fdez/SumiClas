namespace SumiClas.Students.Application;

/// <summary>
/// A student of the current guardian. Privacy of minors: only the first name and the INITIAL of the first surname
/// are exposed (never the full surname or a birth year). The class, school year and school are the ones of the
/// student's confirmed enrollment in an active school year, and are null when there is none.
/// </summary>
public sealed record ChildResponse(
    Guid StudentId,
    string FirstName,
    string SurnameInitial,
    Guid? ClassId,
    string? ClassName,
    Guid? SchoolYearId,
    Guid? SchoolId,
    string? SchoolName);
