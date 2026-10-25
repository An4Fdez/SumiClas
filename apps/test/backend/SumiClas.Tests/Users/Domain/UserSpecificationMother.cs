namespace SumiClas.Tests.Users.Domain;

using SumiClas.Users.Domain;

public static class UserSpecificationMother
{
    public static UserSpecification InClass(Guid classId) => new(classId: classId);

    public static UserSpecification RepresentativesOfClass(Guid classId)
        => new(classId: classId, classRole: ClassRoleType.ClassRepresentative);

    public static UserSpecification RepresentativesOfAny(IReadOnlyCollection<Guid> classIds)
        => new(classIds: classIds, classRole: ClassRoleType.ClassRepresentative);

    public static UserSpecification WithIds(IReadOnlyCollection<Guid> ids) => new(ids: ids);
}
