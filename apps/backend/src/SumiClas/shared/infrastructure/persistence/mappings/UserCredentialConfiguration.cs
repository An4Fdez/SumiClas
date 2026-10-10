using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SumiClas.Authentication.Domain;

namespace SumiClas.Shared.Infrastructure.Persistence.Mappings;

public sealed class UserCredentialConfiguration : IEntityTypeConfiguration<UserCredential>
{
    public void Configure(EntityTypeBuilder<UserCredential> builder)
    {
        builder.ToTable("UserCredentials");

        // The key is the id of the user the credential belongs to (no foreign key, modules reference each other by id).
        builder.HasKey(c => c.Id);

        builder.Property(c => c.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.ChangedAt)
            .IsRequired();

        builder.Ignore(c => c.DomainEvents);
    }
}
