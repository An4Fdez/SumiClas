using MediatR;
using SumiClas.Classes.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Subjects.Application;
using SumiClas.Users.Application;
using SumiClas.Assignments.Domain;

namespace SumiClas.Assignments.Application;

public static class AssignmentFinderByClass
{
    /// <summary>The homework of the class with its subject and its comments, ordered by due date.</summary>
    public sealed record Query(Guid ClassId) : IQuery<Result<IReadOnlyCollection<AssignmentResponse>>>;

    public sealed class Handler(IAssignmentRepository assignmentRepository, IQueryBus queryBus)
        : IRequestHandler<Query, Result<IReadOnlyCollection<AssignmentResponse>>>
    {
        public async Task<Result<IReadOnlyCollection<AssignmentResponse>>> Handle(Query query, CancellationToken ct)
        {
            var schoolClass = await queryBus.Send(new ClassFinderById.Query(query.ClassId), ct);
            if (schoolClass.IsFailure)
            {
                return Result<IReadOnlyCollection<AssignmentResponse>>.Failure(schoolClass.Error!);
            }

            var assignments = await assignmentRepository.SearchAsync(new AssignmentSpecification(classId: query.ClassId), ct);

            var subjects = await queryBus.Send(new SubjectFinderByClass.Query(query.ClassId), ct);
            if (subjects.IsFailure)
            {
                return Result<IReadOnlyCollection<AssignmentResponse>>.Failure(subjects.Error!);
            }
            var subjectLabels = subjects.Value.ToDictionary(subject => subject.Id, subject => subject.Label);

            var authorIds = assignments
                .Select(assignment => assignment.AuthorId)
                .Concat(assignments.SelectMany(assignment => assignment.Comments.Select(comment => comment.AuthorId)))
                .Distinct()
                .ToList();
            var authors = (await queryBus.Send(new UserClassMemberFinderByIds.Query(query.ClassId, authorIds), ct))
                .ToDictionary(author => author.UserId);

            IReadOnlyCollection<AssignmentResponse> response = assignments
                .OrderBy(assignment => assignment.DueDate)
                .ThenBy(assignment => assignment.CreatedAt)
                .Select(assignment => new AssignmentResponse(
                    assignment.Id,
                    assignment.ClassId,
                    assignment.SubjectId,
                    subjectLabels.GetValueOrDefault(assignment.SubjectId) ?? string.Empty,
                    assignment.Title,
                    assignment.Description,
                    assignment.DueDate,
                    assignment.AuthorId,
                    authors.GetValueOrDefault(assignment.AuthorId)?.FullName ?? string.Empty,
                    authors.GetValueOrDefault(assignment.AuthorId)?.ClassRole,
                    assignment.IsProposal,
                    assignment.CreatedAt,
                    assignment.Comments
                        .OrderBy(comment => comment.CreatedAt)
                        .Select(comment => new AssignmentCommentResponse(
                            comment.Id,
                            comment.AuthorId,
                            authors.GetValueOrDefault(comment.AuthorId)?.FullName ?? string.Empty,
                            authors.GetValueOrDefault(comment.AuthorId)?.ClassRole,
                            comment.Text,
                            comment.CreatedAt))
                        .ToList()))
                .ToList();

            return Result<IReadOnlyCollection<AssignmentResponse>>.Success(response);
        }
    }
}
