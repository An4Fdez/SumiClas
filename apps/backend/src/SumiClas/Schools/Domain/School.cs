using SumiClas.Shared.Domain;

namespace SumiClas.Schools.Domain;

public sealed class School : AggregateRoot
{
    public string Name { get; private set; }
    public string NormalizedName { get; private set; }
    public string OfficialCode { get; private set; }
    public Guid LocalityId { get; private set; }
    public Guid? PostalCodeId { get; private set; }
    public string? Address { get; private set; }
    public string? Phone { get; private set; }

    private School(
        Guid id,
        string name,
        string normalizedName,
        string officialCode,
        Guid localityId,
        Guid? postalCodeId,
        string? address,
        string? phone) : base(id)
    {
        Name = name;
        NormalizedName = normalizedName;
        OfficialCode = officialCode;
        LocalityId = localityId;
        PostalCodeId = postalCodeId;
        Address = address;
        Phone = phone;
    }

    public static Result<School> Create(
        Guid id,
        string name,
        string officialCode,
        Guid localityId,
        Guid? postalCodeId = null,
        string? address = null,
        string? phone = null)
    {
        var validation = Validate(id, name, officialCode, localityId, postalCodeId);
        if (validation.IsFailure)
        {
            return Result<School>.Failure(validation.Error!);
        }

        var school = new School(
            id,
            name.Trim(),
            TextNormalizer.Normalize(name),
            officialCode.Trim(),
            localityId,
            postalCodeId,
            address,
            string.IsNullOrWhiteSpace(phone) ? null : phone.Trim());
        return Result<School>.Success(school);
    }

    private static Result Validate(Guid id, string name, string officialCode, Guid localityId, Guid? postalCodeId)
    {
        if (id == Guid.Empty)
        {
            return Result.Failure(SchoolErrors.IdRequired);
        }
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(SchoolErrors.NameRequired);
        }
        if (string.IsNullOrWhiteSpace(officialCode))
        {
            return Result.Failure(SchoolErrors.OfficialCodeRequired);
        }
        if (localityId == Guid.Empty)
        {
            return Result.Failure(SchoolErrors.LocalityRequired);
        }
        if (postalCodeId == Guid.Empty)
        {
            return Result.Failure(SchoolErrors.PostalCodeInvalid);
        }
        return Result.Success();
    }
}
