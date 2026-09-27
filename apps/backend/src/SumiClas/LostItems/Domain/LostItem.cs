using SumiClas.Shared.Domain;

namespace SumiClas.LostItems.Domain;

public sealed class LostItem : AggregateRoot
{
    public ItemScope Scope { get; private set; }

    /// <summary>Only set when Scope == Class.</summary>
    public Guid? ClassId { get; private set; }

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

    private LostItem(
        Guid id,
        ItemScope scope,
        Guid? classId,
        ItemStatus status,
        string description,
        string? photoUrl,
        bool photoPrivacyConfirmed,
        Guid authorId,
        DateTimeOffset createdAt) : base(id)
    {
        Scope = scope;
        ClassId = classId;
        Status = status;
        Recovered = false;
        Description = description;
        PhotoUrl = photoUrl;
        PhotoPrivacyConfirmed = photoPrivacyConfirmed;
        AuthorId = authorId;
        CreatedAt = createdAt;
    }

    public static Result<LostItem> Create(
        ItemScope scope,
        Guid? classId,
        ItemStatus status,
        string description,
        string? photoUrl,
        bool photoPrivacyConfirmed,
        Guid authorId,
        IClock clock)
    {
        var validation = Validate(scope, classId, description, photoUrl, photoPrivacyConfirmed, authorId);
        if (validation.IsFailure)
        {
            return Result<LostItem>.Failure(validation.Error!);
        }

        var lostItem = new LostItem(
            Guid.NewGuid(),
            scope,
            scope == ItemScope.Class ? classId : null,
            status,
            description.Trim(),
            string.IsNullOrWhiteSpace(photoUrl) ? null : photoUrl.Trim(),
            photoPrivacyConfirmed,
            authorId,
            clock.UtcNow);

        return Result<LostItem>.Success(lostItem);
    }

    private static Result Validate(
        ItemScope scope,
        Guid? classId,
        string description,
        string? photoUrl,
        bool photoPrivacyConfirmed,
        Guid authorId)
    {
        if (scope == ItemScope.Class && (classId is null || classId == Guid.Empty))
        {
            return Result.Failure(LostItemErrors.ClassRequiredForClassScope);
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
