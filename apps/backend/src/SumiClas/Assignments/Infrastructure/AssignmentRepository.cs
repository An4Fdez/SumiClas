using SumiClas.Assignments.Domain;
using SumiClas.Shared.Infrastructure.Persistence;

namespace SumiClas.Assignments.Infrastructure;

public sealed class AssignmentRepository(SumiClasDbContext dbContext)
    : BaseRepository<Assignment>(dbContext), IAssignmentRepository;
