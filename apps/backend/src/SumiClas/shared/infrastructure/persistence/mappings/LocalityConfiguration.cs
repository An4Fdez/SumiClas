using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SumiClas.Localities.Domain;

namespace SumiClas.Shared.Infrastructure.Persistence.Mappings;

public sealed class LocalityConfiguration : IEntityTypeConfiguration<Locality>
{
    public void Configure(EntityTypeBuilder<Locality> builder)
    {
        builder.ToTable("Localities");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.ProvinceId)
            .IsRequired();

        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(l => l.NormalizedName)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasIndex(l => new { l.ProvinceId, l.NormalizedName })
            .IsUnique();

        builder.Ignore(l => l.DomainEvents);
    }
}
