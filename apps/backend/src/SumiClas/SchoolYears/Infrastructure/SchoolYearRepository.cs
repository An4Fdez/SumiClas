using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Infrastructure.Persistence;

namespace SumiClas.SchoolYears.Infrastructure;

public sealed class SchoolYearRepository(SumiClasDbContext dbContext)
    : BaseRepository<SchoolYear>(dbContext), ISchoolYearRepository;
