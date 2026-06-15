namespace SmartCareerPath.Application.DTOs.Notification
{
    public record NotificationDto(
    int Id,
    string Title,
    string Message,
    string Type,
    bool IsRead,
    int? RelatedEntityId,
    DateTime CreatedAt
);
}
