using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SumiClas.LostItems.Domain;

namespace SumiClas.Shared.Infrastructure.Persistence.Mappings;

/// <summary>
/// Not an IEntityTypeConfiguration&lt;T&gt; on purpose: LostItemClaim only exists owned by LostItem, so it must never
/// be auto-applied via ApplyConfigurationsFromAssembly (that would register it as an independent entity type and
/// conflict with OwnsMany).
/// </summary>
public sealed class LostItemClaimConfiguration
{
    private const string LOST_ITEM_ID = "LostItemId";

    public void Configure(OwnedNavigationBuilder<LostItem, LostItemClaim> builder)
    {
        builder.ToTable("LostItemClaims");

        builder.WithOwner().HasForeignKey(LOST_ITEM_ID);

        builder.HasKey(c => c.Id);

        // The Id comes from the client: without this EF treats a new claim added to a tracked LostItem as an
        // existing row (non-default key) and issues an UPDATE instead of an INSERT.
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.ClaimantId)
            .IsRequired();

        builder.Property(c => c.Message)
            .HasMaxLength(LostItemClaim.MessageMaxLength)
            .IsRequired();

        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(c => c.ResponseMessage)
            .HasMaxLength(LostItemClaim.ResponseMessageMaxLength);

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        // The database backs up the rule of the domain: a user has at most one pending claim on an item.
        builder.HasIndex(LOST_ITEM_ID, nameof(LostItemClaim.ClaimantId))
            .IsUnique()
            .HasFilter($"\"{nameof(LostItemClaim.Status)}\" = '{nameof(ClaimStatus.Pending)}'");
    }
}
