using System.Security.Cryptography;
using System.Text;
using asp_gather_match.Contracts.Activities;
using asp_gather_match.Models;
using asp_gather_match.Repositories;
using ActivityEntity = asp_gather_match.Models.Activity;

namespace asp_gather_match.Services;

public class ActivityService(IActivityRepository activityRepository) : IActivityService
{
    public async Task<ActivityResponse?> GetAsync(long hostUserId, long activityId)
    {
        var activity = await activityRepository.GetOwnedAsync(activityId, hostUserId);
        return activity is null ? null : ToResponse(activity);
    }

    public async Task<ActivityUpdateResult> UpdateAsync(
        long hostUserId, long activityId, UpdateActivityRequest request)
    {
        var activity = await activityRepository.GetOwnedAsync(activityId, hostUserId);
        if (activity is null)
            return ActivityUpdateResult.NotFound();

        var now = DateTimeOffset.UtcNow;
        if (activity.Status != "open" || activity.DeadlineAt <= now)
            return ActivityUpdateResult.NotEditable();

        if (request.Title is null && request.DateOptions is null)
            return ActivityUpdateResult.Invalid("request", "至少需要提供活動名稱或候選日期。");

        if (request.Title is not null && (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length > 100))
            return ActivityUpdateResult.Invalid("title", "活動名稱必須為 1 到 100 字，且不可只包含空白。");

        if (request.DateOptions is not null)
        {
            if (request.DateOptions.Count == 0 || request.DateOptions.Any(option => option is null))
                return ActivityUpdateResult.Invalid("dateOptions", "至少需要一個有效的候選日期修改項目。");

            if (request.DateOptions.Select(option => option.Id).Distinct().Count() != request.DateOptions.Count)
                return ActivityUpdateResult.Invalid("dateOptions", "候選日期 ID 不可重複。");

            var existingIds = activity.DateOptions.Select(option => option.Id).ToHashSet();
            if (request.DateOptions.Any(option => !existingIds.Contains(option.Id)))
                return ActivityUpdateResult.Invalid("dateOptions", "只能修改這場活動既有的候選日期。");

            if (request.DateOptions.Any(option => option.StartTime.HasValue && option.EndTime.HasValue && option.EndTime <= option.StartTime))
                return ActivityUpdateResult.Invalid("dateOptions", "候選日期的結束時間必須晚於開始時間。");

            var updates = request.DateOptions.ToDictionary(option => option.Id);
            // 同時檢查未修改的項目；只驗證 request 會漏掉與既有日期重複的情況。
            var resultingDates = activity.DateOptions.Select(option => updates.TryGetValue(option.Id, out var update)
                ? (update.OptionDate, update.StartTime)
                : (option.OptionDate, option.StartTime));
            if (resultingDates.Distinct().Count() != activity.DateOptions.Count)
                return ActivityUpdateResult.Invalid("dateOptions", "候選日期與開始時間不可重複。");

            // 所有驗證通過後才修改 tracked entity，失敗時不留下部分變更。
            foreach (var option in activity.DateOptions)
            {
                if (!updates.TryGetValue(option.Id, out var update)) continue;
                option.OptionDate = update.OptionDate;
                option.StartTime = update.StartTime;
                option.EndTime = update.EndTime;
            }
        }

        if (request.Title is not null) activity.Title = request.Title.Trim();
        activity.UpdatedAt = now;
        await activityRepository.SaveChangesAsync();
        return ActivityUpdateResult.Success(ToResponse(activity));
    }

    private static ActivityResponse ToResponse(ActivityEntity activity) => new(
        activity.Id, activity.Title, activity.ActivityTypeId,
        activity.BudgetMin, activity.BudgetMax, activity.CurrencyCode,
        activity.CityId, activity.DistrictId, activity.DeadlineAt, activity.Status,
        activity.CreatedAt, activity.UpdatedAt,
        activity.DateOptions.OrderBy(option => option.SortOrder).ThenBy(option => option.Id)
            .Select(option => new DateOptionResponse(option.Id, option.OptionDate, option.StartTime, option.EndTime, option.SortOrder)).ToList(),
        activity.PlaceOptions.OrderBy(option => option.SortOrder).ThenBy(option => option.Id)
            .Select(option => new PlaceOptionResponse(option.Id, option.SourceType, option.DisplayLabel, option.CustomAddress, option.SortOrder)).ToList());

    public async Task<ActivityCreationResult> CreateAsync(
        long hostUserId,
        CreateActivityRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return ActivityCreationResult.Failure("title", "活動名稱不可為空白。");
        }

        if (request.DeadlineAt <= DateTimeOffset.UtcNow)
        {
            return ActivityCreationResult.Failure("deadlineAt", "投票截止時間必須晚於現在。");
        }

        if (request.BudgetMin.HasValue && request.BudgetMax.HasValue &&
            request.BudgetMax < request.BudgetMin)
        {
            return ActivityCreationResult.Failure("budgetMax", "最高預算不可低於最低預算。");
        }

        if (request.DateOptions.Count == 0)
        {
            return ActivityCreationResult.Failure("dateOptions", "至少需要一個候選日期。");
        }

        if (request.PlaceOptions.Count is < 1 or > 5)
        {
            return ActivityCreationResult.Failure("placeOptions", "候選地點數量必須介於 1 到 5 個。");
        }

        if (request.DateOptions.Any(option =>
                option.EndTime.HasValue && option.StartTime.HasValue &&
                option.EndTime <= option.StartTime))
        {
            return ActivityCreationResult.Failure("dateOptions", "候選日期的結束時間必須晚於開始時間。");
        }

        if (request.DateOptions
            .GroupBy(option => new { option.OptionDate, option.StartTime })
            .Any(group => group.Count() > 1))
        {
            return ActivityCreationResult.Failure("dateOptions", "候選日期與開始時間不可重複。");
        }

        if (request.PlaceOptions.Any(option => string.IsNullOrWhiteSpace(option.DisplayLabel)))
        {
            return ActivityCreationResult.Failure("placeOptions", "候選地點名稱不可為空白。");
        }

        if (!await activityRepository.ActiveActivityTypeExistsAsync(request.ActivityTypeId))
        {
            return ActivityCreationResult.Failure("activityTypeId", "活動類型不存在或已停用。");
        }

        if (!await activityRepository.ActiveCityExistsAsync(request.CityId))
        {
            return ActivityCreationResult.Failure("cityId", "縣市不存在或已停用。");
        }

        if (request.DistrictId.HasValue &&
            !await activityRepository.ActiveDistrictBelongsToCityAsync(
                request.DistrictId.Value,
                request.CityId))
        {
            return ActivityCreationResult.Failure(
                "districtId",
                "行政區不存在、已停用，或不屬於所選縣市。");
        }

        var shareToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var now = DateTimeOffset.UtcNow;
        var activity = new ActivityEntity
        {
            HostUserId = hostUserId,
            ActivityTypeId = request.ActivityTypeId,
            Title = request.Title.Trim(),
            BudgetMin = request.BudgetMin,
            BudgetMax = request.BudgetMax,
            CityId = request.CityId,
            DistrictId = request.DistrictId,
            // PostgreSQL timestamptz 由 Npgsql 以 UTC 寫入；保留相同時間點並將 offset 正規化為 +00:00。
            DeadlineAt = request.DeadlineAt.ToUniversalTime(),
            Status = "open",
            ShareTokenHash = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(shareToken))),
            CreatedAt = now,
            UpdatedAt = now,
            DateOptions = request.DateOptions.Select((option, index) => new DateOption
            {
                OptionDate = option.OptionDate,
                StartTime = option.StartTime,
                EndTime = option.EndTime,
                SortOrder = checked((short)index),
                CreatedAt = now
            }).ToList(),
            PlaceOptions = request.PlaceOptions.Select((option, index) => new PlaceOption
            {
                SourceType = "custom",
                DisplayLabel = option.DisplayLabel.Trim(),
                CustomAddress = string.IsNullOrWhiteSpace(option.CustomAddress)
                    ? null
                    : option.CustomAddress.Trim(),
                SortOrder = checked((short)index),
                CreatedAt = now
            }).ToList()
        };

        await activityRepository.AddAsync(activity);

        return ActivityCreationResult.Success(
            new CreateActivityResponse(activity.Id, shareToken));
    }
}
