using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SumiClas.SchoolYears.Domain;

namespace SumiClas.Shared.Infrastructure.Persistence.Mappings;

public sealed class SchoolYearConfiguration : IEntityTypeConfiguration<SchoolYear>
{
    public void Configure(EntityTypeBuilder<SchoolYear> builder)
    {
        builder.ToTable("SchoolYears");

        builder.HasKey(sy => sy.Id);

        builder.Property(sy => sy.SchoolId)
            .IsRequired();

        builder.Property(sy => sy.Label)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(sy => sy.StartDate)
            .IsRequired();

        builder.Property(sy => sy.EndDate)
            .IsRequired();

        builder.Property(sy => sy.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Ignore(sy => sy.IsArchived);
        builder.Ignore(sy => sy.DomainEvents);
    }
}
