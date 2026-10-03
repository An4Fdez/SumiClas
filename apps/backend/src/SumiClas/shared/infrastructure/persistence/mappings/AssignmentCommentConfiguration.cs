using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SumiClas.Assignments.Domain;

namespace SumiClas.Shared.Infrastructure.Persistence.Mappings;

/// <summary>
/// Not an IEntityTypeConfiguration&lt;T&gt; on purpose: AssignmentComment only exists owned by
/// Assignment, so it must never be auto-applied via ApplyConfigurationsFromAssembly (that would
/// register it as an independent entity type and conflict with OwnsMany below).
/// </summary>
public sealed class AssignmentCommentConfiguration
{
    public void Configure(OwnedNavigationBuilder<Assignment, AssignmentComment> builder)
    {
        builder.ToTable("AssignmentComments");

        builder.WithOwner().HasForeignKey("AssignmentId");

        builder.HasKey(c => c.Id);

        // The Id is assigned by the Domain, never by the database: without this EF treats a new comment added to a
        // tracked Assignment as an existing row (non-default key) and issues an UPDATE instead of an INSERT.
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.AuthorId)
            .IsRequired();

        builder.Property(c => c.Text)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .IsRequired();
    }
}
