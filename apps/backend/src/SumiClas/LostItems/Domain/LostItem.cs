using SumiClas.Shared.Domain;

namespace SumiClas.LostItems.Domain;

public sealed class LostItem : AggregateRoot
{
    public ItemScope Scope { get; private set; }

    /// <summary>Only set when Scope == Class.</summary>
    public Guid? ClassId { get; private set; }

    /// <summary>Only set when Scope == School: the school the lost item belongs to.</summary>
    public Guid? SchoolId { get; private set; }

    /// <summary>"Busco" vs "He encontrado" (Spanish UI copy) — never changes after creation.</summary>
    public ItemStatus Status { get; private set; }

    /// <summary>Resolution state, independent of Status.</summary>
    public bool Recovered { get; private set; }

    public string Description { get; private set; }

    /// <summary>Object URL local to the device; never persisted server-side as a real upload.</summary>
    public string? PhotoUrl { get; private set; }

    /// <summary>
    /// Required to be true whenever PhotoUrl is set — privacy rule (domain doc, section 7.3):
    /// photos of minors are opt-in per photo, never a general setting.
    /// </summary>
    public bool PhotoPrivacyConfirmed { get; private set; }

    public Guid AuthorId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public LostItem(
        Guid id,
        ItemScope scope,
        Guid? classId,
        Guid? schoolId,
        ItemStatus status,
        bool recovered,
        string description,
        string? photoUrl,
        bool photoPrivacyConfirmed,
        Guid authorId,
        DateTimeOffset createdAt) : base(id)
    {
        Scope = scope;
        ClassId = classId;
        SchoolId = schoolId;
        Status = status;
        Recovered = recovered;
        Description = description;
        PhotoUrl = photoUrl;
        PhotoPrivacyConfirmed = photoPrivacyConfirmed;
        AuthorId = authorId;
        CreatedAt = createdAt;
    }

    public static LostItem Create(
        Guid id,
        ItemScope scope,
        Guid? classId,
        Guid? schoolId,
        ItemStatus status,
        string description,
        string? photoUrl,
        bool photoPrivacyConfirmed,
        Guid authorId,
        IClock clock)
    {
        Validate(id, scope, classId, schoolId, description, photoUrl, photoPrivacyConfirmed, authorId);

        return new LostItem(
            id,
            scope,
            scope == ItemScope.Class ? classId : null,
            scope == ItemScope.School ? schoolId : null,
            status,
            false,
            description.Trim(),
            string.IsNullOrWhiteSpace(photoUrl) ? null : photoUrl.Trim(),
            photoPrivacyConfirmed,
            authorId,
            clock.UtcNow);
    }

    private static void Validate(
        Guid id,
        ItemScope scope,
        Guid? classId,
        Guid? schoolId,
        string description,
        string? photoUrl,
        bool photoPrivacyConfirmed,
        Guid authorId)
    {
        if (id == Guid.Empty)
        {
            throw new LostItemExceptions.IdRequired();
        }
        if (scope == ItemScope.Class && (classId is null || classId == Guid.Empty))
        {
            throw new LostItemExceptions.ClassRequiredForClassScope();
        }
        if (scope == ItemScope.School && (schoolId is null || schoolId == Guid.Empty))
        {
            throw new LostItemExceptions.SchoolRequiredForSchoolScope();
        }
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new LostItemExceptions.DescriptionRequired();
        }
        if (!string.IsNullOrWhiteSpace(photoUrl) && !photoPrivacyConfirmed)
        {
            throw new LostItemExceptions.PhotoPrivacyConfirmationRequired();
        }
        if (authorId == Guid.Empty)
        {
            throw new LostItemExceptions.AuthorRequired();
        }
    }

    public void MarkAsRecovered() => Recovered = true;

    public void MarkAsNotRecovered() => Recovered = false;
}
