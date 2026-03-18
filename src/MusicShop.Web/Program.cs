using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using MusicShop.Data;
using MusicShop.Data.Entities;
using MusicShop.Data.UnitOfWork;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Service.Services.Implementation;
using MusicShop.Web.Infrastructure;
using MusicShop.Web.Infrastructure.Implementation;
using Serilog;
using Serilog.Events;

// =====================================================================
// Serilog 結構化日誌設定
// 取代內建 Console Logger，支援檔案日誌、結構化查詢與滾動歸檔
// =====================================================================
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/app-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// 使用 Serilog 取代內建日誌系統
builder.Host.UseSerilog();

// Add services to the container.
// 綁定網站全域設定（SiteSettings）至 appsettings.json 的 SiteSettings 區段
builder.Services.Configure<SiteSettings>(builder.Configuration.GetSection("SiteSettings"));

// 註冊記憶體快取
builder.Services.AddMemoryCache();

// 註冊 DbContext（Scoped）- 每個 HTTP 請求共用一個 DbContext
// ConfigureWarnings：抑制軟刪除 Global Query Filter 與必要端關聯的警告（事件 10622）。
// CartItem/OrderItem/WishlistItem 的 AlbumId 及 UserCoupon 的 CouponId 為必要 FK，
// 但 Album/Coupon 使用軟刪除不會真正消失，故此警告為已知的設計選擇，可安全忽略。
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .ConfigureWarnings(w => w.Ignore(new EventId(10622))));

// 註冊 UnitOfWork（資料存取層）
// 管理所有 Repository 並提供統一的交易控制
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// 註冊快取服務（Singleton，跨請求共享快取資料）
builder.Services.AddSingleton<ICacheService, CacheService>();

// 註冊 AutoMapper
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<MusicShop.Service.Mapper.MapperProfile>();
});

// 註冊 Service（商業邏輯層）
builder.Services.AddScoped<IAlbumService, AlbumService>();
builder.Services.AddScoped<IArtistService, ArtistService>();
builder.Services.AddScoped<IArtistCategoryService, ArtistCategoryService>();
builder.Services.AddScoped<IProductTypeService, ProductTypeService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderValidationService, OrderValidationService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBannerService, BannerService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();
builder.Services.AddScoped<IFeaturedArtistService, FeaturedArtistService>();
builder.Services.AddScoped<ICouponService, CouponService>();

// 註冊 Web 層基礎設施服務
builder.Services.AddScoped<IAlbumImageService, AlbumImageService>();
builder.Services.AddScoped<IBannerImageService, BannerImageService>();

// 註冊 SMTP Email 寄送服務
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

// 註冊 ECPay 物流服務（含 HttpClient，設定 30 秒逾時避免請求掛起）
builder.Services.AddHttpClient<IEcpayLogisticsService, EcpayLogisticsService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

// =====================================================================
// Identity 設定（密碼規則 + 帳號鎖定策略）
// 對應 OWASP A07:Identification and Authentication Failures
// =====================================================================
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    // ─── 密碼規則 ─────────────────────────────────────────
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;

    // ─── Email 驗證 ──────────────────────────────────────
    // 註冊後需驗證 Email 才能登入，防止惡意註冊與帳號冒用
    options.SignIn.RequireConfirmedEmail = true;

    // ─── 帳號鎖定策略（CWE-307: 暴力破解防護）─────────────
    // 連續登入失敗達上限時暫時鎖定帳號，阻止自動化密碼猜測工具。
    // 搭配 UserService.LoginAsync 中 lockoutOnFailure: true 啟用。
    // 流程：失敗 3 次 → 鎖定 5 分鐘 → 自動解鎖 → 計數歸零
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);   // 鎖定持續時間
    options.Lockout.MaxFailedAccessAttempts = 3;                        // 最大失敗次數
    options.Lockout.AllowedForNewUsers = true;                          // 新註冊帳號也受保護
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// =====================================================================
// Cookie 驗證安全設定
// 涵蓋 OWASP Top 10 中 A01（存取控制）、A03（XSS）、A07（認證）相關項目
// 弱點掃描工具（OWASP ZAP / Burp Suite）會逐項檢查以下 Flag
// =====================================================================
builder.Services.ConfigureApplicationCookie(options =>
{
    // ─── 導向路徑設定 ─────────────────────────────────────
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";

    // ─── Session 過期控制（CWE-613: Insufficient Session Expiration）───
    // 閒置超過 2 小時自動失效，即使 Cookie 被竊取也限制可利用時間。
    // SlidingExpiration：使用者持續操作時，超過 50% 有效期的請求會自動延長。
    // 範例：09:00 登入 → 10:05 操作觸發延長至 12:05 → 不操作則 12:05 過期
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.SlidingExpiration = true;

    // ─── HttpOnly（CWE-1004: Sensitive Cookie Without HttpOnly Flag）───
    // Set-Cookie 標頭加上 HttpOnly flag，瀏覽器禁止 JavaScript 透過
    // document.cookie 讀取此 Cookie。即使網站存在 XSS 漏洞，
    // 攻擊者的惡意腳本也無法竊取認證 Cookie 傳送到外部伺服器。
    options.Cookie.HttpOnly = true;

    // ─── Secure（CWE-614: Sensitive Cookie Without Secure Attribute）──
    // Cookie 僅在 HTTPS 加密連線時傳送。防止在公共 WiFi 等環境下，
    // 攻擊者透過中間人攻擊（MITM）截獲明文傳輸的 Cookie。
    // SameAsRequest：開發環境 http 可傳送、正式環境 https 自動套用 Secure flag，
    // 兼顧本機開發便利與正式環境安全。正式環境務必全站強制 HTTPS。
    // options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

    // ─── SameSite（CWE-1275: Improper SameSite Attribute）─────────
    // 限制跨站請求攜帶 Cookie，搭配 [ValidateAntiForgeryToken] 形成雙重 CSRF 防護。
    // Lax 模式：GET 導覽（如 Google 搜尋點擊連結）允許帶 Cookie，
    //           POST 跨站請求（如惡意網站的隱藏表單提交）不帶 Cookie。
    // 比 Strict 更實用——使用者從外部連結點進來不需要重新登入。
    options.Cookie.SameSite = SameSiteMode.Lax;

    // ─── 自訂 Cookie 名稱（CWE-200: Information Disclosure）──────
    // 預設名稱 .AspNetCore.Identity.Application 會暴露後端技術棧，
    // 讓攻擊者針對已知的 ASP.NET Core 漏洞發動攻擊。
    // 自訂名稱可降低被自動化掃描工具識別框架的風險。
    options.Cookie.Name = "MusicShop.Auth";
});

builder.Services.AddControllersWithViews();

// =====================================================================
// Rate Limiting 速率限制（CWE-770: Allocation of Resources Without Limits）
// 防止惡意刷單、API 濫用與暴力破解攻擊
// =====================================================================
builder.Services.AddRateLimiter(options =>
{
    // 全域被拒絕時回傳 429 Too Many Requests
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // ─── 一般請求限制（滑動視窗演算法）──────────────────────
    // 每分鐘最多 100 次請求，超過則暫時拒絕
    options.AddSlidingWindowLimiter("general", limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.SegmentsPerWindow = 6;
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 5;
    });

    // ─── API 請求限制 ───────────────────────────────────────
    // API 端點較敏感，每分鐘最多 30 次
    options.AddSlidingWindowLimiter("api", limiterOptions =>
    {
        limiterOptions.PermitLimit = 30;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.SegmentsPerWindow = 6;
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 2;
    });

    // ─── 登入/註冊限制 ──────────────────────────────────────
    // 認證端點最敏感，每分鐘最多 10 次，搭配 Identity 鎖定機制形成雙重防護
    options.AddFixedWindowLimiter("auth", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });

    // 以 IP 為鍵的全域限制器（未標記特定 Policy 的端點使用）
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 200,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 10
            }));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseGlobalExceptionHandler();
app.UseSecurityHeaders();
app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");

if (!app.Environment.IsDevelopment())
{
    // HSTS：告知瀏覽器未來 1 年內只能透過 HTTPS 連線（含子網域）
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

// Serilog HTTP 請求日誌（記錄每個請求的方法、路徑、狀態碼與耗時）
app.UseSerilogRequestLogging(options =>
{
    // 排除靜態檔案與健康檢查等高頻低價值請求
    options.GetLevel = (httpContext, elapsed, ex) =>
    {
        if (ex != null) return LogEventLevel.Error;
        if (httpContext.Response.StatusCode >= 500) return LogEventLevel.Error;
        if (httpContext.Response.StatusCode >= 400) return LogEventLevel.Warning;
        return LogEventLevel.Information;
    };
});

app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// Area 路由（後台管理 Area 需在預設路由之前註冊，確保優先匹配）
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// 前台預設路由
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// 初始化資料庫角色、管理員帳戶和預設分類
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var configuration = services.GetRequiredService<IConfiguration>();
        var context = services.GetRequiredService<ApplicationDbContext>();

        // 執行資料庫初始化（含角色、管理員、分類、藝人種子資料）
        await DbInitializer.InitializeAsync(roleManager, userManager, configuration, context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "資料庫初始化時發生錯誤");
    }
}

try
{
    Log.Information("MusicShop 應用程式啟動");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "MusicShop 應用程式啟動失敗");
}
finally
{
    Log.CloseAndFlush();
}
