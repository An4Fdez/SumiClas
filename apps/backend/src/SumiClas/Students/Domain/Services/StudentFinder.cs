using SumiClas.Shared.Domain;

namespace SumiClas.Students.Domain.Services;

public sealed class StudentFinder(IStudentRepository students)
{
    public async Task<Result<Student>> FindByIdAsync(Guid id, CancellationToken ct = default)
    {
        var student = await students.GetByIdAsync(id, ct);
        return student is null
            ? Result<Student>.Failure(StudentErrors.NotFound)
            : Result<Student>.Success(student);
    }
}
