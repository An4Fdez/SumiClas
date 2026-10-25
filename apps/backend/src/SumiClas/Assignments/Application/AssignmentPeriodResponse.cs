namespace SumiClas.Assignments.Application;

/// <summary>A homework of the class as a shared summary may tell it: no author, comments or who has done it.</summary>
/// <param name="Title">What the homework is.</param>
/// <param name="DueDate">The day it is due.</param>
public sealed record AssignmentPeriodResponse(string Title, DateOnly DueDate);
