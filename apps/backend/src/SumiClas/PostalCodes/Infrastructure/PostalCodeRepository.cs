using SumiClas.PostalCodes.Domain;
using SumiClas.Shared.Infrastructure.Persistence;

namespace SumiClas.PostalCodes.Infrastructure;

public sealed class PostalCodeRepository(SumiClasDbContext dbContext)
    : BaseRepository<PostalCode>(dbContext), IPostalCodeRepository;
