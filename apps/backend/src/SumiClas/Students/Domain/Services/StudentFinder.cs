namespace SumiClas.Students.Domain.Services;

public sealed class StudentFinder(IStudentRepository students)
{
    public async Task<Student> FindByIdAsync(Guid id, CancellationToken ct = default)
        => await students.GetByIdAsync(id, ct) ?? throw new StudentExceptions.NotFound();
}
