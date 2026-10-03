using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Enrollments.Domain;

namespace SumiClas.Enrollments.Application;

public static class EnrollmentFinderByClass
{
    public sealed record Query(Guid ClassId) : IQuery<IReadOnlyCollection<EnrollmentResponse>>;

    public sealed class Handler(IEnrollmentRepository enrollmentRepository)
        : IRequestHandler<Query, IReadOnlyCollection<EnrollmentResponse>>
    {
        public async Task<IReadOnlyCollection<EnrollmentResponse>> Handle(Query query, CancellationToken ct)
        {
            var enrollments = await enrollmentRepository.SearchAsync(new EnrollmentSpecification(classId: query.ClassId), ct);
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
}
