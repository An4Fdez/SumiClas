using SumiClas.Shared.Domain;

namespace SumiClas.Schools.Domain;

public sealed class School : AggregateRoot
{
    public string Name { get; private set; }
    public string OfficialCode { get; private set; }
    public string? Address { get; private set; }

    private School(Guid id, string name, string officialCode, string? address) : base(id)
    {
        Name = name;
        OfficialCode = officialCode;
        Address = address;
    }

    public static Result<School> Register(string name, string officialCode, string? address = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<School>.Failure(SchoolErrors.NameRequired);
        }
        if (string.IsNullOrWhiteSpace(officialCode))
        {
            return Result<School>.Failure(SchoolErrors.OfficialCodeRequired);
        }

        var school = new School(Guid.NewGuid(), name.Trim(), officialCode.Trim(), address);
        return Result<School>.Success(school);
    }
}
