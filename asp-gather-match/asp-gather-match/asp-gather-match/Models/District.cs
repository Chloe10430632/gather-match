namespace asp_gather_match.Models
{
    public class District
    {
        public int Id { get; set; }
        public short CityId { get; set; }
        public string GovernmentCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public short SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTimeOffset? SourceUpdatedAt { get; set; }

        public City City { get; set; } = null!;
        public ICollection<Activity> Activities { get; set; }
            = new List<Activity>();
    }
}
