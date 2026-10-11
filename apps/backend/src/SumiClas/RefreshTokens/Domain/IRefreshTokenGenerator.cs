namespace SumiClas.RefreshTokens.Domain;

public interface IRefreshTokenGenerator
{
    /// <summary>The raw value goes to the client cookie; only the hash is ever stored.</summary>
    (string Raw, string Hash) Generate();

    string Hash(string raw);
}
