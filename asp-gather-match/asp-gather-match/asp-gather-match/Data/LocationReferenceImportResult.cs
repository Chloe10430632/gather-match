namespace asp_gather_match.Data;

public record LocationReferenceImportResult(
    int AddedCities,
    int UpdatedCities,
    int DeactivatedCities,
    int AddedDistricts,
    int UpdatedDistricts,
    int DeactivatedDistricts,
    int TotalCities,
    int TotalDistricts);
