namespace SumiClas.Tests.Shared;

using AwesomeAssertions;
using SumiClas.Shared.Domain;
using System.Reflection;

[TestFixture]
public class DomainExceptionConventionShould
{
    private const string SUFFIX = "Exceptions";

    private static readonly Assembly Library = typeof(DomainException).Assembly;

    [Test]
    public void declare_every_domain_exception_sealed_and_nested_in_a_public_static_module_exceptions_class()
    {
        IEnumerable<string> offenders = DomainExceptions()
            .Where(type => !(type is { IsNestedPublic: true, IsSealed: true }
                && type.DeclaringType is { IsAbstract: true, IsSealed: true }
                && type.DeclaringType.Name.EndsWith(SUFFIX, StringComparison.Ordinal)))
            .Select(type => type.FullName!);

        offenders.Should().BeEmpty();
    }

    [Test]
    public void make_the_code_of_every_domain_exception_its_module_and_its_name()
    {
        IEnumerable<string> offenders = DomainExceptions()
            .Select(type => (Type: type, Instance: (DomainException)Activator.CreateInstance(type)!))
            .Where(entry => entry.Instance.Code != ExpectedCode(entry.Type))
            .Select(entry => $"{entry.Type.FullName}: {entry.Instance.Code}");

        offenders.Should().BeEmpty("clients branch on the code, so it must be stable and spelled after the type.");
    }

    [Test]
    public void give_every_domain_exception_a_message_for_the_user()
    {
        IEnumerable<string> offenders = DomainExceptions()
            .Select(type => (Type: type, Instance: (DomainException)Activator.CreateInstance(type)!))
            .Where(entry => string.IsNullOrWhiteSpace(entry.Instance.Message))
            .Select(entry => entry.Type.FullName!);

        offenders.Should().BeEmpty();
    }

    [Test]
    public void have_at_least_one_hundred_and_thirty_domain_exceptions()
    {
        DomainExceptions().Length.Should().BeGreaterThanOrEqualTo(130);
    }

    private static string ExpectedCode(Type type)
    {
        string module = type.DeclaringType!.Name[..^SUFFIX.Length];
        return $"{module}.{type.Name}";
    }

    private static Type[] DomainExceptions()
        => Library.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && typeof(DomainException).IsAssignableFrom(type))
            .ToArray();
}
