using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SumiClas.Announcements.Domain;

namespace SumiClas.Shared.Infrastructure.Persistence.Mappings;

/// <summary>
/// Not an IEntityTypeConfiguration&lt;T&gt; on purpose: AnnouncementReadReceipt only exists
/// owned by Announcement, so it must never be auto-applied via ApplyConfigurationsFromAssembly
/// (that would register it as an independent entity type and conflict with OwnsMany below).
/// </summary>
public sealed class AnnouncementReadReceiptConfiguration
{
    public void Configure(OwnedNavigationBuilder<Announcement, AnnouncementReadReceipt> builder)
    {
        builder.ToTable("AnnouncementReadReceipts");

        builder.WithOwner().HasForeignKey("AnnouncementId");

        builder.HasKey(r => r.Id);

        // The Id is assigned by the Domain, never by the database: without this EF treats a new receipt added to a
        // tracked Announcement as an existing row (non-default key) and issues an UPDATE instead of an INSERT.
        builder.Property(r => r.Id).ValueGeneratedNever();

        builder.Property(r => r.UserId)
            .IsRequired();

        builder.Property(r => r.ReadAt)
            .IsRequired();
    }
}
