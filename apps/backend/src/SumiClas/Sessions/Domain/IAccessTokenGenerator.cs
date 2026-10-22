namespace SumiClas.Sessions.Domain;

public interface IAccessTokenGenerator
{
    AccessToken Generate(Guid userId);
}
