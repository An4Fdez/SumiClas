namespace SumiClas.Tests.SchoolYears;

using Moq;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;

public static class SchoolYearsQueryBusExtensions
{
    public static void ShouldEnsureSchoolYearIsActive(this Mock<IQueryBus> queryBus, Guid schoolYearId)
        => queryBus
            .Setup(bus => bus.Send(
                It.Is<SchoolYearEnsureIsActive.Query>(query => query.SchoolYearId == schoolYearId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

    public static void ShouldFailEnsuringSchoolYearIsActive(
        this Mock<IQueryBus> queryBus,
        Guid schoolYearId,
        Error error)
        => queryBus
            .Setup(bus => bus.Send(
                It.Is<SchoolYearEnsureIsActive.Query>(query => query.SchoolYearId == schoolYearId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(error));
}
