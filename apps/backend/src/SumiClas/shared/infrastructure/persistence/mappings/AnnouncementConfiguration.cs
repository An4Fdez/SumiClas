using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SumiClas.Announcements.Domain;

namespace SumiClas.Shared.Infrastructure.Persistence.Mappings;

public sealed class AnnouncementConfiguration : IEntityTypeConfiguration<Announcement>
{
    public void Configure(EntityTypeBuilder<Announcement> builder)
    {
        builder.ToTable("Announcements");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Scope)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(a => a.ClassId);

        builder.Property(a => a.SchoolId);

        builder.Property(a => a.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Body)
            .IsRequired();

        builder.Property(a => a.Priority)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(a => a.AuthorId)
            .IsRequired();

        builder.Property(a => a.RelatedEventId);

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        builder.OwnsMany(a => a.ReadReceipts, receipt => new AnnouncementReadReceiptConfiguration().Configure(receipt));
        builder.Navigation(a => a.ReadReceipts)
            .UsePropertyAccessMode(PropertyAccessMode.Field);


        builder.Ignore(a => a.DomainEvents);
    }
}
