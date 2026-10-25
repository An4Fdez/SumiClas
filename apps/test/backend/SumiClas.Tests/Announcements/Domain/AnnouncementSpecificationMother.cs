namespace SumiClas.Tests.Announcements.Domain;

using SumiClas.Announcements.Domain;
using SumiClas.Shared.Domain;

public static class AnnouncementSpecificationMother
{
    public static AnnouncementSpecification ForClass(Guid classId)
        => new(scope: AnnouncementScope.Class, classId: classId);

    public static AnnouncementSpecification ForSchool(Guid schoolId)
        => new(scope: AnnouncementScope.School, schoolId: schoolId);

    public static AnnouncementSpecification ReadBy(Guid userId) => new(readByUserId: userId);

    public static AnnouncementSpecification UnreadBy(Guid userId) => new(unreadByUserId: userId);

    public static AnnouncementSpecification CreatedFrom(DateTimeOffset moment) => new(createdFrom: moment);

    public static AnnouncementSpecification CreatedBefore(DateTimeOffset moment) => new(createdBefore: moment);

    public static AnnouncementSpecification OfPage(KeysetPage<DateTimeOffset> page) => new(page: page);
}
