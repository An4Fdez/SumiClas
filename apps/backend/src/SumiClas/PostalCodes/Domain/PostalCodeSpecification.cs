using SumiClas.Shared.Domain;

namespace SumiClas.PostalCodes.Domain;

public sealed class PostalCodeSpecification : Specification<PostalCode>
{
    public PostalCodeSpecification(
        Guid? id = null,
        Guid? localityId = null,
        string? code = null,
        IReadOnlyCollection<Guid>? ids = null)
    {
        if (id is { } postalCodeId) AddCriteria(postalCode => postalCode.Id == postalCodeId);
        if (localityId is { } locality) AddCriteria(postalCode => postalCode.LocalityId == locality);
        if (code is not null) AddCriteria(postalCode => postalCode.Code == code);
        if (ids is not null)
        {
            Guid[] postalCodeIds = ids.ToArray();
            AddCriteria(postalCode => postalCodeIds.Contains(postalCode.Id));
        }
    }
}
