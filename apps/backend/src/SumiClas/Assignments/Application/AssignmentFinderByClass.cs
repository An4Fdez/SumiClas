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
    /// <summary>The homework of the class with its subject and its comments, ordered by due date, one page at a time.</summary>
    public sealed record Query(Guid ClassId, int? Limit = null, string? Cursor = null) : IQuery<Result<PagedResponse<AssignmentResponse>>>;

    public sealed class Handler(IAssignmentRepository assignmentRepository, IQueryBus queryBus)
        : IRequestHandler<Query, Result<PagedResponse<AssignmentResponse>>>
    {
        public async Task<Result<PagedResponse<AssignmentResponse>>> Handle(Query query, CancellationToken ct)
        {
            var pageRequest = Pagination.Resolve(query.Limit, query.Cursor);
            if (pageRequest.IsFailure)
            {
                return Result<PagedResponse<AssignmentResponse>>.Failure(pageRequest.Error!);
            }

            var page = Pagination.ByDate(pageRequest.Value);
            if (page.IsFailure)
            {
                return Result<PagedResponse<AssignmentResponse>>.Failure(page.Error!);
            }

            var schoolClass = await queryBus.Send(new ClassFinderById.Query(query.ClassId), ct);
            if (schoolClass.IsFailure)
            {
                return Result<PagedResponse<AssignmentResponse>>.Failure(schoolClass.Error!);
            }

            var candidates = await assignmentRepository.SearchAsync(new AssignmentSpecification(classId: query.ClassId, page: page.Value), ct);
            var (assignments, nextCursor) = Pagination.Cut(
                candidates.OrderBy(assignment => assignment.DueDate).ThenBy(assignment => assignment.Id, Pagination.IdComparer).ToList(),
                pageRequest.Value.Limit,
                assignment => Pagination.CursorOf(assignment.DueDate, assignment.Id));

            var subjects = await queryBus.Send(new SubjectFinderByClass.Query(query.ClassId), ct);
            if (subjects.IsFailure)
            {
                return Result<PagedResponse<AssignmentResponse>>.Failure(subjects.Error!);
            }
            var subjectLabels = subjects.Value.ToDictionary(subject => subject.Id, subject => subject.Label);

            var authorIds = assignments
                .Select(assignment => assignment.AuthorId)
                .Concat(assignments.SelectMany(assignment => assignment.Comments.Select(comment => comment.AuthorId)))
                .Distinct()
                .ToList();
            var authors = (await queryBus.Send(new UserClassMemberFinderByIds.Query(query.ClassId, authorIds), ct))
                .ToDictionary(author => author.UserId);

            IReadOnlyCollection<AssignmentResponse> items = assignments
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

            return Result<PagedResponse<AssignmentResponse>>.Success(new PagedResponse<AssignmentResponse>(items, nextCursor));
        }
    }
}
