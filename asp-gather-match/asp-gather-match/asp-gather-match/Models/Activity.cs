namespace asp_gather_match.Models
{
    public class Activity
    {
        public long Id { get; set; }
        public long HostUserId { get; set; }
        public short ActivityTypeId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal? BudgetMin { get; set; }
        public decimal? BudgetMax { get; set; }
        public string CurrencyCode { get; set; } = "TWD";
        public short CityId { get; set; }
        public int? DistrictId { get; set; }
        public DateTimeOffset DeadlineAt { get; set; }
        public string Status { get; set; } = "draft";
        public string ShareTokenHash { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? FinalizedAt { get; set; }

        public ApplicationUser HostUser { get; set; } = null!;
        public ActivityType ActivityType { get; set; } = null!;
        public City City { get; set; } = null!;
        public District? District { get; set; }
        public ICollection<DateOption> DateOptions { get; set; }
            = new List<DateOption>();
        public ICollection<PlaceOption> PlaceOptions { get; set; }
            = new List<PlaceOption>();
    }
}
