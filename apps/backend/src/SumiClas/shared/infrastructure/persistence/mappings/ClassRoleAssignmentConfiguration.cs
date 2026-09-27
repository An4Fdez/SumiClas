using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SumiClas.Users.Domain;

namespace SumiClas.Shared.Infrastructure.Persistence.Mappings;

/// <summary>
/// Not an IEntityTypeConfiguration&lt;T&gt; on purpose: ClassRoleAssignment only exists owned
/// by User, so it must never be auto-applied via ApplyConfigurationsFromAssembly (that would
/// register it as an independent entity type and conflict with OwnsMany below).
/// </summary>
public sealed class ClassRoleAssignmentConfiguration
{
    public void Configure(OwnedNavigationBuilder<User, ClassRoleAssignment> builder)
    {
        builder.ToTable("UserClassRoles");

        builder.WithOwner().HasForeignKey("UserId");

        builder.HasKey("UserId", nameof(ClassRoleAssignment.ClassId));

        builder.Property(r => r.ClassId)
            .IsRequired();

        builder.Property(r => r.Role)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();
    }
}
