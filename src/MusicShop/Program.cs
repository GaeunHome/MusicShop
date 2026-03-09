using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MusicShop.Data;
using MusicShop.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// 使用 DbContextFactory 以支援更好的並行性和資源管理
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 註冊 Repository（資料存取層）
builder.Services.AddScoped<MusicShop.Repositories.Interface.IAlbumRepository, MusicShop.Repositories.Implementation.AlbumRepository>();
builder.Services.AddScoped<MusicShop.Repositories.Interface.IArtistRepository, MusicShop.Repositories.Implementation.ArtistRepository>();
builder.Services.AddScoped<MusicShop.Repositories.Interface.IArtistCategoryRepository, MusicShop.Repositories.Implementation.ArtistCategoryRepository>();
builder.Services.AddScoped<MusicShop.Repositories.Interface.IProductTypeRepository, MusicShop.Repositories.Implementation.ProductTypeRepository>();
builder.Services.AddScoped<MusicShop.Repositories.Interface.ICartRepository, MusicShop.Repositories.Implementation.CartRepository>();
builder.Services.AddScoped<MusicShop.Repositories.Interface.IOrderRepository, MusicShop.Repositories.Implementation.OrderRepository>();
builder.Services.AddScoped<MusicShop.Repositories.Interface.IStatisticsRepository, MusicShop.Repositories.Implementation.StatisticsRepository>();

// 註冊 Service（商業邏輯層）
builder.Services.AddScoped<MusicShop.Services.Interface.IAlbumService, MusicShop.Services.Implementation.AlbumService>();
builder.Services.AddScoped<MusicShop.Services.Interface.IArtistService, MusicShop.Services.Implementation.ArtistService>();
builder.Services.AddScoped<MusicShop.Services.Interface.IArtistCategoryService, MusicShop.Services.Implementation.ArtistCategoryService>();
builder.Services.AddScoped<MusicShop.Services.Interface.IProductTypeService, MusicShop.Services.Implementation.ProductTypeService>();
builder.Services.AddScoped<MusicShop.Services.Interface.ICartService, MusicShop.Services.Implementation.CartService>();
builder.Services.AddScoped<MusicShop.Services.Interface.IOrderService, MusicShop.Services.Implementation.OrderService>();
builder.Services.AddScoped<MusicShop.Services.Interface.IOrderValidationService, MusicShop.Services.Implementation.OrderValidationService>();
builder.Services.AddScoped<MusicShop.Services.Interface.IStatisticsService, MusicShop.Services.Implementation.StatisticsService>();
builder.Services.AddScoped<MusicShop.Services.Interface.IUserService, MusicShop.Services.Implementation.UserService>();

// 加入 Identity
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 登入/登出導向設定
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

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
        var contextFactory = services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
        var context = await contextFactory.CreateDbContextAsync();

        // 執行資料庫初始化
        await MusicShop.Data.DbInitializer.InitializeAsync(roleManager, userManager, configuration, context);

        // 插入藝人資料（如果資料庫中沒有的話）
        await MusicShop.Data.SeedArtists.SeedAsync(contextFactory);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "資料庫初始化時發生錯誤");
    }
}

app.Run();
