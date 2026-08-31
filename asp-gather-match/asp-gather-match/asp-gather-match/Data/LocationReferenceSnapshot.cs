namespace asp_gather_match.Data;

public class LocationReferenceSnapshot
{
    public string Source { get; set; } = string.Empty;
    public string SourceUrl { get; set; } = string.Empty;
    public DateTimeOffset GeneratedAt { get; set; }
    public bool IsCompleteSnapshot { get; set; }
    public List<LocationCitySnapshot> Cities { get; set; } = [];
}

public class LocationCitySnapshot
{
    public string GovernmentCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public short SortOrder { get; set; }
    public List<LocationDistrictSnapshot> Districts { get; set; } = [];
}

public class LocationDistrictSnapshot
{
    public string GovernmentCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public short SortOrder { get; set; }
}
