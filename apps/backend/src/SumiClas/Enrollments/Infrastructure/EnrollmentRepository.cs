using SumiClas.Enrollments.Domain;
using SumiClas.Shared.Infrastructure.Persistence;

namespace SumiClas.Enrollments.Infrastructure;

public sealed class EnrollmentRepository(SumiClasDbContext dbContext)
    : BaseRepository<Enrollment>(dbContext), IEnrollmentRepository;
