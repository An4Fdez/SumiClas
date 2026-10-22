using SumiClas.Shared.Domain;

namespace SumiClas.SchoolYears.Domain;

public sealed class SchoolYearSpecification : Specification<SchoolYear>
{
    public SchoolYearSpecification(
        Guid? id = null,
        Guid? schoolId = null,
        string? label = null,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        SchoolYearStatus? status = null,
        IReadOnlyCollection<Guid>? ids = null)
    {
        if (id is { } schoolYearId) AddCriteria(schoolYear => schoolYear.Id == schoolYearId);
        if (schoolId is { } owner) AddCriteria(schoolYear => schoolYear.SchoolId == owner);
        if (label is not null) AddCriteria(schoolYear => schoolYear.Label == label);
        if (startDate is { } start) AddCriteria(schoolYear => schoolYear.StartDate == start);
        if (endDate is { } end) AddCriteria(schoolYear => schoolYear.EndDate == end);
        if (status is { } state) AddCriteria(schoolYear => schoolYear.Status == state);
        if (ids is not null)
        {
            Guid[] schoolYearIds = ids.ToArray();
            AddCriteria(schoolYear => schoolYearIds.Contains(schoolYear.Id));
        }
    }
}
