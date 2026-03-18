using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MusicShop.Data;
using MusicShop.Data.Entities;
using MusicShop.Data.UnitOfWork;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Service.Services.Implementation;
using MusicShop.Web.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseGlobalExceptionHandler();
app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");

if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseRouting();
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

app.Run();
