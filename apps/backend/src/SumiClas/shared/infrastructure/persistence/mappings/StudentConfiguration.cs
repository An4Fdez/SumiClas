using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SumiClas.Students.Domain;

namespace SumiClas.Shared.Infrastructure.Persistence.Mappings;

public sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");

        builder.HasKey(s => s.Id);

        builder.OwnsOne(s => s.Name, name =>
        {
            name.Property(n => n.FirstName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("FirstName");

            name.Property(n => n.FirstSurname)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("FirstSurname");

            name.Property(n => n.SecondSurname)
                .HasMaxLength(100)
                .HasColumnName("SecondSurname");
        });
        builder.Navigation(s => s.Name).IsRequired();

        builder.OwnsMany(s => s.Guardians, guardian => new GuardianLinkConfiguration().Configure(guardian));
        builder.Navigation(s => s.Guardians)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(s => s.DomainEvents);
    }
}
