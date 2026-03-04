using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MusicShop.Models;
using MusicShop.ViewModels;
using MusicShop.Services.Interface;

namespace MusicShop.Controllers;

/// <summary>
/// 帳號管理控制器 - 展示層
/// 使用三層式架構：Controller → Service (UserManager/SignInManager/IOrderService)
/// </summary>
public class AccountController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IOrderService _orderService;

    public AccountController(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IOrderService orderService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _orderService = orderService;
    }

    // GET: /Account/Register
    public IActionResult Register() => View();

    // POST: /Account/Register
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = new AppUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            PhoneNumber = model.PhoneNumber,
            Birthday = model.Birthday,
            Gender = model.Gender
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            // 自動指派 User 角色給新註冊的使用者
            await _userManager.AddToRoleAsync(user, "User");

            // 登入使用者
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(model);
    }

    // GET: /Account/Login
    public IActionResult Login() => View();

    // POST: /Account/Login
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _signInManager.PasswordSignInAsync(
            model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

        if (result.Succeeded)
            return RedirectToAction("Index", "Home");

        ModelState.AddModelError(string.Empty, "帳號或密碼錯誤");
        return View(model);
    }

    // POST: /Account/Logout
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    // GET: /Account/AccessDenied
    public IActionResult AccessDenied() => View();

    // ==================== 會員中心功能 ====================

    // GET: /Account/Index
    // 會員中心首頁
    [Authorize]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return RedirectToAction("Login");

        // 透過 Service 層取得使用者訂單資料
        var orders = await _orderService.GetUserOrdersAsync(user.Id);
        var ordersList = orders.ToList();

        var viewModel = new AccountIndexViewModel
        {
            FullName = user.FullName ?? "訪客",
            Email = user.Email ?? "",
            RegisteredAt = user.RegisteredAt,
            TotalSpent = ordersList.Where(o => o.Status != OrderStatus.Cancelled).Sum(o => o.TotalAmount),
            TotalOrders = ordersList.Count,
            RecentOrders = ordersList.Take(5).ToList()
        };

        return View(viewModel);
    }

    // GET: /Account/Edit
    // 編輯個人資料
    [Authorize]
    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return RedirectToAction("Login");

        var viewModel = new EditProfileViewModel
        {
            FullName = user.FullName ?? "",
            Email = user.Email ?? "",
            RegisteredAt = user.RegisteredAt
        };

        return View(viewModel);
    }

    // POST: /Account/Edit
    // 更新個人資料
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Edit(EditProfileViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return RedirectToAction("Login");

        user.FullName = model.FullName;
        user.Email = model.Email;
        user.UserName = model.Email;

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            TempData["Success"] = "個人資料已更新";
            return RedirectToAction("Index");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(model);
    }
}
