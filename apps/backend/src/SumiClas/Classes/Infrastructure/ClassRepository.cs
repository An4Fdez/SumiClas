using Microsoft.EntityFrameworkCore;
using SumiClas.Classes.Domain;
using SumiClas.Shared.Infrastructure.Persistence;

namespace SumiClas.Classes.Infrastructure;

public sealed class ClassRepository(SumiClasDbContext dbContext)
    : BaseRepository<Class>(dbContext), IClassRepository
{
    public async Task<IReadOnlyCollection<Class>> GetBySchoolYearIdAsync(Guid schoolYearId, CancellationToken ct = default)
        => await Set.Where(schoolClass => schoolClass.SchoolYearId == schoolYearId).ToListAsync(ct);
}
