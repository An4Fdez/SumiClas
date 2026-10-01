using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Students.Domain.Services;

namespace SumiClas.Students.Application;

public static class StudentFinderById
{
    public sealed record Query(Guid StudentId) : IQuery<Result<StudentResponse>>;

    /// <summary>Deliberately minimal (id only): students are minors, other modules only need to know the student exists.</summary>
    public sealed record StudentResponse(Guid Id);

    public sealed class Handler(StudentFinder finder) : IRequestHandler<Query, Result<StudentResponse>>
    {
        public async Task<Result<StudentResponse>> Handle(Query query, CancellationToken ct)
        {
            var found = await finder.FindByIdAsync(query.StudentId, ct);
            return found.IsFailure
                ? Result<StudentResponse>.Failure(found.Error!)
                : Result<StudentResponse>.Success(new StudentResponse(found.Value.Id));
        }
    }
}
