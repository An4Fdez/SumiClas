using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SumiClas.WeeklySummaries.Domain;

namespace SumiClas.Shared.Infrastructure.Persistence.Mappings;

public sealed class WeeklySummaryConfiguration : IEntityTypeConfiguration<WeeklySummary>
{
    public void Configure(EntityTypeBuilder<WeeklySummary> builder)
    {
        builder.ToTable("WeeklySummaries");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.ClassId)
            .IsRequired();

        builder.Property(s => s.WeekStart)
            .IsRequired();

        builder.Property(s => s.Content)
            .IsRequired();

        builder.Property(s => s.InputHash)
            .HasMaxLength(WeeklySummary.InputHashLength)
            .IsRequired();

        builder.Property(s => s.GeneratedAt)
            .IsRequired();

        // A class has one summary per week: whoever generates it twice meets this index, not a second row.
        builder.HasIndex(s => new { s.ClassId, s.WeekStart })
            .IsUnique();

        builder.Ignore(s => s.DomainEvents);
    }
}
