namespace SumiClas.Tests.LostItems.Application;

using SumiClas.LostItems.Application;
using SumiClas.Tests.Shared;

public static class LostItemCreateCommandMother
{
    public static LostItemCreate.Command With(
        Guid id,
        string scope,
        Guid? classId,
        string status,
        string description,
        string? photoUrl,
        bool photoPrivacyConfirmed)
        => new(id, scope, classId, status, description, photoUrl, photoPrivacyConfirmed);

    public static LostItemCreate.Command Valid(
        Guid? id = null,
        string? scope = null,
        Guid? classId = null,
        string? status = null,
        string? description = null)
        => With(
            id ?? GuidMother.Random(),
            scope ?? "School",
            classId ?? GuidMother.Random(),
            status ?? "Lost",
            description ?? StringMother.Random("Item"),
            null,
            false);

    public static LostItemCreate.Command WithPhoto(string photoUrl, bool photoPrivacyConfirmed)
        => With(
            GuidMother.Random(),
            "School",
            null,
            "Lost",
            StringMother.Random("Item"),
            photoUrl,
            photoPrivacyConfirmed);
}
