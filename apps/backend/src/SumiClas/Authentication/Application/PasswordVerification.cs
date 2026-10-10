namespace SumiClas.Authentication.Application;

public enum PasswordVerification
{
    Failed,
    Success,

    /// <summary>The password is right but the stored hash uses older parameters: store a new hash.</summary>
    SuccessRehashNeeded,
}
