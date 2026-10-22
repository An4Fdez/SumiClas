using SumiClas.Localities.Application;
using SumiClas.PostalCodes.Application;
using SumiClas.Provinces.Application;
using SumiClas.Schools.Domain;
using SumiClas.Shared.Application;

namespace SumiClas.Schools.Application;

/// <summary>
/// Builds school responses with the territory names. The names belong to other modules, so they are read through
/// <see cref="IQueryBus"/>, once for the whole list: a constant number of queries however many schools there are.
/// </summary>
internal static class SchoolResponseFactory
{
    public static async Task<IReadOnlyCollection<SchoolResponse>> CreateAsync(
        IQueryBus queryBus,
        IReadOnlyCollection<School> schools,
        CancellationToken ct)
    {
        if (schools.Count == 0)
        {
            return [];
        }

        Dictionary<Guid, LocalityResponse> localities = (await queryBus.Send(
            new LocalityFinderByIds.Query(schools.Select(school => school.LocalityId).Distinct().ToList()), ct))
            .ToDictionary(locality => locality.Id);

        Dictionary<Guid, ProvinceResponse> provinces = (await queryBus.Send(
            new ProvinceFinderByIds.Query(localities.Values.Select(locality => locality.ProvinceId).Distinct().ToList()), ct))
            .ToDictionary(province => province.Id);

        List<Guid> postalCodeIds = schools
            .Where(school => school.PostalCodeId is not null)
            .Select(school => school.PostalCodeId!.Value)
            .Distinct()
            .ToList();
        Dictionary<Guid, PostalCodeResponse> postalCodes = postalCodeIds.Count == 0
            ? new Dictionary<Guid, PostalCodeResponse>()
            : (await queryBus.Send(new PostalCodeFinderByIds.Query(postalCodeIds), ct))
                .ToDictionary(postalCode => postalCode.Id);

        return schools.Select(school =>
        {
            if (!localities.TryGetValue(school.LocalityId, out LocalityResponse? locality)
                || !provinces.TryGetValue(locality.ProvinceId, out ProvinceResponse? province))
            {
                throw new InvalidOperationException($"School {school.Id} points to a locality or province that does not exist.");
            }

            PostalCodeResponse? postalCode = school.PostalCodeId is { } postalCodeId ? postalCodes.GetValueOrDefault(postalCodeId) : null;

            return new SchoolResponse(
                school.Id,
                school.Name,
                school.OfficialCode,
                province.Id,
                province.Name,
                locality.Id,
                locality.Name,
                school.PostalCodeId,
                postalCode?.Code,
                school.Address,
                school.Phone);
        }).ToList();
    }
}
