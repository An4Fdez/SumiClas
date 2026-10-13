namespace SumiClas.Tests.Students;

using Moq;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Students.Application;
using SumiClas.Students.Domain;
using SumiClas.Tests.Students.Application;

public static class StudentsQueryBusExtensions
{
    public static void ShouldFindStudent(this Mock<IQueryBus> queryBus, Guid studentId)
        => queryBus
            .Setup(bus => bus.Send(
                It.Is<StudentFinderById.Query>(query => query.StudentId == studentId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<StudentFinderById.StudentResponse>.Success(StudentResponseMother.WithId(studentId)));

    public static void ShouldNotFindStudent(this Mock<IQueryBus> queryBus, Guid studentId)
        => queryBus
            .Setup(bus => bus.Send(
                It.Is<StudentFinderById.Query>(query => query.StudentId == studentId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<StudentFinderById.StudentResponse>.Failure(StudentErrors.NotFound));
}
