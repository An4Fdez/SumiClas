using SumiClas.Shared.Infrastructure.Persistence;
using SumiClas.WeeklySummaries.Domain;

namespace SumiClas.WeeklySummaries.Infrastructure;

public sealed class WeeklySummaryRepository(SumiClasDbContext dbContext)
    : BaseRepository<WeeklySummary>(dbContext), IWeeklySummaryRepository;
