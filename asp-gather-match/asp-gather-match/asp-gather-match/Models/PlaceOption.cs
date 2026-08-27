namespace asp_gather_match.Models
{
    public class PlaceOption
    {
        public long Id { get; set; }
        public long ActivityId { get; set; }
        public string SourceType { get; set; } = "custom";
        public string DisplayLabel { get; set; } = string.Empty;
        public string? CustomAddress { get; set; }
        public string? GooglePlaceId { get; set; }
        public short SortOrder { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public Activity Activity { get; set; } = null!;
    }
}
