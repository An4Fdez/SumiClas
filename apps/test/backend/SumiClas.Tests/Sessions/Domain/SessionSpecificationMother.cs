namespace SumiClas.Tests.Sessions.Domain;

using SumiClas.Sessions.Domain;

public static class SessionSpecificationMother
{
    public static SessionSpecification Create(Guid? id, Guid? userId, string? tokenHash)
        => new(id, userId, tokenHash);

    public static SessionSpecification Unfiltered() => Create(null, null, null);

    public static SessionSpecification With(Guid? id = null, Guid? userId = null, string? tokenHash = null)
        => Create(id, userId, tokenHash);

    public static SessionSpecification WithId(Guid id) => With(id: id);

    public static SessionSpecification WithUserId(Guid userId) => With(userId: userId);

    public static SessionSpecification WithTokenHash(string tokenHash) => With(tokenHash: tokenHash);
}
