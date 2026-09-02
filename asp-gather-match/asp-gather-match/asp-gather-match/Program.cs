using asp_gather_match.Contracts.Common;
using asp_gather_match.Data;
using asp_gather_match.ErrorHandling;
using asp_gather_match.Models;
using asp_gather_match.Repositories;
using asp_gather_match.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 以下 builder.Services 是「DI 服務註冊」：先告訴 ASP.NET Core 可提供哪些物件，
// Controller 或其他服務需要它們時，框架就能自動建立並注入。
// 註冊 MVC Controller，以及 [ApiController] 的模型驗證等 Web API 功能。
builder.Services.AddControllers();

// 改寫 [ApiController] 的預設驗證失敗回應，讓 DTO 驗證錯誤也使用 ApiResponse 統一格式。
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

// 將 ApplicationDbContext 註冊為 Scoped：同一個 HTTP request 共用同一個 DbContext，
// request 結束後由框架釋放；UseNpgsql 指定它要連線至 PostgreSQL。
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// 註冊授權服務，供 [Authorize] 與 UseAuthorization 判斷目前使用者是否有權限。
builder.Services.AddAuthorization();

// 註冊 ASP.NET Core Identity 所需的帳號、密碼驗證與登入服務，
// 例如 AuthController 注入的 UserManager 與 SignInManager。
// AddEntityFrameworkStores 表示 Identity 帳號資料由 ApplicationDbContext 存取。
builder.Services
    .AddIdentityApiEndpoints<ApplicationUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// AddScoped 表示每個 HTTP request 各建立一份實例；
// 當程式要求介面時，DI 會提供右側的實作類別。
builder.Services.AddScoped<IActivityRepository, ActivityRepository>();
builder.Services.AddScoped<IActivityService, ActivityService>();

// 位置資料匯入器也需要 DbContext，因此跟著 request／手動建立的 scope 共用同一生命週期。
builder.Services.AddScoped<LocationReferenceImporter>();

// 註冊全域例外處理器。真正將它放入 HTTP 管線的是下方的 UseExceptionHandler。
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// 提供 ASP.NET Core 標準錯誤資訊所需的服務，並支援例外處理機制。
builder.Services.AddProblemDetails();

// 產生 OpenAPI 文件，並註冊 Swagger UI 所需的文件產生服務。
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

// 以下 app.Use...／app.Map... 是「HTTP middleware 管線」：
// request 會依照這裡的順序通過每一層處理。
if (app.Environment.IsDevelopment())
{
    // 只在 Development 提供 OpenAPI JSON 與 Swagger 測試介面。
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 將 HTTP request 重新導向 HTTPS。
app.UseHttpsRedirection();

// 捕捉後續 middleware／Controller 未處理的例外，交由 GlobalExceptionHandler 統一回傳 500。
app.UseExceptionHandler();

// 當後續處理只設定 401、403、404 等狀態碼、但沒有 Response Body 時，
// 補上統一的 ApiResponse 錯誤內容。
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

// 先讀取登入 Cookie 等憑證，確認「這個 request 是誰」，並建立 HttpContext.User。
app.UseAuthentication();

// 再根據 [Authorize] 等規則，確認「這個使用者有沒有權限」。順序不可和 Authentication 對調。
app.UseAuthorization();

// 將標有 [Route]／[HttpPost] 等 Attribute 的 Controller endpoints 加入路由。
app.MapControllers();

app.Run();
