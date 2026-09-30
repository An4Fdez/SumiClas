using SumiClas.Shared.Domain;

namespace SumiClas.Classes.Domain;

public sealed class Class : AggregateRoot
{
    public Guid SchoolYearId { get; private set; }
    public string Name { get; private set; }

    private Class(Guid id, Guid schoolYearId, string name) : base(id)
    {
        SchoolYearId = schoolYearId;
        Name = name;
    }

    public static Result<Class> Create(Guid id, Guid schoolYearId, string name)
    {
        var validation = Validate(id, schoolYearId, name);
        if (validation.IsFailure)
        {
            return Result<Class>.Failure(validation.Error!);
        }

        var schoolClass = new Class(id, schoolYearId, name.Trim());
        return Result<Class>.Success(schoolClass);
    }

    private static Result Validate(Guid id, Guid schoolYearId, string name)
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
