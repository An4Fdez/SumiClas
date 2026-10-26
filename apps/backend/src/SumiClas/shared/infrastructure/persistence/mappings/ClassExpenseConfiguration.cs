using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SumiClas.ClassExpenses.Domain;

namespace SumiClas.Shared.Infrastructure.Persistence.Mappings;

public sealed class ClassExpenseConfiguration : IEntityTypeConfiguration<ClassExpense>
{
    public void Configure(EntityTypeBuilder<ClassExpense> builder)
    {
        builder.ToTable("ClassExpenses");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ClassId)
            .IsRequired();

        builder.Property(e => e.Item)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.TotalAmount)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(e => e.RelatedEventId);

        builder.Property(e => e.AuthorId)
            .IsRequired();

        builder.Property(e => e.UpdatedAt);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.OwnsMany(e => e.Participants, participant => new ExpenseParticipantConfiguration().Configure(participant));
        builder.Navigation(e => e.Participants)
            .UsePropertyAccessMode(PropertyAccessMode.Field);


        builder.Ignore(e => e.DomainEvents);
    }
}
