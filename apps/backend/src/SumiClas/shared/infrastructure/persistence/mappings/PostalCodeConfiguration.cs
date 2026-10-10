using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SumiClas.PostalCodes.Domain;

namespace SumiClas.Shared.Infrastructure.Persistence.Mappings;

public sealed class PostalCodeConfiguration : IEntityTypeConfiguration<PostalCode>
{
    public void Configure(EntityTypeBuilder<PostalCode> builder)
    {
        builder.ToTable("PostalCodes");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.LocalityId)
            .IsRequired();

        builder.Property(p => p.Code)
            .IsRequired()
            .HasMaxLength(PostalCode.Length)
            .IsFixedLength();

        builder.HasIndex(p => new { p.LocalityId, p.Code })
            .IsUnique();

        builder.Ignore(p => p.DomainEvents);
    }
}
