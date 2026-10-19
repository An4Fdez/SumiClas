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

    public static Result<LostItem> Create(
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
        var validation = Validate(id, scope, classId, schoolId, description, photoUrl, photoPrivacyConfirmed, authorId);
        if (validation.IsFailure)
        {
            return Result<LostItem>.Failure(validation.Error!);
        }

        var lostItem = new LostItem(
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

        return Result<LostItem>.Success(lostItem);
    }

    private static Result Validate(
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
            return Result.Failure(LostItemErrors.IdRequired);
        }
        if (scope == ItemScope.Class && (classId is null || classId == Guid.Empty))
        {
            return Result.Failure(LostItemErrors.ClassRequiredForClassScope);
        }
        if (scope == ItemScope.School && (schoolId is null || schoolId == Guid.Empty))
        {
            return Result.Failure(LostItemErrors.SchoolRequiredForSchoolScope);
        }
        if (string.IsNullOrWhiteSpace(description))
        {
            return Result.Failure(LostItemErrors.DescriptionRequired);
        }
        if (!string.IsNullOrWhiteSpace(photoUrl) && !photoPrivacyConfirmed)
        {
            return Result.Failure(LostItemErrors.PhotoPrivacyConfirmationRequired);
        }
        if (authorId == Guid.Empty)
        {
            return Result.Failure(LostItemErrors.AuthorRequired);
        }
        return Result.Success();
    }

    public void MarkAsRecovered() => Recovered = true;

    public void MarkAsNotRecovered() => Recovered = false;
}
