using System.ComponentModel.DataAnnotations;

namespace asp_gather_match.Contracts.Activities;

public class CreateActivityRequest
{
    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    public short ActivityTypeId { get; set; }

    [Range(typeof(decimal), "0", "99999999.99")]
    public decimal? BudgetMin { get; set; }

    [Range(typeof(decimal), "0", "99999999.99")]
    public decimal? BudgetMax { get; set; }

    public short CityId { get; set; }

    public int? DistrictId { get; set; }

    public DateTimeOffset DeadlineAt { get; set; }

    [MinLength(1)]
    public List<CreateDateOptionRequest> DateOptions { get; set; } = [];

    [MinLength(1)]
    [MaxLength(5)]
    public List<CreatePlaceOptionRequest> PlaceOptions { get; set; } = [];
}

public class CreateDateOptionRequest
{
    public DateOnly OptionDate { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }
}

public class CreatePlaceOptionRequest
{
    [Required]
    [StringLength(200)]
    public string DisplayLabel { get; set; } = string.Empty;

    public string? CustomAddress { get; set; }
}
