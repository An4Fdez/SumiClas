namespace SumiClas.Tests.Shared;

using AwesomeAssertions;
using SumiClas.Shared.Application;
using System.Reflection;

[TestFixture]
public class UseCaseConventionShould
{
    private static readonly Assembly Library = typeof(IUseCase).Assembly;

    [Test]
    public void make_handlers_delegate_only_to_nested_classes_that_implement_iusecase()
    {
        IEnumerable<Type> handlers = Library.GetTypes().Where(type =>
            type is { IsNestedPublic: true, IsClass: true, Name: "Handler" }
            && type.DeclaringType is { IsAbstract: true, IsSealed: true });

        List<string> offenders = [];
        foreach (Type handler in handlers)
        {
            IEnumerable<Type> delegates = handler.GetConstructors()
                .SelectMany(constructor => constructor.GetParameters())
                .Select(parameter => parameter.ParameterType)
                .Where(type => type.DeclaringType == handler.DeclaringType);

            offenders.AddRange(
                delegates.Where(type => !typeof(IUseCase).IsAssignableFrom(type)).Select(type => type.FullName!));
        }

        offenders.Should().BeEmpty(
            "nested classes used by a Handler must implement IUseCase or they are not registered in DI.");
    }

    [Test]
    public void make_use_cases_public_sealed_and_nested_in_a_public_static_class()
    {
        IEnumerable<string> offenders = UseCases()
            .Where(type => !(type.IsNestedPublic && type.IsSealed
                && type.DeclaringType is { IsPublic: true, IsAbstract: true, IsSealed: true }))
            .Select(type => type.FullName!);

        offenders.Should().BeEmpty();
    }

    [Test]
    public void have_at_least_sixty_use_cases()
    {
        UseCases().Length.Should().BeGreaterThanOrEqualTo(60);
    }

    private static Type[] UseCases()
        => Library.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && typeof(IUseCase).IsAssignableFrom(type))
            .ToArray();
}
