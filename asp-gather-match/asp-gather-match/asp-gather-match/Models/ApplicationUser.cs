using Microsoft.AspNetCore.Identity;

namespace asp_gather_match.Models
{
    public class ApplicationUser : IdentityUser<long>
    {
        public string DisplayName { get; set; } = string.Empty;
    }
}
