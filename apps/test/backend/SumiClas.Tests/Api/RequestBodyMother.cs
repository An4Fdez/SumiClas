namespace SumiClas.Tests.Api;

using SumiClas.Api.Endpoints;
using SumiClas.Tests.Shared;
using System.Text.Json;

/// <summary>The JSON bodies of the requests the API validates, built from the real request records.</summary>
public static class RequestBodyMother
{
    public const int EXPENSE_ITEM_MAX = 200;
    public const decimal EXPENSE_AMOUNT_MIN = 0.01m;
    public const decimal EXPENSE_AMOUNT_MAX = 99999999.99m;
    public const int PASSWORD_MAX = 128;
    public const int EMAIL_MAX = 320;
    public const int PHOTO_URL_MAX = 2048;
    public const int STUDENT_NAME_MAX = 100;
    public const int USER_NAME_MAX = 200;

    public static string Expense(
        string? item = null,
        decimal? totalAmount = null,
        IReadOnlyCollection<Guid>? participantIds = null,
        bool withoutParticipants = false)
        => Json(new ClassExpenseCreateRequest(
            GuidMother.Random(),
            GuidMother.Random(),
            item ?? StringMother.Random("item"),
            totalAmount ?? EXPENSE_AMOUNT_MIN,
            withoutParticipants ? null! : participantIds ?? [GuidMother.Random()],
            null));

    public static string Registration(string? email = null, string? password = null)
        => Json(new AccountRegisterRequest(
            GuidMother.Random(),
            StringMother.Random("name"),
            email ?? "a@b.com",
            password ?? "12345678"));

    public static string Login(string? email = null, string? password = null)
        => Json(new AccountLoginRequest(email ?? "a@b.com", password ?? "12345678"));

    public static string LostItem(string? photoUrl = null)
        => Json(new LostItemCreateRequest(
            GuidMother.Random(),
            "Class",
            GuidMother.Random(),
            "Lost",
            StringMother.Random("description"),
            photoUrl,
            photoUrl is not null));

    public static string Student(string? firstName = null)
        => Json(new StudentRegisterRequest(
            GuidMother.Random(),
            firstName ?? StringMother.Random("first"),
            StringMother.Random("surname"),
            null,
            "Mother"));

    public static string Announcement(string scope)
        => Json(new AnnouncementCreateRequest(
            GuidMother.Random(),
            scope,
            GuidMother.Random(),
            StringMother.Random("title"),
            StringMother.Random("body"),
            "Normal",
            null));

    public static string UserName(string? fullName = null)
        => Json(new UserNameChangeRequest(fullName ?? StringMother.Random("name")));

    public static string PasswordChange(string? currentPassword = null, string? newPassword = null)
        => Json(new UserPasswordChangeRequest(
            currentPassword ?? StringMother.Random("current"),
            newPassword ?? StringMother.Random("new-password")));

    public static string StudentName(string? firstName = null, string? secondSurname = null)
        => Json(new StudentNameChangeRequest(
            firstName ?? StringMother.Random("first"),
            StringMother.Random("surname"),
            secondSurname));

    public static string OfLength(int length) => new('x', length);

    private static string Json<T>(T request) => JsonSerializer.Serialize(request, JsonSerializerOptions.Web);
}
