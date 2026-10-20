namespace SumiClas.Api.Http;

/// <summary>Endpoint metadata: the HTTP status the endpoint answers when this domain exception comes out of its use case.</summary>
public sealed record DomainExceptionStatus(Type ExceptionType, int StatusCode);
