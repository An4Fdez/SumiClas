namespace SumiClas.PostalCodes.Application;

public sealed record PostalCodeResponse(Guid Id, Guid LocalityId, string Code);
