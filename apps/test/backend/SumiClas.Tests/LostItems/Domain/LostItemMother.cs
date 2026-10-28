namespace SumiClas.Tests.LostItems.Domain;

using SumiClas.LostItems.Domain;
using SumiClas.Tests.Shared;

public static class LostItemMother
{
    public static LostItem With(
        Guid? id = null,
        ItemScope scope = ItemScope.School,
        Guid? classId = null,
        Guid? schoolId = null,
        ItemStatus status = ItemStatus.Lost,
        bool recovered = false,
        string? description = null,
        string? photoUrl = null,
        Guid? authorId = null,
        DateTimeOffset? createdAt = null,
        IEnumerable<LostItemClaim>? claims = null)
        => new(
            id ?? GuidMother.Random(),
            scope,
            classId,
            schoolId ?? (scope == ItemScope.School ? GuidMother.Random() : null),
            status,
            recovered,
            description ?? StringMother.Random("Item"),
            photoUrl,
            photoUrl is not null,
            authorId ?? GuidMother.Random(),
            createdAt ?? UnitTestCase.Now,
            null,
            claims: claims);

    public static LostItem ForClass(
        Guid classId,
        Guid? id = null,
        bool recovered = false,
        DateTimeOffset? createdAt = null,
        Guid? authorId = null,
        IEnumerable<LostItemClaim>? claims = null)
        => With(
            id: id,
            scope: ItemScope.Class,
            classId: classId,
            recovered: recovered,
            authorId: authorId,
            createdAt: createdAt,
            claims: claims);
}
