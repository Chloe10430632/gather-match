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

        public DbSet<Activity> Activities => Set<Activity>();
        public DbSet<ActivityType> ActivityTypes => Set<ActivityType>();
        public DbSet<City> Cities => Set<City>();
        public DbSet<District> Districts => Set<District>();
        public DbSet<DateOption> DateOptions => Set<DateOption>();
        public DbSet<PlaceOption> PlaceOptions => Set<PlaceOption>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(user => user.DisplayName)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(user => user.IsActive)
                    .HasDefaultValue(true);

                entity.Property(user => user.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(user => user.UpdatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            modelBuilder.Entity<ActivityType>(entity =>
            {
                entity.Property(type => type.Code).HasMaxLength(30).IsRequired();
                entity.Property(type => type.Name).HasMaxLength(50).IsRequired();
                entity.Property(type => type.IsActive).HasDefaultValue(true);
                entity.Property(type => type.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(type => type.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasIndex(type => type.Code).IsUnique();
                entity.HasIndex(type => type.Name).IsUnique();
            });

            modelBuilder.Entity<City>(entity =>
            {
                entity.Property(city => city.GovernmentCode).HasMaxLength(10).IsRequired();
                entity.Property(city => city.Name).HasMaxLength(20).IsRequired();
                entity.Property(city => city.IsActive).HasDefaultValue(true);
                entity.HasIndex(city => city.GovernmentCode).IsUnique();
                entity.HasIndex(city => city.Name).IsUnique();
            });

            modelBuilder.Entity<District>(entity =>
            {
                entity.Property(district => district.GovernmentCode).HasMaxLength(10).IsRequired();
                entity.Property(district => district.Name).HasMaxLength(20).IsRequired();
                entity.Property(district => district.IsActive).HasDefaultValue(true);
                entity.HasIndex(district => district.GovernmentCode).IsUnique();
                entity.HasIndex(district => new { district.CityId, district.Name }).IsUnique();
                entity.HasOne(district => district.City)
                    .WithMany(city => city.Districts)
                    .HasForeignKey(district => district.CityId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Activity>(entity =>
            {
                entity.ToTable(table =>
                {
                    table.HasCheckConstraint(
                        "CK_Activities_BudgetMin",
                        "\"BudgetMin\" IS NULL OR \"BudgetMin\" >= 0");
                    table.HasCheckConstraint(
                        "CK_Activities_BudgetMax",
                        "\"BudgetMax\" IS NULL OR \"BudgetMax\" >= 0");
                    table.HasCheckConstraint(
                        "CK_Activities_BudgetRange",
                        "\"BudgetMin\" IS NULL OR \"BudgetMax\" IS NULL OR \"BudgetMax\" >= \"BudgetMin\"");
                    table.HasCheckConstraint(
                        "CK_Activities_Status",
                        "\"Status\" IN ('draft', 'open', 'closed', 'finalized', 'cancelled')");
                });
                entity.Property(activity => activity.Title).HasMaxLength(100).IsRequired();
                entity.Property(activity => activity.BudgetMin).HasPrecision(10, 2);
                entity.Property(activity => activity.BudgetMax).HasPrecision(10, 2);
                entity.Property(activity => activity.CurrencyCode).HasMaxLength(3).IsFixedLength().HasDefaultValue("TWD");
                entity.Property(activity => activity.Status).HasMaxLength(20).HasDefaultValue("draft");
                entity.Property(activity => activity.ShareTokenHash).HasMaxLength(128).IsRequired();
                entity.Property(activity => activity.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(activity => activity.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasIndex(activity => activity.ShareTokenHash).IsUnique();
                entity.HasOne(activity => activity.HostUser)
                    .WithMany(user => user.Activities)
                    .HasForeignKey(activity => activity.HostUserId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(activity => activity.ActivityType)
                    .WithMany(type => type.Activities)
                    .HasForeignKey(activity => activity.ActivityTypeId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(activity => activity.City)
                    .WithMany(city => city.Activities)
                    .HasForeignKey(activity => activity.CityId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(activity => activity.District)
                    .WithMany(district => district.Activities)
                    .HasForeignKey(activity => activity.DistrictId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<DateOption>(entity =>
            {
                entity.ToTable(table => table.HasCheckConstraint(
                    "CK_DateOptions_TimeRange",
                    "\"EndTime\" IS NULL OR \"StartTime\" IS NULL OR \"EndTime\" > \"StartTime\""));
                entity.Property(option => option.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasIndex(option => new { option.ActivityId, option.OptionDate, option.StartTime }).IsUnique();
                entity.HasOne(option => option.Activity)
                    .WithMany(activity => activity.DateOptions)
                    .HasForeignKey(option => option.ActivityId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PlaceOption>(entity =>
            {
                entity.ToTable(table => table.HasCheckConstraint(
                    "CK_PlaceOptions_Source",
                    "(\"SourceType\" = 'custom' AND \"GooglePlaceId\" IS NULL) OR " +
                    "(\"SourceType\" = 'google' AND \"GooglePlaceId\" IS NOT NULL)"));
                entity.Property(option => option.SourceType).HasMaxLength(20).IsRequired();
                entity.Property(option => option.DisplayLabel).HasMaxLength(200).IsRequired();
                entity.Property(option => option.GooglePlaceId).HasMaxLength(255);
                entity.Property(option => option.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasIndex(option => new { option.ActivityId, option.GooglePlaceId }).IsUnique();
                entity.HasOne(option => option.Activity)
                    .WithMany(activity => activity.PlaceOptions)
                    .HasForeignKey(option => option.ActivityId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
