using SumiClas.Shared.Domain;

namespace SumiClas.LostItems.Domain;

public sealed class LostItemSpecification : Specification<LostItem>
{
    public LostItemSpecification(
        Guid? id = null,
        ItemScope? scope = null,
        Guid? classId = null,
        Guid? schoolId = null,
        ItemStatus? status = null,
        bool? recovered = null,
        string? description = null,
        string? photoUrl = null,
        bool? photoPrivacyConfirmed = null,
        Guid? authorId = null,
        DateTimeOffset? createdAt = null,
        KeysetPage<DateTimeOffset>? page = null)
    {
        if (id is { } itemId) AddCriteria(item => item.Id == itemId);
        if (scope is { } itemScope) AddCriteria(item => item.Scope == itemScope);
        if (classId is { } schoolClass) AddCriteria(item => item.ClassId == schoolClass);
        if (schoolId is { } school) AddCriteria(item => item.SchoolId == school);
        if (status is { } itemStatus) AddCriteria(item => item.Status == itemStatus);
        if (recovered is { } isRecovered) AddCriteria(item => item.Recovered == isRecovered);
        if (description is not null) AddCriteria(item => item.Description == description);
        if (photoUrl is not null) AddCriteria(item => item.PhotoUrl == photoUrl);
        if (photoPrivacyConfirmed is { } privacyConfirmed) AddCriteria(item => item.PhotoPrivacyConfirmed == privacyConfirmed);
        if (authorId is { } author) AddCriteria(item => item.AuthorId == author);
        if (createdAt is { } created) AddCriteria(item => item.CreatedAt == created);
        if (page is not null) AddKeysetPage(item => item.CreatedAt, item => item.Id, page, descending: true);
    }
}
