using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicShop.Service.ViewModels.Account;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Web.Infrastructure;

namespace MusicShop.Controllers;

/// <summary>
/// 帳號管理控制器 - 展示層
/// 使用三層式架構：Controller → Service → Repository
/// 所有 Identity 操作透過 IUserService 封裝，不直接接觸 Data 層
/// </summary>
public class AccountController : BaseController
{
    private readonly IOrderService _orderService;
    private readonly IUserService _userService;

    public AccountController(
        IOrderService orderService,
        IUserService userService)
    {
        _orderService = orderService;
        _userService = userService;
    }

    // GET: /Account/Register
    public IActionResult Register() => View();

    // POST: /Account/Register
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var (success, userId, errors) = await _userService.RegisterAsync(model);

        if (success)
        {
            TempData[TempDataKeys.Success] = $"歡迎加入 MusicShop！註冊成功，{model.FullName}。";
            return RedirectToAction("Index", "Home");
        }

        foreach (var error in errors)
            ModelState.AddModelError(string.Empty, error);

        return View(model);
    }

    // GET: /Account/Login
    public IActionResult Login() => View();

    // POST: /Account/Login
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var (success, fullName, isLockedOut, lockoutMinutes, remainingAttempts)
            = await _userService.LoginAsync(model.Email, model.Password, model.RememberMe);

        // 登入成功
        if (success)
        {
            TempData[TempDataKeys.Success] = $"歡迎回來，{fullName ?? ""}！登入成功。";
            return RedirectToAction("Index", "Home");
        }

        // 帳號被鎖定
        if (isLockedOut)
        {
            TempData[TempDataKeys.Error] = $"帳號已被鎖定，請在 {lockoutMinutes} 分鐘後再試。";
            return View(model);
        }

        // 密碼錯誤，顯示剩餘嘗試次數
        if (remainingAttempts > 0)
        {
            TempData[TempDataKeys.Warning] = $"帳號或密碼錯誤，還剩 {remainingAttempts} 次嘗試機會。";
        }
        else
        {
            TempData[TempDataKeys.Error] = "帳號或密碼錯誤，請重新輸入。";
        }

        return View(model);
    }

    // POST: /Account/Logout
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _userService.LogoutAsync();
        TempData[TempDataKeys.Success] = "您已成功登出。";
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
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return RedirectToAction("Login");

        // 透過 Service 層取得使用者帳戶摘要
        var summary = await _userService.GetAccountSummaryAsync(userId);
        if (summary == null)
            return RedirectToAction("Login");

        // 透過 Service 層取得訂單統計與近期訂單
        var (totalOrders, totalSpent) = await _orderService.GetUserOrderStatsAsync(userId);
        var recentOrders = await _orderService.GetRecentOrderViewModelsAsync(userId, 5);

        var viewModel = new AccountIndexViewModel
        {
            FullName = summary.Value.FullName,
            Email = summary.Value.Email,
            RegisteredAt = summary.Value.RegisteredAt,
            TotalSpent = totalSpent,
            TotalOrders = totalOrders,
            RecentOrders = recentOrders
        };

        return View(viewModel);
    }

    // GET: /Account/Edit
    // 編輯個人資料
    [Authorize]
    public async Task<IActionResult> Edit()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return RedirectToAction("Login");

        var viewModel = await _userService.GetEditProfileAsync(userId);
        if (viewModel == null)
            return RedirectToAction("Login");

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

        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return RedirectToAction("Login");

        var (success, errors) = await _userService.UpdateProfileAsync(userId, model);

        if (success)
        {
            TempData[TempDataKeys.Success] = "個人資料已更新";
            return RedirectToAction("Index");
        }

        foreach (var error in errors)
            ModelState.AddModelError(string.Empty, error);

        return View(model);
    }

    // GET: /Account/ChangePassword
    // 更新密碼頁面
    [Authorize]
    public IActionResult ChangePassword()
    {
        return View();
    }

    // POST: /Account/ChangePassword
    // 處理密碼更新
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return RedirectToAction("Login");

        // 透過 Service 層處理密碼更新
        var (success, message) = await _userService.ChangePasswordAsync(
            userId,
            model.CurrentPassword,
            model.NewPassword);

        if (success)
        {
            TempData[TempDataKeys.Success] = message;
            return RedirectToAction("Index");
        }

        ModelState.AddModelError(string.Empty, message);
        return View(model);
    }
}
