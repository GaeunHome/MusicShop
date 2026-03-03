using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MusicShop.Data;
using MusicShop.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 註冊 Repository（資料存取層）
builder.Services.AddScoped<MusicShop.Repositories.Interface.IAlbumRepository, MusicShop.Repositories.Implementation.AlbumRepository>();
builder.Services.AddScoped<MusicShop.Repositories.Interface.ICategoryRepository, MusicShop.Repositories.Implementation.CategoryRepository>();
builder.Services.AddScoped<MusicShop.Repositories.Interface.ICartRepository, MusicShop.Repositories.Implementation.CartRepository>();
builder.Services.AddScoped<MusicShop.Repositories.Interface.IOrderRepository, MusicShop.Repositories.Implementation.OrderRepository>();

// 註冊 Service（商業邏輯層）
builder.Services.AddScoped<MusicShop.Services.Interface.IAlbumService, MusicShop.Services.Implementation.AlbumService>();
builder.Services.AddScoped<MusicShop.Services.Interface.ICategoryService, MusicShop.Services.Implementation.CategoryService>();
builder.Services.AddScoped<MusicShop.Services.Interface.ICartService, MusicShop.Services.Implementation.CartService>();
builder.Services.AddScoped<MusicShop.Services.Interface.IOrderService, MusicShop.Services.Implementation.OrderService>();

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

// 待修改
// Cookie 驗證
// JWT
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    if (!await roleManager.RoleExistsAsync("Admin"))
        await roleManager.CreateAsync(new IdentityRole("Admin"));

    // 從設定檔讀取，不寫死在程式碼裡
    var adminEmail = config["AdminSettings:Email"];
    if (!string.IsNullOrEmpty(adminEmail))
    {
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser != null && !await userManager.IsInRoleAsync(adminUser, "Admin"))
            await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}

app.Run();
