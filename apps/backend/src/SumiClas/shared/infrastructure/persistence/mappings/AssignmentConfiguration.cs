using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SumiClas.Assignments.Domain;

namespace SumiClas.Shared.Infrastructure.Persistence.Mappings;

public sealed class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> builder)
    {
        builder.ToTable("Assignments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.ClassId)
            .IsRequired();

        builder.Property(a => a.SubjectId)
            .IsRequired();

        builder.Property(a => a.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Description);

        builder.Property(a => a.DueDate)
            .IsRequired();

        builder.Property(a => a.AuthorId)
            .IsRequired();

        builder.Property(a => a.IsProposal)
            .IsRequired();

        builder.Property(a => a.UpdatedAt);

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        builder.OwnsMany(a => a.Comments, comment => new AssignmentCommentConfiguration().Configure(comment));
        builder.Navigation(a => a.Comments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(a => a.Completions, completion => new AssignmentCompletionConfiguration().Configure(completion));
        builder.Navigation(a => a.Completions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);


        builder.Ignore(a => a.DomainEvents);
    }
}
