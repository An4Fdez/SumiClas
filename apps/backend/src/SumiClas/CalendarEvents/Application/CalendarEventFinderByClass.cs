using MediatR;
using SumiClas.CalendarEvents.Domain;
using SumiClas.Classes.Application;
using SumiClas.Enrollments.Application;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;

namespace SumiClas.CalendarEvents.Application;

public static class CalendarEventFinderByClass
{
    /// <summary>
    /// School-wide events, the events of the class, and the birthdays of the students currently enrolled in it
    /// (resolved through their confirmed enrollments, because a birthday follows the student and not the class).
    /// </summary>
    public sealed record Query(Guid ClassId) : IQuery<Result<IReadOnlyCollection<CalendarEventResponse>>>;

    public sealed class Finder(
        ICalendarEventRepository repository,
        IQueryBus queryBus,
        IUserProvider userProvider) : IUseCase
    {
        public async Task<Result<IReadOnlyCollection<CalendarEventResponse>>> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            var schoolClass = await queryBus.Send(new ClassFinderById.Query(query.ClassId), ct);

            var current = userProvider.GetCurrentUserId();
            if (current.IsFailure)
            {
                return Result<IReadOnlyCollection<CalendarEventResponse>>.Failure(current.Error!);
            }

            var access = await queryBus.Send(new UserClassAccessEnsure.Query(current.Value, query.ClassId), ct);
            if (access.IsFailure)
            {
                return Result<IReadOnlyCollection<CalendarEventResponse>>.Failure(access.Error!);
            }

            var school = await queryBus.Send(new SchoolIdFinderBySchoolYear.Query(schoolClass.SchoolYearId), ct);

            var enrollments = await queryBus.Send(new EnrollmentFinderByClass.Query(query.ClassId), ct);
            var studentIds = enrollments
                .Where(enrollment => enrollment.Status == "Confirmed")
                .Select(enrollment => enrollment.StudentId)
                .Distinct()
                .ToList();

            var events = (await repository.SearchAsync(new CalendarEventSpecification(scope: EventScope.School, schoolId: school), ct)).ToList();
            events.AddRange(await repository.SearchAsync(new CalendarEventSpecification(scope: EventScope.Class, classId: query.ClassId), ct));
            if (studentIds.Count > 0)
            {
                events.AddRange(await repository.SearchAsync(new CalendarEventSpecification(studentIds: studentIds), ct));
            }

            var authorIds = events.Select(calendarEvent => calendarEvent.AuthorId).Distinct().ToList();
            var authors = (await queryBus.Send(new UserClassMemberFinderByIds.Query(query.ClassId, authorIds), ct))
                .ToDictionary(author => author.UserId);

            IReadOnlyCollection<CalendarEventResponse> response = events
                .OrderBy(calendarEvent => calendarEvent.EventDate.Year ?? 0)
                .ThenBy(calendarEvent => calendarEvent.EventDate.Month)
                .ThenBy(calendarEvent => calendarEvent.EventDate.Day)
                .Select(calendarEvent => new CalendarEventResponse(
                    calendarEvent.Id,
                    calendarEvent.Scope.ToString(),
                    calendarEvent.ClassId,
                    calendarEvent.StudentId,
                    calendarEvent.Type.ToString(),
                    calendarEvent.Title,
                    calendarEvent.EventDate.Month,
                    calendarEvent.EventDate.Day,
                    calendarEvent.EventDate.Year,
                    calendarEvent.Time,
                    calendarEvent.Location,
                    calendarEvent.Cost,
                    calendarEvent.AuthorId,
                    authors.GetValueOrDefault(calendarEvent.AuthorId)?.FullName ?? string.Empty,
                    authors.GetValueOrDefault(calendarEvent.AuthorId)?.ClassRole,
                    calendarEvent.CreatedAt))
                .ToList();

            return Result<IReadOnlyCollection<CalendarEventResponse>>.Success(response);
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, Result<IReadOnlyCollection<CalendarEventResponse>>>
    {
        public Task<Result<IReadOnlyCollection<CalendarEventResponse>>> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
