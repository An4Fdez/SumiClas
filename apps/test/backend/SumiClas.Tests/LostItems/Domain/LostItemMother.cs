namespace SumiClas.Tests.LostItems.Domain;

using SumiClas.LostItems.Domain;
using SumiClas.Tests.Shared;

public static class LostItemMother
{
    public static LostItem With(
        Guid? id = null,
        ItemScope scope = ItemScope.School,
        Guid? classId = null,
        ItemStatus status = ItemStatus.Lost,
        bool recovered = false,
        string? description = null,
        string? photoUrl = null,
        Guid? authorId = null,
        DateTimeOffset? createdAt = null)
        => new(
            id ?? GuidMother.Random(),
            scope,
            classId,
            status,
            recovered,
            description ?? StringMother.Random("Item"),
            photoUrl,
            photoUrl is not null,
            authorId ?? GuidMother.Random(),
            createdAt ?? UnitTestCase.Now);

    public static LostItem ForClass(
        Guid classId,
        Guid? id = null,
        bool recovered = false,
        DateTimeOffset? createdAt = null)
        => With(id: id, scope: ItemScope.Class, classId: classId, recovered: recovered, createdAt: createdAt);
}
