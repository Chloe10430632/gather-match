using asp_gather_match.Data;
using asp_gather_match.Models;
using asp_gather_match.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace GatherMatch.Tests;

// SQLite 只供隔離的關聯式測試；實際 PostgreSQL 另以 Test-HostFlow.ps1 驗證。
public class ActivityRepositoryTests
{
    private sealed class FailSecondSave : SaveChangesInterceptor
    {
        public bool Enabled { get; set; }
        private int saveCount;

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (Enabled && ++saveCount == 2) throw new InvalidOperationException("Simulated final write failure");
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }

    private static async Task SeedAsync(ApplicationDbContext db)
    {
        await db.Database.EnsureCreatedAsync();
        db.Users.Add(new ApplicationUser { Id = 7, UserName = "test@example.invalid", DisplayName = "TEST host" });
        db.Cities.Add(new City { Id = 1, GovernmentCode = "00001", Name = "TEST city" });
        db.Activities.Add(ActivityUpdateTests.SampleActivity());
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Save_DateSwap_IsAtomicAndPreservesIds(bool failFinalWrite)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var interceptor = new FailSecondSave();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection).AddInterceptors(interceptor).Options;
        await using var db = new ApplicationDbContext(options);
        await SeedAsync(db);
        var repository = new ActivityRepository(db);
        var activity = (await repository.GetOwnedAsync(42, 7))!;
        var dates = activity.DateOptions.OrderBy(x => x.Id).ToArray();
        (dates[0].OptionDate, dates[1].OptionDate) = (dates[1].OptionDate, dates[0].OptionDate);
        activity.Title = "Changed";
        interceptor.Enabled = failFinalWrite;

        if (failFinalWrite)
            await Assert.ThrowsAsync<InvalidOperationException>(() => repository.SaveChangesAsync());
        else
            await repository.SaveChangesAsync();

        // 重新讀資料庫，不能只檢查記憶體內 entity。
        await using var verificationDb = new ApplicationDbContext(options);
        var saved = (await new ActivityRepository(verificationDb).GetOwnedAsync(42, 7))!;
        var savedDates = saved.DateOptions.OrderBy(x => x.Id).ToArray();
        Assert.Equal(failFinalWrite ? "Original" : "Changed", saved.Title);
        Assert.Equal(new long[] { 11, 12 }, savedDates.Select(x => x.Id));
        Assert.Equal(new DateOnly(2026, 10, failFinalWrite ? 1 : 2), savedDates[0].OptionDate);
        Assert.Equal(new DateOnly(2026, 10, failFinalWrite ? 2 : 1), savedDates[1].OptionDate);
        Assert.Single(saved.PlaceOptions);
    }

    [Fact]
    public async Task GetOwned_FiltersOwnerAndIncludesOptions()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options;
        await using var db = new ApplicationDbContext(options);
        await SeedAsync(db);
        var repository = new ActivityRepository(db);
        Assert.Null(await repository.GetOwnedAsync(42, 8));
        Assert.Null(await repository.GetOwnedAsync(999, 7));
        var activity = await repository.GetOwnedAsync(42, 7);
        Assert.Equal(2, activity!.DateOptions.Count);
        Assert.Single(activity.PlaceOptions);
    }
}
