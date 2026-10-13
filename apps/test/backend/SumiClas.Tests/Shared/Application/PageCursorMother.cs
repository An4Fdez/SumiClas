namespace SumiClas.Tests.Shared.Application;

using SumiClas.Shared.Application;

public static class PageCursorMother
{
    public static PageCursor Create(long key, Guid id) => new(key, id);
}
