namespace SumiClas.Authentication.Application;

public interface IAccessTokenIssuer
{
    AccessToken Issue(Guid userId);
}
