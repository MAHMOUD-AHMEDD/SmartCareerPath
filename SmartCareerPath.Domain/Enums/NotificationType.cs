namespace SmartCareerPath.Domain.Enums
{
    public enum NotificationType
    {
        NewChatRequest,        // sent to Mentor when Seeker sends a request
        ChatRequestAccepted,   // sent to Seeker when Mentor accepts
        ChatRequestDeclined    // sent to Seeker when Mentor declines
    }
}
