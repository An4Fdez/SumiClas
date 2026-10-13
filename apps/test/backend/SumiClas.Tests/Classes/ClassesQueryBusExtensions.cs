namespace SumiClas.Tests.Classes;

using Moq;
using SumiClas.Classes.Application;
using SumiClas.Classes.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Classes.Application;

public static class ClassesQueryBusExtensions
{
    public static void ShouldFindClass(this Mock<IQueryBus> queryBus, Guid classId)
        => queryBus
            .Setup(bus => bus.Send(It.IsAny<IQuery<Result<ClassResponse>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ClassResponse>.Success(ClassResponseMother.WithId(classId)));

    public static void ShouldNotFindClass(this Mock<IQueryBus> queryBus)
        => queryBus
            .Setup(bus => bus.Send(It.IsAny<IQuery<Result<ClassResponse>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ClassResponse>.Failure(ClassErrors.NotFound));
}
