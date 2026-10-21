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

    public School(
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

    public static School Create(
        Guid id,
        string name,
        string officialCode,
        Guid localityId,
        Guid? postalCodeId = null,
        string? address = null,
        string? phone = null)
    {
        Validate(id, name, officialCode, localityId, postalCodeId);

        return new School(
            id,
            name.Trim(),
            TextNormalizer.Normalize(name),
            officialCode.Trim(),
            localityId,
            postalCodeId,
            address,
            string.IsNullOrWhiteSpace(phone) ? null : phone.Trim());
    }

    private static void Validate(Guid id, string name, string officialCode, Guid localityId, Guid? postalCodeId)
    {
        if (id == Guid.Empty)
        {
            throw new SchoolExceptions.IdRequired();
        }
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new SchoolExceptions.NameRequired();
        }
        if (string.IsNullOrWhiteSpace(officialCode))
        {
            throw new SchoolExceptions.OfficialCodeRequired();
        }
        if (localityId == Guid.Empty)
        {
            throw new SchoolExceptions.LocalityRequired();
        }
        if (postalCodeId == Guid.Empty)
        {
            throw new SchoolExceptions.PostalCodeInvalid();
        }
    }
}
