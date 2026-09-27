using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SumiClas.Students.Domain;

namespace SumiClas.Shared.Infrastructure.Persistence.Mappings;

/// <summary>
/// Not an IEntityTypeConfiguration&lt;T&gt; on purpose: GuardianLink only exists owned by
/// Student, so it must never be auto-applied via ApplyConfigurationsFromAssembly (that
/// would register it as an independent entity type and conflict with OwnsMany below).
/// </summary>
public sealed class GuardianLinkConfiguration
{
    public void Configure(OwnedNavigationBuilder<Student, GuardianLink> builder)
    {
        builder.ToTable("StudentGuardians");

        builder.WithOwner().HasForeignKey("StudentId");

        builder.HasKey("StudentId", nameof(GuardianLink.GuardianId));

        builder.Property(g => g.GuardianId)
            .IsRequired();

        builder.Property(g => g.Role)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();
    }
}
