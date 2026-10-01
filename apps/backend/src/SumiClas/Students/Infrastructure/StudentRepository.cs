using SumiClas.Shared.Infrastructure.Persistence;
using SumiClas.Students.Domain;

namespace SumiClas.Students.Infrastructure;

public sealed class StudentRepository(SumiClasDbContext dbContext)
    : BaseRepository<Student>(dbContext), IStudentRepository;
