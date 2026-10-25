namespace SumiClas.Tests.Assignments.Domain;

using SumiClas.Assignments.Domain;

public static class AssignmentSpecificationMother
{
    public static AssignmentSpecification DueFrom(DateOnly day) => new(dueFrom: day);

    public static AssignmentSpecification DueTo(DateOnly day) => new(dueTo: day);

    public static AssignmentSpecification NotDoneBy(Guid userId) => new(notDoneByUserId: userId);
}
