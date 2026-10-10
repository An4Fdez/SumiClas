using Microsoft.EntityFrameworkCore;
using SumiClas.Announcements.Domain;
using SumiClas.Assignments.Domain;
using SumiClas.CalendarEvents.Domain;
using SumiClas.Classes.Domain;
using SumiClas.ClassExpenses.Domain;
using SumiClas.Enrollments.Domain;
using SumiClas.LostItems.Domain;
using SumiClas.Schools.Domain;
using SumiClas.Shared.Application;
using SumiClas.SchoolYears.Domain;
using SumiClas.Students.Domain;
using SumiClas.Subjects.Domain;
using SumiClas.Users.Domain;

namespace SumiClas.Shared.Infrastructure.Persistence;

public sealed class SumiClasDbContext : DbContext, IUnitOfWork
{
    public SumiClasDbContext(DbContextOptions<SumiClasDbContext> options) : base(options)
    {
    }

    public DbSet<School> Schools => Set<School>();
    public DbSet<SchoolYear> SchoolYears => Set<SchoolYear>();
    public DbSet<Class> Classes => Set<Class>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<CalendarEvent> CalendarEvents => Set<CalendarEvent>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<ClassExpense> ClassExpenses => Set<ClassExpense>();
    public DbSet<LostItem> LostItems => Set<LostItem>();

    public bool HasActiveTransaction => Database.CurrentTransaction is not null;

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        catch (DbUpdateException exception) when (UniqueViolationDetector.TryGetConstraintName(exception, out var constraintName))
        {
            throw new DuplicateKeyException(constraintName, exception);
        }
    }

    public async Task BeginTransactionAsync(CancellationToken ct = default)
        => await Database.BeginTransactionAsync(ct);

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        var transaction = Database.CurrentTransaction;
        if (transaction is null)
        {
            return;
        }

        await using (transaction)
        {
            await transaction.CommitAsync(ct);
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        var transaction = Database.CurrentTransaction;
        if (transaction is null)
        {
            return;
        }

        await using (transaction)
        {
            await transaction.RollbackAsync(ct);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SumiClasDbContext).Assembly);
    }
}
