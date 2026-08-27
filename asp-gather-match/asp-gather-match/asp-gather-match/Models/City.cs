namespace asp_gather_match.Models
{
    public class City
    {
        public short Id { get; set; }
        public string GovernmentCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public short SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTimeOffset? SourceUpdatedAt { get; set; }

        public ICollection<District> Districts { get; set; }
            = new List<District>();
        public ICollection<Activity> Activities { get; set; }
            = new List<Activity>();
    }
}
