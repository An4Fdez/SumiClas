namespace SumiClas.Tests.WeeklySummaries.Infrastructure;

using System.Text.Json;

/// <summary>The bodies the Messages API answers with, built the way the service writes them.</summary>
public static class AnthropicResponseMother
{
    public const string END_TURN = "end_turn";

    public static string Message(string text, string stopReason = END_TURN)
        => Blocks([new { type = "text", text }], stopReason);

    public static string Blocks(object[] content, string stopReason = END_TURN)
        => JsonSerializer.Serialize(new { type = "message", role = "assistant", content, stop_reason = stopReason });

    public static string Error(string errorType, string message = "ignored")
        => JsonSerializer.Serialize(new { type = "error", error = new { type = errorType, message } });
}
