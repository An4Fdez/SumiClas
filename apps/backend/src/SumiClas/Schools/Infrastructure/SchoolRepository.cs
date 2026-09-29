using Microsoft.EntityFrameworkCore;
using SumiClas.Schools.Domain;
using SumiClas.Shared.Infrastructure.Persistence;

namespace SumiClas.Schools.Infrastructure;

public sealed class SchoolRepository(SumiClasDbContext dbContext) : ISchoolRepository
{
    public Task<School?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => dbContext.Schools.FirstOrDefaultAsync(school => school.Id == id, ct);

    public async Task AddAsync(School school, CancellationToken ct = default)
        => await dbContext.Schools.AddAsync(school, ct);
}
