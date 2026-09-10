using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace asp_gather_match.Contracts.Activities;

// 不接受不可修改的欄位，避免客戶端誤以為地點或截止時間已更新。
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class UpdateActivityRequest
{
    [StringLength(100, MinimumLength = 1)]
    public string? Title { get; set; }

    [MinLength(1)]
    public List<UpdateDateOptionRequest>? DateOptions { get; set; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class UpdateDateOptionRequest
{
    [Range(1, long.MaxValue)]
    public long Id { get; set; }

    [JsonRequired]
    public DateOnly OptionDate { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }
}
