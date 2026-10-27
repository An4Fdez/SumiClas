namespace SumiClas.Students.Application;

/// <summary>
/// A student of the current guardian. Privacy of minors: the full surnames are exposed only here, to the student's
/// own guardians (to edit the name), never in a read model shared with other users, and never a birth year;
/// everywhere else only the first name and the INITIAL of the first surname go out. The class, school year and
/// school are the ones of the student's confirmed enrollment in an active school year, and are null when there is none.
/// </summary>
public sealed record ChildResponse(
    Guid StudentId,
    string FirstName,
    string SurnameInitial,
    string FirstSurname,
    string? SecondSurname,
    Guid? ClassId,
    string? ClassName,
    Guid? SchoolYearId,
    Guid? SchoolId,
    string? SchoolName);
