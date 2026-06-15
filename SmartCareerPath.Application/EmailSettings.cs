namespace SmartCareerPath.Application
{
    public class EmailSettings
    {
        public string SenderName { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string AppPassword { get; set; } = string.Empty;
        public string SmtpHost { get; set; } = "smtp.gmail.com";
        public int SmtpPort { get; set; } = 587;
        // Development: https://localhost:7xxx  |  Production: https://your-api.monsterapp.net
        public string BaseUrl { get; set; } = "https://smartcareerpath.runasp.net/";
    }
}
