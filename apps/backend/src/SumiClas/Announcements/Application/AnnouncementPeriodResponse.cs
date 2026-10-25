namespace SumiClas.Announcements.Application;

/// <summary>
/// An announcement as the summary of a period sees it: what it says and whether it is urgent, and nothing about who
/// wrote it or who has read it, so it is the same for every member of the class.
/// </summary>
/// <param name="Urgent">Whether the announcement has high priority.</param>
public sealed record AnnouncementPeriodResponse(string Title, bool Urgent);
