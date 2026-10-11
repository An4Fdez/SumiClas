using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SumiClas.Sessions.Domain;

namespace SumiClas.Shared.Infrastructure.Persistence.Mappings;

/// <summary>
/// Not an IEntityTypeConfiguration&lt;T&gt; on purpose: RefreshToken only exists owned by Session, so it must never be
/// auto-applied via ApplyConfigurationsFromAssembly (that would register it as an independent entity type and conflict
/// with OwnsMany).
/// </summary>
public sealed class RefreshTokenConfiguration
{
    public void Configure(OwnedNavigationBuilder<Session, RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.WithOwner().HasForeignKey("SessionId");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.TokenHash)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.ExpiresAt)
            .IsRequired();

        builder.HasIndex("SessionId")
            .HasDatabaseName("IX_RefreshTokens_SessionId");

        builder.HasIndex(t => t.TokenHash)
            .IsUnique()
            .HasDatabaseName("IX_RefreshTokens_TokenHash");
    }
}
