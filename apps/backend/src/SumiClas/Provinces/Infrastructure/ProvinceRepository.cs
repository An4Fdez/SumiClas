using SumiClas.Provinces.Domain;
using SumiClas.Shared.Infrastructure.Persistence;

namespace SumiClas.Provinces.Infrastructure;

public sealed class ProvinceRepository(SumiClasDbContext dbContext)
    : BaseRepository<Province>(dbContext), IProvinceRepository;
