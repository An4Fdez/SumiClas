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
    public static Result<Class> Create(Guid id, Guid schoolYearId, string name, Guid creatorId, IClock clock)
    {
        var validation = Validate(id, schoolYearId, name, creatorId);
        if (validation.IsFailure)
        {
            return Result<Class>.Failure(validation.Error!);
        }

        var schoolClass = new Class(id, schoolYearId, name.Trim());
        schoolClass.AddDomainEvent(new ClassCreatedDomainEvent(id, schoolYearId, schoolClass.Name, creatorId, clock));
        return Result<Class>.Success(schoolClass);
    }

    private static Result Validate(Guid id, Guid schoolYearId, string name, Guid creatorId)
    {
        if (id == Guid.Empty)
        {
            return Result.Failure(ClassErrors.IdRequired);
        }
        if (schoolYearId == Guid.Empty)
        {
            return Result.Failure(ClassErrors.SchoolYearRequired);
        }
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(ClassErrors.NameRequired);
        }
        if (creatorId == Guid.Empty)
        {
            return Result.Failure(ClassErrors.CreatorRequired);
        }
        return Result.Success();
    }

    public Result Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            return Result.Failure(ClassErrors.NameRequired);
        }

        Name = newName.Trim();
        return Result.Success();
    }
}
