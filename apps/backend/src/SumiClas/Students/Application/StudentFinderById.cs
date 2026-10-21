using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Students.Domain.Services;

namespace SumiClas.Students.Application;

public static class StudentFinderById
{
    public sealed record Query(Guid StudentId) : IQuery<StudentResponse>;

    /// <summary>Deliberately minimal (id only): students are minors, other modules only need to know the student exists.</summary>
    public sealed record StudentResponse(Guid Id);

    public sealed class Finder(StudentFinder finder) : IUseCase
    {
        public async Task<StudentResponse> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            var student = await finder.FindByIdAsync(query.StudentId, ct);
            return new StudentResponse(student.Id);
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, StudentResponse>
    {
        public Task<StudentResponse> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
