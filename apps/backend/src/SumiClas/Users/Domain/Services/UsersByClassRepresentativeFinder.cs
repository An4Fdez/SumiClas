namespace SumiClas.Users.Domain.Services;

public sealed class UsersByClassRepresentativeFinder(IUserRepository users)
{
    public Task<IReadOnlyCollection<User>> FindAsync(Guid userId, Guid classId, CancellationToken ct = default)
        => users.SearchAsync(
            new UserSpecification(id: userId, classId: classId, classRole: ClassRoleType.ClassRepresentative),
            ct);
}
