using SumiClas.Shared.Domain;

namespace SumiClas.LostItems.Domain;

/// <summary>
/// Child entity of the LostItem aggregate: a private offer from a class member ("it is my child's" or "I have it"),
/// with the detail that proves it. Only LostItem (via AddClaim) can create one, and only its author resolves it.
/// </summary>
public sealed class LostItemClaim : Entity
{
    /// <summary>The longest message a claim can have; it is checked in the request and the column, not here.</summary>
    public const int MessageMaxLength = 300;

    /// <summary>The longest coordination line the author can leave when accepting.</summary>
    public const int ResponseMessageMaxLength = 200;

    public Guid ClaimantId { get; private set; }
    public string Message { get; private set; }
    public ClaimStatus Status { get; private set; }

    /// <summary>The author's coordination line; only an accepted claim can have one.</summary>
    public string? ResponseMessage { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public LostItemClaim(
        Guid id,
        Guid claimantId,
        string message,
        ClaimStatus status,
        string? responseMessage,
        DateTimeOffset createdAt) : base(id)
    {
        ClaimantId = claimantId;
        Message = message;
        Status = status;
        ResponseMessage = responseMessage;
        CreatedAt = createdAt;
    }

    internal static LostItemClaim Create(Guid id, Guid claimantId, string message, IClock clock)
    {
        Validate(id, claimantId, message);

        return new LostItemClaim(id, claimantId, message.Trim(), ClaimStatus.Pending, null, clock.UtcNow);
    }

    internal void Accept(string? responseMessage)
    {
        EnsureIsPending();

        Status = ClaimStatus.Accepted;
        ResponseMessage = string.IsNullOrWhiteSpace(responseMessage) ? null : responseMessage.Trim();
    }

    internal void Reject()
    {
        EnsureIsPending();

        Status = ClaimStatus.Rejected;
    }

    private void EnsureIsPending()
    {
        if (Status != ClaimStatus.Pending)
        {
            throw new LostItemExceptions.ClaimNotPending();
        }
    }

    private static void Validate(Guid id, Guid claimantId, string message)
    {
        if (id == Guid.Empty)
        {
            throw new LostItemExceptions.ClaimIdRequired();
        }
        if (claimantId == Guid.Empty)
        {
            throw new LostItemExceptions.ClaimantRequired();
        }
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new LostItemExceptions.ClaimMessageRequired();
        }
    }
}
