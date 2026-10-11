using SumiClas.Shared.Domain;

namespace SumiClas.Authentication.Domain;

public sealed class RefreshTokenSpecification : Specification<RefreshToken>
{
    public RefreshTokenSpecification(string? tokenHash = null, Guid? familyId = null)
    {
        if (tokenHash is not null) AddCriteria(token => token.TokenHash == tokenHash);
        if (familyId is { } family) AddCriteria(token => token.FamilyId == family);
    }
}
