using System.Security.Cryptography;
using System.Text;

namespace SumiClas.WeeklySummaries.Application;

/// <summary>
/// The fingerprint of what the model is asked: a SHA-256 in hexadecimal of the whole prompt. Two weeks with the same
/// fingerprint would get the same text, so a stored summary written from it is still right; any change in the data
/// that reaches the model, or in the instructions it is given, changes the fingerprint.
/// </summary>
public static class WeeklySummaryFingerprint
{
    /// <summary>The length of the instructions goes first, so no text can move the point where they end.</summary>
    public static string Of(WeeklySummaryPrompt prompt)
        => Convert.ToHexStringLower(
            SHA256.HashData(Encoding.UTF8.GetBytes($"{prompt.System.Length}:{prompt.System}{prompt.User}")));
}
