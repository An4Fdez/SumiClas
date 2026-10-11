using SumiClas.Shared.Domain;

namespace SumiClas.RefreshTokens.Domain;

public interface IAccessTokenGenerator
{
    Result<AccessToken> Generate(Guid userId);
}
