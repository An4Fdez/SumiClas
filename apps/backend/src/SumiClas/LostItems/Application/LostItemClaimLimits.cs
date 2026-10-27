using SumiClas.LostItems.Domain;

namespace SumiClas.LostItems.Application;

/// <summary>The text limits of a claim, for the layers above that cannot see the Domain.</summary>
public static class LostItemClaimLimits
{
    public const int MessageMaxLength = LostItemClaim.MessageMaxLength;
    public const int ResponseMessageMaxLength = LostItemClaim.ResponseMessageMaxLength;
}
