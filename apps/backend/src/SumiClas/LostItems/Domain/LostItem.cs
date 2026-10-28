using SumiClas.Shared.Domain;

namespace SumiClas.LostItems.Domain;

public sealed class LostItem : AggregateRoot
{
    private readonly List<LostItemClaim> _claims = new();
    public IReadOnlyCollection<LostItemClaim> Claims => _claims.AsReadOnly();

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

    /// <summary>When the author last modified it; null if it was never modified.</summary>
    public DateTimeOffset? UpdatedAt { get; private set; }

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
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt,
        IEnumerable<LostItemClaim>? claims = null) : base(id)
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
        UpdatedAt = updatedAt;
        _claims.AddRange(claims ?? []);
    }

    private LostItem()
    {
        // For reflection purposes (EF Core materialization): the claims parameter is an owned navigation
        Description = null!;
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
            clock.UtcNow,
            null);
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
        ValidateContent(description);
        if (!string.IsNullOrWhiteSpace(photoUrl) && !photoPrivacyConfirmed)
        {
            throw new LostItemExceptions.PhotoPrivacyConfirmationRequired();
        }
        if (authorId == Guid.Empty)
        {
            throw new LostItemExceptions.AuthorRequired();
        }
    }

    public static void ValidateContent(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new LostItemExceptions.DescriptionRequired();
        }
    }

    /// <summary>Changes the description. The status, the photo and whether it was recovered stay as they were.</summary>
    public void Update(string description, IClock clock)
    {
        ValidateContent(description);

        Description = description.Trim();
        UpdatedAt = clock.UtcNow;
    }

    public void MarkAsRecovered() => Recovered = true;

    public void MarkAsNotRecovered() => Recovered = false;

    /// <summary>
    /// Someone other than the author offers to resolve the item. A user can have only one pending claim at a time, but
    /// can claim again once the previous one was resolved.
    /// </summary>
    public void AddClaim(Guid claimId, Guid claimantId, string message, IClock clock)
    {
        LostItemClaim claim = LostItemClaim.Create(claimId, claimantId, message, clock);

        if (claimantId == AuthorId)
        {
            throw new LostItemExceptions.ClaimOwnItemForbidden();
        }
        if (Recovered)
        {
            throw new LostItemExceptions.AlreadyRecovered();
        }
        if (_claims.Any(existing => existing.ClaimantId == claimantId && existing.Status == ClaimStatus.Pending))
        {
            throw new LostItemExceptions.ClaimAlreadyExists();
        }

        _claims.Add(claim);
    }

    /// <summary>Accepting a claim resolves the item: it is marked as recovered and the other pending claims are rejected.</summary>
    public void AcceptClaim(Guid claimId, string? responseMessage)
    {
        LostItemClaim accepted = FindClaim(claimId);

        accepted.Accept(responseMessage);

        foreach (LostItemClaim other in _claims.Where(claim => claim.Status == ClaimStatus.Pending))
        {
            other.Reject();
        }

        MarkAsRecovered();
    }

    public void RejectClaim(Guid claimId) => FindClaim(claimId).Reject();

    public int CountPendingClaims() => _claims.Count(claim => claim.Status == ClaimStatus.Pending);

    /// <summary>The status of the most recent claim the user made on this item; null if they never claimed it.</summary>
    public ClaimStatus? LatestClaimStatusOf(Guid claimantId)
        => _claims
            .Where(claim => claim.ClaimantId == claimantId)
            .OrderByDescending(claim => claim.CreatedAt)
            .Select(claim => (ClaimStatus?)claim.Status)
            .FirstOrDefault();

    private LostItemClaim FindClaim(Guid claimId)
        => _claims.FirstOrDefault(claim => claim.Id == claimId) ?? throw new LostItemExceptions.ClaimNotFound();
}
