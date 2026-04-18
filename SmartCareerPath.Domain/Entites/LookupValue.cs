namespace SmartCareerPath.Domain.Entites
{
    public class LookupValue
    {
        public int Id { get; set; }
        public int LookupTypeId { get; set; }
        public string Value { get; set; } = string.Empty;
        public LookupType LookupType { get; set; } = null!;
    }
}
