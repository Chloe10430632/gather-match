namespace asp_gather_match.Models
{
    public class DateOption
    {
        public long Id { get; set; }
        public long ActivityId { get; set; }
        public DateOnly OptionDate { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
        public short SortOrder { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public Activity Activity { get; set; } = null!;
    }
}
