namespace SumiClas.Subjects.Domain;

public interface ISubjectRepository
{
    Task AddAsync(Subject subject, CancellationToken ct = default);
}
