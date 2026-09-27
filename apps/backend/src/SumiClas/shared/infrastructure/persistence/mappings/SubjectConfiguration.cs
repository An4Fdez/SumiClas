using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SumiClas.Subjects.Domain;

namespace SumiClas.Shared.Infrastructure.Persistence.Mappings;

public sealed class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        builder.ToTable("Subjects");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.ClassId)
            .IsRequired();

        builder.Property(s => s.Label)
            .IsRequired()
            .HasMaxLength(100);

        builder.Ignore(s => s.DomainEvents);
    }
}
