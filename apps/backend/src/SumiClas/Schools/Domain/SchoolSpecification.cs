using SumiClas.Shared.Domain;

namespace SumiClas.Schools.Domain;

public sealed class SchoolSpecification : Specification<School>
{
    public SchoolSpecification(
        Guid? id = null,
        string? name = null,
        string? officialCode = null,
        Guid? localityId = null,
        Guid? postalCodeId = null,
        string? address = null,
        IReadOnlyCollection<Guid>? ids = null)
    {
        if (id is { } schoolId) AddCriteria(school => school.Id == schoolId);
        if (name is not null) AddCriteria(school => school.Name == name);
        if (officialCode is not null) AddCriteria(school => school.OfficialCode == officialCode);
        if (localityId is { } locality) AddCriteria(school => school.LocalityId == locality);
        if (postalCodeId is { } postalCode) AddCriteria(school => school.PostalCodeId == postalCode);
        if (address is not null) AddCriteria(school => school.Address == address);
        if (ids is not null)
        {
            Guid[] schoolIds = ids.ToArray();
            AddCriteria(school => schoolIds.Contains(school.Id));
        }
    }
}
