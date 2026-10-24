namespace SumiClas.Assignments.Application;

/// <summary>What the current user still has to do: <paramref name="Pending"/> counts the assignments not done yet that
/// are due today or later and <paramref name="NextDueDate"/> is the earliest of their due dates (null when none).</summary>
public sealed record AssignmentPendingSummaryResponse(int Pending, DateOnly? NextDueDate);
