using System.Security.Cryptography;
using System.Text;
using asp_gather_match.Contracts.Activities;
using asp_gather_match.Models;
using asp_gather_match.Repositories;
using ActivityEntity = asp_gather_match.Models.Activity;

namespace asp_gather_match.Services;

public class ActivityService(IActivityRepository activityRepository) : IActivityService
{
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
