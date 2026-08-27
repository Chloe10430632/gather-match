using asp_gather_match.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace asp_gather_match.Data
{
    public class ApplicationDbContext
        : IdentityUserContext<ApplicationUser, long>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(user => user.DisplayName)
                    .HasMaxLength(50)
                    .IsRequired();
            });
        }
    }
}
