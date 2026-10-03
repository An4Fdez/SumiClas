namespace SumiClas.Enrollments.Application;

public sealed record EnrollmentResponse(
    Guid Id,
    Guid StudentId,
    Guid ClassId,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ConfirmedAt);
