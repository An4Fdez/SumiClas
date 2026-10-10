using SumiClas.Localities.Domain;
using SumiClas.Shared.Infrastructure.Persistence;

namespace SumiClas.Localities.Infrastructure;

public sealed class LocalityRepository(SumiClasDbContext dbContext)
    : BaseRepository<Locality>(dbContext), ILocalityRepository;
