using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SumiClas.Schools.Domain;

namespace SumiClas.Shared.Infrastructure.Persistence.Mappings;

public sealed class SchoolConfiguration : IEntityTypeConfiguration<School>
{
    public void Configure(EntityTypeBuilder<School> builder)
    {
        builder.ToTable("Schools");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.NormalizedName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.OfficialCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.LocalityId)
            .IsRequired();

        builder.Property(s => s.Address)
            .HasMaxLength(300);

        builder.Property(s => s.Phone)
            .HasMaxLength(20);

        builder.HasIndex(s => s.OfficialCode)
            .IsUnique();

        builder.HasIndex(s => new { s.LocalityId, s.NormalizedName });

        builder.Ignore(s => s.DomainEvents);
    }
}
