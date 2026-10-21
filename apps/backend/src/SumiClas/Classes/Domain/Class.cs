using SumiClas.Shared.Domain;
using SumiClas.Shared.Domain.Events;

namespace SumiClas.Classes.Domain;

public sealed class Class : AggregateRoot
{
    public Guid SchoolYearId { get; private set; }
    public string Name { get; private set; }

    public Class(Guid id, Guid schoolYearId, string name) : base(id)
    {
        SchoolYearId = schoolYearId;
        Name = name;
    }

    /// <param name="creatorId">Not stored in Class: it travels in the <see cref="ClassCreatedDomainEvent"/> so the creator becomes the first delegate.</param>
    public static Class Create(Guid id, Guid schoolYearId, string name, Guid creatorId, IClock clock)
    {
        Validate(id, schoolYearId, name, creatorId);

        var schoolClass = new Class(id, schoolYearId, name.Trim());
        schoolClass.AddDomainEvent(new ClassCreatedDomainEvent(id, schoolYearId, schoolClass.Name, creatorId, clock));
        return schoolClass;
    }

    private static void Validate(Guid id, Guid schoolYearId, string name, Guid creatorId)
    {
        if (id == Guid.Empty)
        {
            throw new ClassExceptions.IdRequired();
        }
        if (schoolYearId == Guid.Empty)
        {
            throw new ClassExceptions.SchoolYearRequired();
        }
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ClassExceptions.NameRequired();
        }
        if (creatorId == Guid.Empty)
        {
            throw new ClassExceptions.CreatorRequired();
        }
    }

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new ClassExceptions.NameRequired();
        }

        Name = newName.Trim();
    }
}
