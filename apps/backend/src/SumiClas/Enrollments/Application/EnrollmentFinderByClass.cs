using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Enrollments.Domain;

namespace SumiClas.Enrollments.Application;

public static class EnrollmentFinderByClass
{
    public sealed record Query(Guid ClassId) : IQuery<IReadOnlyCollection<EnrollmentResponse>>;

    public sealed class Finder(IEnrollmentRepository repository) : IUseCase
    {
        public async Task<IReadOnlyCollection<EnrollmentResponse>> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            IReadOnlyCollection<Enrollment> enrollments = await repository.SearchAsync(new EnrollmentSpecification(classId: query.ClassId), ct);
            return enrollments
                .Select(enrollment => new EnrollmentResponse(
                    enrollment.Id,
                    enrollment.StudentId,
                    enrollment.ClassId,
                    enrollment.Status.ToString(),
                    enrollment.CreatedAt,
                    enrollment.ConfirmedAt))
                .ToList();
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, IReadOnlyCollection<EnrollmentResponse>>
    {
        public Task<IReadOnlyCollection<EnrollmentResponse>> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
