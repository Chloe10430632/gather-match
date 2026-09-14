using asp_gather_match.Contracts.Common;
using asp_gather_match.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace asp_gather_match.Controllers;

[ApiController]
[Route("api/reference-data")]
public class ReferenceDataController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> Get(CancellationToken cancellationToken)
    {
        var activityTypes = await db.ActivityTypes.AsNoTracking().Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
            .Select(x => new { x.Id, x.Name }).ToListAsync(cancellationToken);
        var cities = await db.Cities.AsNoTracking().Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
            .Select(x => new { x.Id, x.Name, Districts = x.Districts.Where(d => d.IsActive)
                .OrderBy(d => d.SortOrder).ThenBy(d => d.Id).Select(d => new { d.Id, d.Name }).ToList() })
            .ToListAsync(cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { activityTypes, cities }, HttpContext.TraceIdentifier));
    }
}
