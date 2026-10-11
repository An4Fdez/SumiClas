using SumiClas.Shared.Application;

namespace SumiClas.Tests.Shared;

[TestFixture]
public class UseCaseConventionTests
{
    private static readonly System.Reflection.Assembly Library = typeof(IUseCase).Assembly;

    private static Type[] UseCases() =>
        Library.GetTypes().Where(type => type is { IsClass: true, IsAbstract: false } && typeof(IUseCase).IsAssignableFrom(type)).ToArray();

    [Test]
    public void Handlers_DelegateOnlyToNestedClassesThatImplementIUseCase()
    {
        var handlers = Library.GetTypes().Where(type =>
            type is { IsNestedPublic: true, IsClass: true, Name: "Handler" }
            && type.DeclaringType is { IsAbstract: true, IsSealed: true });

        var offenders = new List<string>();
        foreach (var handler in handlers)
        {
            var delegates = handler.GetConstructors()
                .SelectMany(constructor => constructor.GetParameters())
                .Select(parameter => parameter.ParameterType)
                .Where(type => type.DeclaringType == handler.DeclaringType);

            offenders.AddRange(delegates.Where(type => !typeof(IUseCase).IsAssignableFrom(type)).Select(type => type.FullName!));
        }

        Assert.That(offenders, Is.Empty, "Nested classes used by a Handler must implement IUseCase or they are not registered in DI.");
    }

    [Test]
    public void UseCases_ArePublicSealedAndNestedInAPublicStaticClass()
    {
        var offenders = UseCases()
            .Where(type => !(type.IsNestedPublic && type.IsSealed
                && type.DeclaringType is { IsPublic: true, IsAbstract: true, IsSealed: true }))
            .Select(type => type.FullName!);

        Assert.That(offenders, Is.Empty);
    }

    [Test]
    public void UseCases_AreAtLeastSixty()
    {
        Assert.That(UseCases().Length, Is.GreaterThanOrEqualTo(60));
    }
}
