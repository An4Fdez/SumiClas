namespace SumiClas.Tests.Assignments.Application;

using SumiClas.Assignments.Application;

public static class AssignmentFinderByClassQueryMother
{
    public static AssignmentFinderByClass.Query With(Guid classId, int? limit, string? cursor)
        => new(classId, limit, cursor);

    public static AssignmentFinderByClass.Query WithClassId(Guid classId) => With(classId, null, null);

    public static AssignmentFinderByClass.Query WithLimit(Guid classId, int limit) => With(classId, limit, null);

    public static AssignmentFinderByClass.Query WithCursor(Guid classId, int limit, string cursor)
        => With(classId, limit, cursor);
}
