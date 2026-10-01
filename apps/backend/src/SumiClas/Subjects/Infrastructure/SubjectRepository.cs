using SumiClas.Shared.Infrastructure.Persistence;
using SumiClas.Subjects.Domain;

namespace SumiClas.Subjects.Infrastructure;

public sealed class SubjectRepository(SumiClasDbContext dbContext)
    : BaseRepository<Subject>(dbContext), ISubjectRepository;
