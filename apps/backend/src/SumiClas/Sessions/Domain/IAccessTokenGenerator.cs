using SumiClas.Shared.Domain;

namespace SumiClas.Sessions.Domain;

public interface IAccessTokenGenerator
{
    Result<AccessToken> Generate(Guid userId);
}
