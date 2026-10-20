namespace SumiClas.Tests.Api;

using Microsoft.AspNetCore.Http;
using SumiClas.Api.Http;

public static class EndpointMother
{
    public static Endpoint Declaring(params DomainExceptionStatus[] statuses)
        => new(null, new EndpointMetadataCollection(statuses), "sample endpoint");

    public static Endpoint Declaring<TException>(int statusCode)
        => Declaring(new DomainExceptionStatus(typeof(TException), statusCode));
}
