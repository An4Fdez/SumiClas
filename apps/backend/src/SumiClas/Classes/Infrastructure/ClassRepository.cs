using SumiClas.Classes.Domain;
using SumiClas.Shared.Infrastructure.Persistence;

namespace SumiClas.Classes.Infrastructure;

public sealed class ClassRepository(SumiClasDbContext dbContext)
    : BaseRepository<Class>(dbContext), IClassRepository;
