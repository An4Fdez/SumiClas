using Microsoft.EntityFrameworkCore;
using SumiClas.Shared.Infrastructure.Persistence;
using SumiClas.Students.Domain;

namespace SumiClas.Students.Infrastructure;

public sealed class StudentRepository(SumiClasDbContext dbContext)
    : BaseRepository<Student>(dbContext), IStudentRepository
{
    public async Task<IReadOnlyCollection<Student>> GetByGuardianIdAsync(Guid guardianId, CancellationToken ct = default)
        => await Set.Where(student => student.Guardians.Any(guardian => guardian.GuardianId == guardianId)).ToListAsync(ct);
}
