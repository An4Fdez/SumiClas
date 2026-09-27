using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SumiClas.Users.Domain;

namespace SumiClas.Shared.Infrastructure.Persistence.Mappings;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Value)
                .IsRequired()
                .HasMaxLength(320)
                .HasColumnName("Email");
        });
        builder.Navigation(u => u.Email).IsRequired();

        builder.OwnsMany(u => u.ClassRoles, role => new ClassRoleAssignmentConfiguration().Configure(role));
        builder.Navigation(u => u.ClassRoles)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(u => u.DomainEvents);
    }
}
