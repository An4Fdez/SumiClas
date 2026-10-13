namespace SumiClas.Tests.Students.Application;

using SumiClas.Students.Application;

public static class StudentResponseMother
{
    public static StudentFinderById.StudentResponse WithId(Guid id) => new(id);
}
