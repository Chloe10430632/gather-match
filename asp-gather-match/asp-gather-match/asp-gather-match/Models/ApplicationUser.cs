using Microsoft.AspNetCore.Identity;

namespace asp_gather_match.Models
{
    public class ApplicationUser : IdentityUser<long>
    {
        public string DisplayName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public DateTimeOffset CreatedAt { get; set; }
            = DateTimeOffset.UtcNow;

        public DateTimeOffset UpdatedAt { get; set; }
            = DateTimeOffset.UtcNow;

        public ICollection<Activity> Activities { get; set; }
            = new List<Activity>();
    }
}
