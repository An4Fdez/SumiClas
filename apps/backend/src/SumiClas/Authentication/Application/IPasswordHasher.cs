namespace SumiClas.Authentication.Application;

public interface IPasswordHasher
{
    string Hash(string password);

    /// <summary>
    /// A <c>null</c> hash (no credential to check) is verified against a throwaway hash and always fails, so the
    /// time taken does not reveal whether the account exists.
    /// </summary>
    PasswordVerification Verify(string? hash, string password);
}
