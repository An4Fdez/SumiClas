namespace SumiClas.Users.Domain;

public interface IPasswordHasher
{
    string Generate(Password password);

    /// <summary>
    /// A <c>null</c> hash (no credential to check) is verified against a throwaway hash and always fails, so the
    /// time taken does not reveal whether the account exists.
    /// </summary>
    PasswordVerification Verify(string? hash, string password);
}
