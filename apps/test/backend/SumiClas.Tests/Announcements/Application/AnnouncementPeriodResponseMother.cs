namespace SumiClas.Tests.Announcements.Application;

using SumiClas.Announcements.Application;
using SumiClas.Tests.Shared;

public static class AnnouncementPeriodResponseMother
{
    public static AnnouncementPeriodResponse With(string? title = null, bool urgent = false, string? body = null)
        => new(title ?? StringMother.Random("announcement"), urgent, body ?? StringMother.Random("body"));
}
