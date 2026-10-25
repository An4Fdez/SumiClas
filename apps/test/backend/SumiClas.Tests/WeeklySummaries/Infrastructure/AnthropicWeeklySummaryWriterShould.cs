namespace SumiClas.Tests.WeeklySummaries.Infrastructure;

using AwesomeAssertions;
using AwesomeAssertions.Specialized;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SumiClas.Api.Configuration;
using SumiClas.Tests.Shared;
using SumiClas.Tests.WeeklySummaries.Application;
using SumiClas.WeeklySummaries.Application;
using SumiClas.WeeklySummaries.Infrastructure;
using System.Diagnostics;
using System.Net;
using System.Text.Json;

[TestFixture]
public class AnthropicWeeklySummaryWriterShould
{
    private const int NO_RETRIES = 0;
    private const int TWO_RETRIES = 2;
    private const int ONE_RETRY = 1;
    private const int ATTEMPTS_WITH_TWO_RETRIES = TWO_RETRIES + 1;
    private const int SECOND_PAUSE_FACTOR = 2;
    private const int TIMER_TOLERANCE = 2;
    private const int PATIENCE_SECONDS = 10;
    private const int ONE_SECOND = 1;
    private const int THREE_SECONDS = 3;
    private const int LONGER_THAN_THE_CAP_SECONDS = 60;
    private const string THINKING_OFF = "disabled";
    private const string USER_ROLE = "user";

    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(PATIENCE_SECONDS);

    private FakeHttpMessageHandler _handler = null!;
    private List<TimeSpan> _pauses = null!;

    [SetUp]
    public void create_the_network_and_the_clock_of_the_test()
    {
        this._handler = new FakeHttpMessageHandler();
        this._pauses = [];
    }

    [TearDown]
    public void release_the_network_of_the_test()
    {
        this._handler.Dispose();
    }

    [Test]
    public async Task send_the_prompt_to_the_messages_endpoint_with_the_key_and_the_version()
    {
        AiOptions options = AiOptionsMother.Enabled();
        WeeklySummaryInput input = AnInput();
        WeeklySummaryPrompt prompt = WeeklySummaryPromptBuilder.Build(input);
        this._handler.Reply(HttpStatusCode.OK, AnthropicResponseMother.Message(StringMother.Random("text")));

        await Writer(options).WriteAsync(input);

        RecordedRequest request = this._handler.Requests.Should().ContainSingle().Which;
        request.Method.Should().Be(HttpMethod.Post);
        request.Uri.Should().Be(
            new Uri(AnthropicWeeklySummaryWriter.BaseAddress + AnthropicWeeklySummaryWriter.MessagesPath));
        request.Headers[AnthropicWeeklySummaryWriter.ApiKeyHeader].Should().Be(options.ApiKey);
        request.Headers[AnthropicWeeklySummaryWriter.VersionHeader]
            .Should().Be(AnthropicWeeklySummaryWriter.ApiVersion);
        using JsonDocument body = JsonDocument.Parse(request.Body);
        JsonElement root = body.RootElement;
        root.GetProperty("model").GetString().Should().Be(options.Model);
        root.GetProperty("max_tokens").GetInt32().Should().Be(AnthropicWeeklySummaryWriter.MaxOutputTokens);
        root.GetProperty("system").GetString().Should().Be(prompt.System);
        root.GetProperty("thinking").GetProperty("type").GetString().Should().Be(THINKING_OFF);
        JsonElement message = root.GetProperty("messages").EnumerateArray().Should().ContainSingle().Which;
        message.GetProperty("role").GetString().Should().Be(USER_ROLE);
        message.GetProperty("content").GetString().Should().Be(prompt.User);
    }

    [Test]
    public async Task return_the_text_of_the_answer_without_the_blanks_around()
    {
        string text = StringMother.Random("text");
        this._handler.Reply(HttpStatusCode.OK, AnthropicResponseMother.Message($"  {text}\n"));

        string summary = await Writer().WriteAsync(AnInput());

        summary.Should().Be(text);
    }

    [Test]
    public async Task join_the_text_blocks_and_leave_the_other_blocks_out()
    {
        string first = StringMother.Random("first");
        string second = StringMother.Random("second");
        object[] blocks =
        [
            new { type = "thinking", thinking = StringMother.Random("thought"), text = StringMother.Random("ignored") },
            new { type = "text", text = first },
            new { type = "text", text = second },
        ];
        this._handler.Reply(HttpStatusCode.OK, AnthropicResponseMother.Blocks(blocks));

        string summary = await Writer().WriteAsync(AnInput());

        summary.Should().Be(first + second);
    }

    [Test]
    public async Task refuse_to_work_without_a_key_and_never_call_the_service()
    {
        Func<Task> act = () => Writer(AiOptionsMother.WithoutKey()).WriteAsync(AnInput());

        await act.Should().ThrowExactlyAsync<WeeklySummaryWriterException>().WithMessage("*Ai:ApiKey*");
        this._handler.Requests.Should().BeEmpty();
    }

    [TestCase(HttpStatusCode.RequestTimeout)]
    [TestCase(HttpStatusCode.InternalServerError)]
    public async Task try_again_when_the_service_fails_for_now(HttpStatusCode status)
    {
        string text = StringMother.Random("text");
        this._handler
            .Reply(status, AnthropicResponseMother.Error("overloaded_error"))
            .Reply(HttpStatusCode.OK, AnthropicResponseMother.Message(text));

        string summary = await Writer(AiOptionsMother.Enabled(maxRetries: ONE_RETRY)).WriteAsync(AnInput());

        summary.Should().Be(text);
        this._handler.Requests.Should().HaveCount(ONE_RETRY + 1);
    }

    [TestCase(HttpStatusCode.Forbidden)]
    public async Task fail_at_once_when_the_service_rejects_the_request(HttpStatusCode status)
    {
        this._handler.Reply(status, AnthropicResponseMother.Error("authentication_error"));

        Func<Task> act = () => Writer(AiOptionsMother.Enabled(maxRetries: TWO_RETRIES)).WriteAsync(AnInput());

        await act.Should().ThrowExactlyAsync<WeeklySummaryWriterException>()
            .WithMessage($"*{(int)status}*(authentication_error)*");
        this._handler.Requests.Should().ContainSingle();
        this._pauses.Should().BeEmpty();
    }

    [Test]
    public async Task give_up_after_the_retries_it_is_allowed()
    {
        for (int attempt = 0; attempt < ATTEMPTS_WITH_TWO_RETRIES; attempt++)
        {
            this._handler.Reply(HttpStatusCode.InternalServerError, string.Empty);
        }

        Func<Task> act = () => Writer(AiOptionsMother.Enabled(maxRetries: TWO_RETRIES)).WriteAsync(AnInput());

        await act.Should().ThrowExactlyAsync<WeeklySummaryWriterException>().WithMessage("*500*");
        this._handler.Requests.Should().HaveCount(ATTEMPTS_WITH_TWO_RETRIES);
    }

    [Test]
    public async Task not_try_again_when_it_is_allowed_no_retries()
    {
        this._handler.Reply(HttpStatusCode.InternalServerError, string.Empty);

        Func<Task> act = () => Writer(AiOptionsMother.Enabled(maxRetries: NO_RETRIES)).WriteAsync(AnInput());

        await act.Should().ThrowExactlyAsync<WeeklySummaryWriterException>();
        this._handler.Requests.Should().ContainSingle();
        this._pauses.Should().BeEmpty();
    }

    [Test]
    public async Task try_again_after_a_network_failure_and_keep_it_as_the_cause_when_it_gives_up()
    {
        HttpRequestException failure = new(StringMother.Random("network"));
        this._handler.Fail(failure).Fail(failure);

        Func<Task> act = () => Writer(AiOptionsMother.Enabled(maxRetries: ONE_RETRY)).WriteAsync(AnInput());

        ExceptionAssertions<WeeklySummaryWriterException> thrown =
            await act.Should().ThrowExactlyAsync<WeeklySummaryWriterException>();
        thrown.Which.InnerException.Should().BeSameAs(failure);
        this._handler.Requests.Should().HaveCount(ONE_RETRY + 1);
    }

    [Test]
    public async Task try_again_after_a_timeout()
    {
        string text = StringMother.Random("text");
        this._handler.Hang().Reply(HttpStatusCode.OK, AnthropicResponseMother.Message(text));

        string summary = await Writer(AiOptionsMother.Enabled(timeoutSeconds: ONE_SECOND, maxRetries: ONE_RETRY))
            .WriteAsync(AnInput()).WaitAsync(Patience);

        summary.Should().Be(text);
        this._handler.Requests.Should().HaveCount(ONE_RETRY + 1);
    }

    [Test]
    public async Task say_how_long_it_waited_when_the_service_never_answers()
    {
        this._handler.Hang();

        Func<Task> act = () => Writer(AiOptionsMother.Enabled(timeoutSeconds: ONE_SECOND, maxRetries: NO_RETRIES))
            .WriteAsync(AnInput()).WaitAsync(Patience);

        await act.Should().ThrowExactlyAsync<WeeklySummaryWriterException>().WithMessage($"*{ONE_SECOND} seconds*");
    }

    [Test]
    public async Task stop_without_trying_again_when_the_caller_cancels()
    {
        using CancellationTokenSource cancellation = new();
        this._handler.Hang();
        Task<string> writing = Writer(AiOptionsMother.Enabled(maxRetries: TWO_RETRIES))
            .WriteAsync(AnInput(), cancellation.Token);
        await this._handler.FirstRequest.WaitAsync(Patience);

        await cancellation.CancelAsync();

        Func<Task> act = () => writing.WaitAsync(Patience);
        await act.Should().ThrowAsync<OperationCanceledException>();
        this._handler.Requests.Should().ContainSingle();
        this._pauses.Should().BeEmpty();
    }

    [Test]
    public async Task pause_longer_before_each_new_attempt()
    {
        for (int attempt = 0; attempt < ATTEMPTS_WITH_TWO_RETRIES; attempt++)
        {
            this._handler.Reply(HttpStatusCode.InternalServerError, string.Empty);
        }

        Func<Task> act = () => Writer(AiOptionsMother.Enabled(maxRetries: TWO_RETRIES)).WriteAsync(AnInput());
        await act.Should().ThrowExactlyAsync<WeeklySummaryWriterException>();

        this._pauses.Should().Equal(
            AnthropicWeeklySummaryWriter.BaseRetryDelay,
            AnthropicWeeklySummaryWriter.BaseRetryDelay * SECOND_PAUSE_FACTOR);
    }

    [Test]
    public async Task really_wait_between_attempts_when_nobody_replaces_the_pause()
    {
        this._handler
            .Reply(HttpStatusCode.InternalServerError, string.Empty)
            .Reply(HttpStatusCode.OK, AnthropicResponseMother.Message(StringMother.Random("text")));
        using HttpClient client = new(this._handler)
        {
            BaseAddress = new Uri(AnthropicWeeklySummaryWriter.BaseAddress),
        };
        IOptions<AiOptions> options = Options.Create(AiOptionsMother.Enabled(maxRetries: ONE_RETRY));
        AnthropicWeeklySummaryWriter writer = new(client, options);
        Stopwatch stopwatch = Stopwatch.StartNew();

        await writer.WriteAsync(AnInput());

        stopwatch.Elapsed.Should().BeGreaterThan(AnthropicWeeklySummaryWriter.BaseRetryDelay / TIMER_TOLERANCE);
    }

    [Test]
    public async Task wait_what_the_service_asks_for_when_it_is_asked_to_slow_down()
    {
        this._handler
            .Reply(HttpStatusCode.TooManyRequests, string.Empty, TimeSpan.FromSeconds(THREE_SECONDS))
            .Reply(HttpStatusCode.OK, AnthropicResponseMother.Message(StringMother.Random("text")));

        await Writer(AiOptionsMother.Enabled(maxRetries: ONE_RETRY)).WriteAsync(AnInput());

        this._pauses.Should().Equal(TimeSpan.FromSeconds(THREE_SECONDS));
    }

    [Test]
    public async Task not_wait_longer_than_the_cap_even_if_the_service_asks_for_it()
    {
        this._handler
            .Reply(HttpStatusCode.TooManyRequests, string.Empty, TimeSpan.FromSeconds(LONGER_THAN_THE_CAP_SECONDS))
            .Reply(HttpStatusCode.OK, AnthropicResponseMother.Message(StringMother.Random("text")));

        await Writer(AiOptionsMother.Enabled(maxRetries: ONE_RETRY)).WriteAsync(AnInput());

        this._pauses.Should().Equal(AnthropicWeeklySummaryWriter.MaxRetryDelay);
    }

    [Test]
    public async Task not_wait_a_negative_time_if_the_service_asks_for_it()
    {
        this._handler
            .Reply(HttpStatusCode.TooManyRequests, string.Empty, TimeSpan.FromSeconds(-THREE_SECONDS))
            .Reply(HttpStatusCode.OK, AnthropicResponseMother.Message(StringMother.Random("text")));

        await Writer(AiOptionsMother.Enabled(maxRetries: ONE_RETRY)).WriteAsync(AnInput());

        this._pauses.Should().Equal(TimeSpan.Zero);
    }

    [TestCaseSource(nameof(UnusableAnswers))]
    public async Task fail_at_once_when_the_answer_is_not_a_summary(string body, string reason)
    {
        this._handler.Reply(HttpStatusCode.OK, body);

        Func<Task> act = () => Writer(AiOptionsMother.Enabled(maxRetries: TWO_RETRIES)).WriteAsync(AnInput());

        await act.Should().ThrowExactlyAsync<WeeklySummaryWriterException>().WithMessage($"*{reason}*");
        this._handler.Requests.Should().ContainSingle();
    }

    [TestCaseSource(nameof(FailuresOfTheService))]
    public async Task never_put_the_key_nor_the_data_of_the_class_in_a_failure(
        Action<FakeHttpMessageHandler, string, string> fail)
    {
        AiOptions options = AiOptionsMother.Enabled(timeoutSeconds: ONE_SECOND, maxRetries: NO_RETRIES);
        string secretTitle = StringMother.Random("title");
        WeeklySummaryInput input = WeeklySummaryInputMother.With(
            announcements: [WeeklySummaryInputMother.Announcement(secretTitle)]);
        fail(this._handler, options.ApiKey, secretTitle);

        Func<Task> act = () => Writer(options).WriteAsync(input).WaitAsync(Patience);

        string message = (await act.Should().ThrowExactlyAsync<WeeklySummaryWriterException>()).Which.Message;
        message.Should().NotContain(options.ApiKey).And.NotContain(secretTitle);
    }

    [Test]
    public void be_resolved_with_its_own_client_pointing_at_the_service_and_without_a_general_timeout()
    {
        ServiceCollection services = new();
        services.AddSingleton(Options.Create(AiOptionsMother.Enabled()));
        services.AddSumiClasWeeklySummaryWriter();
        using ServiceProvider provider = services.BuildServiceProvider();

        IWeeklySummaryWriter writer = provider.GetRequiredService<IWeeklySummaryWriter>();
        IHttpClientFactory factory = provider.GetRequiredService<IHttpClientFactory>();
        HttpClient client = factory.CreateClient(nameof(IWeeklySummaryWriter));

        writer.Should().BeOfType<AnthropicWeeklySummaryWriter>();
        client.BaseAddress.Should().Be(new Uri(AnthropicWeeklySummaryWriter.BaseAddress));
        client.Timeout.Should().Be(Timeout.InfiniteTimeSpan);
    }

    private static IEnumerable<TestCaseData> UnusableAnswers()
    {
        yield return new TestCaseData(
            AnthropicResponseMother.Message(StringMother.Random("text"), "refusal"), "refused").SetName("a_refusal");
        yield return new TestCaseData(
            AnthropicResponseMother.Message(StringMother.Random("text"), "max_tokens"), "cut").SetName("a_cut_text");
        yield return new TestCaseData(
            AnthropicResponseMother.Message("  \n"), "without text").SetName("a_blank_text");
        yield return new TestCaseData("not json", "not a message").SetName("something_that_is_not_json");
        yield return new TestCaseData("null", "not a message").SetName("a_json_null");
    }

    private static IEnumerable<TestCaseData> FailuresOfTheService()
    {
        yield return new TestCaseData((Action<FakeHttpMessageHandler, string, string>)((handler, key, title) =>
            handler.Reply(
                HttpStatusCode.Unauthorized,
                AnthropicResponseMother.Error("authentication_error", $"{key} {title}"))))
            .SetName("a_rejected_request");
        yield return new TestCaseData((Action<FakeHttpMessageHandler, string, string>)((handler, key, title) =>
            handler.Fail(new HttpRequestException($"{key} {title}")))).SetName("an_unreachable_service");
        yield return new TestCaseData((Action<FakeHttpMessageHandler, string, string>)((handler, _, _) =>
            handler.Hang())).SetName("no_answer_in_time");
        yield return new TestCaseData((Action<FakeHttpMessageHandler, string, string>)((handler, key, title) =>
            handler.Reply(
                HttpStatusCode.OK,
                AnthropicResponseMother.Message($"{key} {title}", "refusal")))).SetName("a_refusal");
        yield return new TestCaseData((Action<FakeHttpMessageHandler, string, string>)((handler, key, title) =>
            handler.Reply(HttpStatusCode.OK, $"{key} {title}"))).SetName("something_that_is_not_a_message");
    }

    private static WeeklySummaryInput AnInput()
        => WeeklySummaryInputMother.With(announcements: [WeeklySummaryInputMother.Announcement()]);

    private AnthropicWeeklySummaryWriter Writer(AiOptions? options = null)
        => new(
            new HttpClient(this._handler) { BaseAddress = new Uri(AnthropicWeeklySummaryWriter.BaseAddress) },
            Options.Create(options ?? AiOptionsMother.Enabled()),
            (pause, _) =>
            {
                this._pauses.Add(pause);
                return Task.CompletedTask;
            });
}
