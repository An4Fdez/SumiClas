using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SumiClas.Provinces.Domain;

namespace SumiClas.Shared.Infrastructure.Persistence.Mappings;

public sealed class ProvinceConfiguration : IEntityTypeConfiguration<Province>
{
    public void Configure(EntityTypeBuilder<Province> builder)
    {
        builder.ToTable("Provinces");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.NormalizedName)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(p => p.NormalizedName)
            .IsUnique();

        builder.Ignore(p => p.DomainEvents);
    }
}
