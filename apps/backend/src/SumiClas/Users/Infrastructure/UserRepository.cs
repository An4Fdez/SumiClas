using Microsoft.EntityFrameworkCore;
using SumiClas.Shared.Infrastructure.Persistence;
using SumiClas.Users.Domain;

namespace SumiClas.Users.Infrastructure;

public sealed class UserRepository(SumiClasDbContext dbContext)
    : BaseRepository<User>(dbContext), IUserRepository
{
    public Task<User?> GetByEmailAsync(Email email, CancellationToken ct = default)
        => Set.FirstOrDefaultAsync(user => user.Email.Value == email.Value, ct);

    public Task<int> CountClassRepresentativesAsync(Guid classId, CancellationToken ct = default)
        => Set.CountAsync(
            user => user.ClassRoles.Any(role => role.ClassId == classId && role.Role == ClassRoleType.ClassRepresentative),
            ct);
}
