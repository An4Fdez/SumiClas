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

        builder.Property(s => s.OfficialCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Address)
            .HasMaxLength(300);

        builder.Ignore(s => s.DomainEvents);
    }
}
