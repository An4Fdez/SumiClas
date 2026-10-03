using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SumiClas.LostItems.Domain;

namespace SumiClas.Shared.Infrastructure.Persistence.Mappings;

public sealed class LostItemConfiguration : IEntityTypeConfiguration<LostItem>
{
    public void Configure(EntityTypeBuilder<LostItem> builder)
    {
        builder.ToTable("LostItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Scope)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(i => i.ClassId);

        builder.Property(i => i.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(i => i.Recovered)
            .IsRequired();

        builder.Property(i => i.Description)
            .IsRequired();

        builder.Property(i => i.PhotoUrl)
            .HasMaxLength(2048);

        builder.Property(i => i.PhotoPrivacyConfirmed)
            .IsRequired();

        builder.Property(i => i.AuthorId)
            .IsRequired();

        builder.Property(i => i.CreatedAt)
            .IsRequired();


        builder.Ignore(i => i.DomainEvents);
    }
}
