namespace SumiClas.Sessions.Domain;

public interface IRefreshTokenGenerator
{
    /// <summary>The raw value goes to the client cookie; only the hash is ever stored.</summary>
    string  Generate();

    string Hash(string refreshToken);
}
