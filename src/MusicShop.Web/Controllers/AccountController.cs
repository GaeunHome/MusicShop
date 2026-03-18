using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
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
    private readonly IEmailService _emailService;

    public AccountController(
        IOrderService orderService,
        IUserService userService,
        IEmailService emailService)
    {
        _orderService = orderService;
        _userService = userService;
        _emailService = emailService;
    }

    // ==================== 註冊與 Email 驗證 ====================

    // GET: /Account/Register
    public IActionResult Register() => View();

    // POST: /Account/Register
    [HttpPost, ValidateAntiForgeryToken]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var (success, userId, errors) = await _userService.RegisterAsync(model);

        if (success)
        {
            // 產生 Email 驗證 Token 並寄送驗證信
            await SendConfirmationEmailAsync(userId, model.Email);

            TempData[TempDataKeys.Success] = $"註冊成功！我們已寄送驗證信至 {model.Email}，請點擊信中連結完成驗證。";
            return RedirectToAction(nameof(RegisterConfirmation), new { email = model.Email, userId });
        }

        foreach (var error in errors)
            ModelState.AddModelError(string.Empty, error);

        return View(model);
    }

    // GET: /Account/RegisterConfirmation
    // 註冊後提示驗證信已寄出
    public IActionResult RegisterConfirmation(string email, string userId)
    {
        ViewBag.Email = email;
        ViewBag.UserId = userId;
        return View();
    }

    // POST: /Account/ResendConfirmation
    // 重新寄送驗證信
    [HttpPost, ValidateAntiForgeryToken]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ResendConfirmation(string userId)
    {
        var (token, uid, email) = await _userService.ResendConfirmationTokenAsync(userId);

        if (token != null && email != null)
        {
            await SendConfirmationEmailAsync(uid!, email);
            TempData[TempDataKeys.Success] = $"驗證信已重新寄送至 {email}";
        }
        else
        {
            TempData[TempDataKeys.Info] = "此帳號已完成驗證，請直接登入";
            return RedirectToAction(nameof(Login));
        }

        return RedirectToAction(nameof(RegisterConfirmation), new { email, userId });
    }

    // GET: /Account/ConfirmEmail
    // 使用者點擊驗證信連結
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            TempData[TempDataKeys.Error] = "無效的驗證連結";
            return RedirectToAction("Index", "Home");
        }

        var (success, message) = await _userService.ConfirmEmailAsync(userId, token);

        if (success)
            TempData[TempDataKeys.Success] = message;
        else
            TempData[TempDataKeys.Error] = message;

        return RedirectToAction("Index", "Home");
    }

    // ==================== 登入 / 登出 ====================

    // GET: /Account/Login
    public IActionResult Login() => View();

    // POST: /Account/Login
    [HttpPost, ValidateAntiForgeryToken]
    [EnableRateLimiting("auth")]
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

        // Email 未驗證（remainingAttempts == -1 代表未驗證）
        if (remainingAttempts == -1)
        {
            TempData[TempDataKeys.Warning] = "您的 Email 尚未驗證，請先點擊驗證信中的連結。";
            return View(model);
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

    // ==================== 忘記密碼 ====================

    // GET: /Account/ForgotPassword
    public IActionResult ForgotPassword() => View();

    // POST: /Account/ForgotPassword
    [HttpPost, ValidateAntiForgeryToken]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var (token, userId) = await _userService.GeneratePasswordResetTokenAsync(model.Email);

        if (token != null && userId != null)
        {
            // 組建重設連結
            var resetUrl = Url.Action(nameof(ResetPassword), "Account",
                new { userId, token }, Request.Scheme);

            // 寄送重設密碼信
            await _emailService.SendEmailAsync(model.Email, "MusicShop - 重設密碼",
                $@"<h3>重設密碼</h3>
                <p>您好，我們收到了您的密碼重設請求。</p>
                <p>請點擊以下連結重設您的密碼：</p>
                <p><a href='{resetUrl}' style='padding: 10px 20px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px;'>重設密碼</a></p>
                <p>此連結將在一段時間後失效。如果您未申請重設密碼，請忽略此信。</p>
                <p>── MusicShop 團隊</p>");
        }

        // 無論是否找到帳號，都顯示相同訊息（防止帳號列舉攻擊）
        TempData[TempDataKeys.Success] = $"如果 {model.Email} 是已註冊的帳號，我們已寄送密碼重設信。";
        return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }

    // GET: /Account/ForgotPasswordConfirmation
    public IActionResult ForgotPasswordConfirmation() => View();

    // GET: /Account/ResetPassword
    public IActionResult ResetPassword(string userId, string token)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            TempData[TempDataKeys.Error] = "無效的重設連結";
            return RedirectToAction("Index", "Home");
        }

        return View(new ResetPasswordViewModel { UserId = userId, Token = token });
    }

    // POST: /Account/ResetPassword
    [HttpPost, ValidateAntiForgeryToken]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var (success, message) = await _userService.ResetPasswordAsync(
            model.UserId, model.Token, model.NewPassword);

        if (success)
        {
            TempData[TempDataKeys.Success] = message;
            return RedirectToAction(nameof(Login));
        }

        ModelState.AddModelError(string.Empty, message);
        return View(model);
    }

    // ==================== 會員中心功能 ====================

    // GET: /Account/Index
    // 會員中心首頁
    [Authorize]
    public async Task<IActionResult> Index()
    {
        var userId = GetAuthorizedUserId();

        // 透過 Service 層取得使用者帳戶摘要
        var summary = await _userService.GetAccountSummaryAsync(userId);

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
        var userId = GetAuthorizedUserId();

        var viewModel = await _userService.GetEditProfileAsync(userId);

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

        var userId = GetAuthorizedUserId();

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

        var userId = GetAuthorizedUserId();

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

    // ==================== 私有輔助方法 ====================

    /// <summary>
    /// 產生並寄送 Email 驗證信
    /// </summary>
    private async Task SendConfirmationEmailAsync(string userId, string email)
    {
        var (token, _) = await _userService.GenerateEmailConfirmationTokenAsync(userId);
        if (token == null) return;

        var confirmUrl = Url.Action(nameof(ConfirmEmail), "Account",
            new { userId, token }, Request.Scheme);

        await _emailService.SendEmailAsync(email, "MusicShop - 驗證您的 Email",
            $@"<h3>歡迎加入 MusicShop！</h3>
            <p>請點擊以下連結完成 Email 驗證：</p>
            <p><a href='{confirmUrl}' style='padding: 10px 20px; background-color: #28a745; color: white; text-decoration: none; border-radius: 5px;'>驗證 Email</a></p>
            <p>如果您未註冊此帳號，請忽略此信。</p>
            <p>── MusicShop 團隊</p>");
    }
}
