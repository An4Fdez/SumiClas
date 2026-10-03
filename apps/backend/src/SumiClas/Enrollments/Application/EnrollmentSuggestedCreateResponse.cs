namespace SumiClas.Enrollments.Application;

/// <summary>How many suggested enrollments were created and how many students were skipped. It never lists students (privacy of minors).</summary>
public sealed record EnrollmentSuggestedCreateResponse(int Created, int Skipped);
