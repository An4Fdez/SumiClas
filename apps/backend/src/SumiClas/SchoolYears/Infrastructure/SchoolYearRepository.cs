using Microsoft.EntityFrameworkCore;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Infrastructure.Persistence;

namespace SumiClas.SchoolYears.Infrastructure;

public sealed class SchoolYearRepository(SumiClasDbContext dbContext)
    : BaseRepository<SchoolYear>(dbContext), ISchoolYearRepository
{
    public Task<SchoolYear?> GetActiveBySchoolIdAsync(Guid schoolId, CancellationToken ct = default)
        => Set.FirstOrDefaultAsync(
            schoolYear => schoolYear.SchoolId == schoolId && schoolYear.Status == SchoolYearStatus.Active, ct);
}
