using asp_gather_match.Contracts.Common;
using asp_gather_match.Data;
using asp_gather_match.ErrorHandling;
using asp_gather_match.Models;
using asp_gather_match.Repositories;
using asp_gather_match.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .ToDictionary(
                entry => entry.Key,
                entry => entry.Value!.Errors
                    .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                        ? "輸入格式不正確。"
                        : error.ErrorMessage)
                    .ToArray());

        var response = ApiResponse<object>.Fail(
            new ApiError(
                "validation_failed",
                "輸入資料驗證失敗。",
                errors),
            context.HttpContext.TraceIdentifier);

        return new BadRequestObjectResult(response);
    };
});


//connectionString for PostgreSQL database
var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' was not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddAuthorization();

builder.Services
    .AddIdentityApiEndpoints<ApplicationUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddScoped<IActivityRepository, ActivityRepository>();
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<LocationReferenceImporter>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (args.Length == 2 && args[0] == "validate-locations")
{
    var snapshot = await LocationReferenceImporter.LoadAndValidateAsync(args[1], CancellationToken.None);
    Console.WriteLine(
        $"Location snapshot is valid: {snapshot.Cities.Count} cities/counties, " +
        $"{snapshot.Cities.Sum(city => city.Districts.Count)} districts.");
    return;
}

if (args.Length == 2 && args[0] == "import-locations")
{
    await using var scope = app.Services.CreateAsyncScope();
    var importer = scope.ServiceProvider.GetRequiredService<LocationReferenceImporter>();
    await importer.ImportAsync(args[1]);
    return;
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseExceptionHandler();

app.UseStatusCodePages(async statusCodeContext =>
{
    var httpContext = statusCodeContext.HttpContext;
    var (code, message) = httpContext.Response.StatusCode switch
    {
        StatusCodes.Status401Unauthorized =>
            ("unauthorized", "需要登入才能執行此操作。"),
        StatusCodes.Status403Forbidden =>
            ("forbidden", "你沒有執行此操作的權限。"),
        StatusCodes.Status404NotFound =>
            ("not_found", "找不到指定的資源。"),
        _ =>
            ("request_failed", "請求無法完成。")
    };

    var response = ApiResponse<object>.Fail(
        new ApiError(code, message),
        httpContext.TraceIdentifier);

    await httpContext.Response.WriteAsJsonAsync(response);
});

// Add authentication and authorization middleware
//確認「這個請求是誰」
app.UseAuthentication();
//確認「這個請求有沒有權限」
app.UseAuthorization();

app.MapControllers();

app.Run();
