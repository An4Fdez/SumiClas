namespace SumiClas.Tests.Schools;

using Moq;
using SumiClas.Schools.Application;
using SumiClas.Schools.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Schools.Application;

public static class SchoolsQueryBusExtensions
{
    public static void ShouldFindSchool(this Mock<IQueryBus> queryBus, Guid schoolId)
        => queryBus
            .Setup(bus => bus.Send(
                It.Is<SchoolFinderById.Query>(query => query.SchoolId == schoolId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<SchoolResponse>.Success(SchoolResponseMother.WithId(schoolId)));

    public static void ShouldNotFindSchool(this Mock<IQueryBus> queryBus, Guid schoolId)
        => queryBus
            .Setup(bus => bus.Send(
                It.Is<SchoolFinderById.Query>(query => query.SchoolId == schoolId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<SchoolResponse>.Failure(SchoolErrors.NotFound));
}
