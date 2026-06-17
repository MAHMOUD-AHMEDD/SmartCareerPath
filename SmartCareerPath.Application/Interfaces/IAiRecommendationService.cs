namespace SmartCareerPath.Application.Interfaces
{
    public interface IAiRecommendationService
    {
        Task CallAndSaveAsync(string seekerId);
    }
}
