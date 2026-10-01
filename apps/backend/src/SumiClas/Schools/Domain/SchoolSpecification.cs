using SumiClas.Shared.Domain;

namespace SumiClas.Schools.Domain;

public sealed class SchoolSpecification : Specification<School>
{
    public SchoolSpecification(Guid? id = null, string? name = null, string? officialCode = null, string? address = null)
    {
        if (id is { } schoolId) AddCriteria(school => school.Id == schoolId);
        if (name is not null) AddCriteria(school => school.Name == name);
        if (officialCode is not null) AddCriteria(school => school.OfficialCode == officialCode);
        if (address is not null) AddCriteria(school => school.Address == address);
    }
}
