using SumiClas.Schools.Domain;
using SumiClas.Shared.Infrastructure.Persistence;

namespace SumiClas.Schools.Infrastructure;

public sealed class SchoolRepository(SumiClasDbContext dbContext)
    : BaseRepository<School>(dbContext), ISchoolRepository;
