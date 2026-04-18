namespace SmartCareerPath.Domain.Entites
{
    public class LookupType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;    //  "JobTitle"
        public ICollection<LookupValue> Values { get; set; } = [];
    }
}
