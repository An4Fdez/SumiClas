using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SumiClas.CalendarEvents.Domain;

namespace SumiClas.Shared.Infrastructure.Persistence.Mappings;

public sealed class CalendarEventConfiguration : IEntityTypeConfiguration<CalendarEvent>
{
    public void Configure(EntityTypeBuilder<CalendarEvent> builder)
    {
        builder.ToTable("CalendarEvents");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Scope)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(e => e.ClassId);

        builder.Property(e => e.SchoolId);

        builder.Property(e => e.StudentId);

        builder.Property(e => e.Type)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.OwnsOne(e => e.EventDate, date =>
        {
            date.Property(d => d.Month)
                .IsRequired()
                .HasColumnName("EventMonth");

            date.Property(d => d.Day)
                .IsRequired()
                .HasColumnName("EventDay");

            date.Property(d => d.Year)
                .HasColumnName("EventYear");

            date.Ignore(d => d.IsRecurringAnnual);
        });
        builder.Navigation(e => e.EventDate).IsRequired();

        builder.Property(e => e.Time);

        builder.Property(e => e.Location)
            .HasMaxLength(200);

        builder.Property(e => e.Cost)
            .HasMaxLength(100);

        builder.Property(e => e.AuthorId)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.UpdatedAt);

        builder.Ignore(e => e.DomainEvents);
    }
}
