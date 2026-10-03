using System.Text;
using SumiClas.Shared.Application;

namespace SumiClas.Tests.Shared.Application;

[TestFixture]
public class PaginationTests
{
    private static readonly Guid SomeId = Guid.Parse("3f2b1c7e-8d4a-4c1e-9b5f-0a6d2e7c9b10");

    [Test]
    public void Cursor_EncodedThenDecoded_ReturnsTheSameKeyAndId()
    {
        var encoded = new PageCursor(638_000_000_000_000_000, SomeId).Encode();

        var decoded = PageCursor.TryDecode(encoded, out var cursor);

        Assert.That(decoded, Is.True);
        Assert.That(cursor, Is.EqualTo(new PageCursor(638_000_000_000_000_000, SomeId)));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("not a cursor!")]
    [TestCase("bm9wZQ")]
    public void Cursor_WithGarbage_DoesNotDecode(string? value)
    {
        Assert.That(PageCursor.TryDecode(value, out _), Is.False);
    }

    [Test]
    public void Cursor_WithNonNumericKeyOrBadId_DoesNotDecode()
    {
        var badKey = Convert.ToBase64String(Encoding.UTF8.GetBytes($"abc|{SomeId}")).TrimEnd('=');
        var badId = Convert.ToBase64String(Encoding.UTF8.GetBytes("123|xyz")).TrimEnd('=');

        Assert.That(PageCursor.TryDecode(badKey, out _), Is.False);
        Assert.That(PageCursor.TryDecode(badId, out _), Is.False);
    }

    [Test]
    public void Resolve_WithoutParameters_UsesDefaultLimitAndFirstPage()
    {
        var result = Pagination.Resolve(null, null);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Limit, Is.EqualTo(Pagination.DefaultLimit));
        Assert.That(result.Value.After, Is.Null);
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(51)]
    public void Resolve_WithLimitOutOfRange_ReturnsInvalidLimit(int limit)
    {
        var result = Pagination.Resolve(limit, null);

        Assert.That(result.Error!.Code, Is.EqualTo("Pagination.InvalidLimit"));
    }

    [Test]
    public void Resolve_WithInvalidCursor_ReturnsInvalidCursor()
    {
        var result = Pagination.Resolve(10, "garbage!");

        Assert.That(result.Error!.Code, Is.EqualTo("Pagination.InvalidCursor"));
    }

    [Test]
    public void ByDate_WithKeyOutOfRange_ReturnsInvalidCursor()
    {
        var result = Pagination.ByDate(new PageRequest(10, new PageCursor(long.MaxValue, SomeId)));

        Assert.That(result.Error!.Code, Is.EqualTo("Pagination.InvalidCursor"));
    }

    [Test]
    public void ByCreatedAt_AsksOneExtraItemToKnowIfThereIsANextPage()
    {
        var result = Pagination.ByCreatedAt(new PageRequest(10, null));

        Assert.That(result.Value.Take, Is.EqualTo(11));
        Assert.That(result.Value.After, Is.Null);
    }

    [Test]
    public void Cut_WithMoreItemsThanLimit_ReturnsTheLimitAndACursorOfTheLastReturned()
    {
        var (items, next) = Pagination.Cut([1, 2, 3], 2, number => new PageCursor(number, SomeId));

        Assert.That(items, Is.EqualTo(new[] { 1, 2 }));
        PageCursor.TryDecode(next, out var cursor);
        Assert.That(cursor.Key, Is.EqualTo(2));
    }

    [Test]
    public void Cut_WithAsManyItemsAsLimit_HasNoNextCursor()
    {
        var (items, next) = Pagination.Cut([1, 2], 2, number => new PageCursor(number, SomeId));

        Assert.That(items, Has.Count.EqualTo(2));
        Assert.That(next, Is.Null);
    }

    [Test]
    public void CompareIds_FollowsTheTextualOrderOfTheGuid()
    {
        var low = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var high = Guid.Parse("ff000000-0000-0000-0000-000000000000");

        Assert.That(Pagination.CompareIds(low, high), Is.LessThan(0));
        Assert.That(Pagination.CompareIds(high, low), Is.GreaterThan(0));
        Assert.That(Pagination.CompareIds(low, low), Is.EqualTo(0));
    }
}
