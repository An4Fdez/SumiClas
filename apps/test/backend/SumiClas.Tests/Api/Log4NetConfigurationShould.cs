namespace SumiClas.Tests.Api;

using AwesomeAssertions;
using System.Xml.Linq;

/// <summary>
/// The log4net.config the API ships with, read as the file it is: opening it with log4net would create the log file.
/// </summary>
[TestFixture]
public class Log4NetConfigurationShould
{
    private const string CONFIG_FILE = "log4net.config";

    [Test]
    public void send_every_message_to_the_console_and_to_a_rolling_file()
    {
        XElement configuration = Load();

        string[] destinations = configuration.Element("root")!.Elements("appender-ref")
            .Select(reference => (string)reference.Attribute("ref")!)
            .ToArray();

        destinations.Should().BeEquivalentTo(["Console", "RollingFile"]);
        TypeOf(configuration, "Console").Should().Be("log4net.Appender.ConsoleAppender");
        TypeOf(configuration, "RollingFile").Should().Be("log4net.Appender.RollingFileAppender");
    }

    [Test]
    public void keep_a_bounded_number_of_rolled_files()
    {
        XElement rolling = Appender(Load(), "RollingFile");

        ValueOf(rolling, "rollingStyle").Should().Be("Composite");
        ValueOf(rolling, "maximumFileSize").Should().NotBeNullOrWhiteSpace();
        int.Parse(ValueOf(rolling, "maxSizeRollBackups")!).Should().BePositive();
    }

    [TestCase("Console")]
    [TestCase("RollingFile")]
    public void write_the_exception_of_every_message(string appender)
    {
        string pattern = (string)Appender(Load(), appender)
            .Element("layout")!.Element("conversionPattern")!.Attribute("value")!;

        pattern.Should().Contain("%exception");
    }

    private static XElement Load()
    {
        string path = Path.Combine(AppContext.BaseDirectory, CONFIG_FILE);
        File.Exists(path).Should().BeTrue($"{CONFIG_FILE} must be copied next to the API");

        return XDocument.Load(path).Root!;
    }

    private static XElement Appender(XElement configuration, string name)
        => configuration.Elements("appender").Single(appender => (string)appender.Attribute("name")! == name);

    private static string? TypeOf(XElement configuration, string name)
        => (string?)Appender(configuration, name).Attribute("type");

    private static string? ValueOf(XElement appender, string setting)
        => (string?)appender.Element(setting)?.Attribute("value");
}
