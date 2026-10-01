using SumiClas.Shared.Domain;

namespace SumiClas.Classes.Domain;

public sealed class ClassSpecification : Specification<Class>
{
    public ClassSpecification(Guid? id = null, Guid? schoolYearId = null, string? name = null)
    {
        if (id is { } classId) AddCriteria(schoolClass => schoolClass.Id == classId);
        if (schoolYearId is { } schoolYear) AddCriteria(schoolClass => schoolClass.SchoolYearId == schoolYear);
        if (name is not null) AddCriteria(schoolClass => schoolClass.Name == name);
    }
}
