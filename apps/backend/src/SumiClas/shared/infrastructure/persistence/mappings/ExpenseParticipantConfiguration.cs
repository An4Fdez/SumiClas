using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SumiClas.ClassExpenses.Domain;

namespace SumiClas.Shared.Infrastructure.Persistence.Mappings;

/// <summary>
/// Not an IEntityTypeConfiguration&lt;T&gt; on purpose: ExpenseParticipant only exists owned by
/// ClassExpense, so it must never be auto-applied via ApplyConfigurationsFromAssembly (that
/// would register it as an independent entity type and conflict with OwnsMany below).
/// </summary>
public sealed class ExpenseParticipantConfiguration
{
    public void Configure(OwnedNavigationBuilder<ClassExpense, ExpenseParticipant> builder)
    {
        builder.ToTable("ClassExpenseParticipants");

        builder.WithOwner().HasForeignKey("ClassExpenseId");

        builder.HasKey("ClassExpenseId", nameof(ExpenseParticipant.GuardianId));

        builder.Property(p => p.GuardianId)
            .IsRequired();

        builder.Property(p => p.HasPaid)
            .IsRequired();
    }
}
