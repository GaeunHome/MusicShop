using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    private readonly ICaptchaService _captchaService;

    public AccountController(
        IOrderService orderService,
        IUserService userService,
        IEmailService emailService,
        ICaptchaService captchaService)
    {
        _orderService = orderService;
        _userService = userService;
        _emailService = emailService;
        _captchaService = captchaService;
    }

    // ==================== 驗證碼 ====================

    // GET: /Account/CaptchaImage
    // 產生驗證碼圖片（供登入頁面 <img> 標籤載入）
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult CaptchaImage()
    {
        var imageBytes = _captchaService.GenerateCaptchaImage(HttpContext.Session);
        return File(imageBytes, "image/png");
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
    public async Task<IActionResult> Login()
    {
        ViewBag.ExternalProviders = await _userService.GetExternalAuthenticationSchemesAsync();
        return View();
    }

    // POST: /Account/Login
    [HttpPost, ValidateAntiForgeryToken]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        // 驗證失敗返回 View 時需要外部登入提供者資料
        ViewBag.ExternalProviders = await _userService.GetExternalAuthenticationSchemesAsync();

        if (!ModelState.IsValid) return View(model);

        // 驗證碼檢查（在密碼驗證之前，避免無效請求消耗 Identity 資源）
        if (!_captchaService.ValidateCaptcha(HttpContext.Session, model.CaptchaCode))
        {
            ModelState.AddModelError(nameof(model.CaptchaCode), "驗證碼不正確，請重新輸入");
            return View(model);
        }

        var (success, fullName, isLockedOut, lockoutMinutes, remainingAttempts, requiresTwoFactor)
            = await _userService.LoginAsync(model.Email, model.Password, model.RememberMe);

        // 登入成功
        if (success)
        {
            TempData[TempDataKeys.Success] = $"歡迎回來，{fullName ?? ""}！登入成功。";
            return RedirectToAction("Index", "Home");
        }

        // 需要兩步驟驗證
        if (requiresTwoFactor)
        {
            return RedirectToAction(nameof(LoginTwoFactor), new { rememberMe = model.RememberMe });
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

    // ==================== 社群登入 ====================

    // POST: /Account/ExternalLogin
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult ExternalLogin(string provider)
    {
        var redirectUrl = Url.Action(nameof(ExternalLoginCallback));
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, provider);
    }

    // GET: /Account/ExternalLoginCallback
    public async Task<IActionResult> ExternalLoginCallback()
    {
        var info = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
        if (info?.Principal == null)
        {
            TempData[TempDataKeys.Error] = "社群登入失敗，請重試。";
            return RedirectToAction(nameof(Login));
        }

        var provider = info.Properties?.Items[".AuthScheme"] ?? "Unknown";
        var providerKey = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        var name = info.Principal.FindFirstValue(ClaimTypes.Name);

        // 清除外部登入 Cookie
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        var (success, fullName, isNewUser) = await _userService.ExternalLoginAsync(
            provider, providerKey, email, name);

        if (!success)
        {
            TempData[TempDataKeys.Error] = "社群登入失敗，請改用 Email 登入或聯繫客服。";
            return RedirectToAction(nameof(Login));
        }

        if (isNewUser)
        {
            TempData[TempDataKeys.Success] = $"歡迎加入，{fullName}！已透過 {provider} 建立帳號。建議您補充個人資料以獲得完整的購物體驗。";
            return RedirectToAction(nameof(Edit));
        }

        TempData[TempDataKeys.Success] = $"歡迎回來，{fullName}！";
        return RedirectToAction("Index", "Home");
    }

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

        // 取得 2FA 狀態
        var twoFactorStatus = await _userService.GetTwoFactorStatusAsync(userId);

        var viewModel = new AccountIndexViewModel
        {
            FullName = summary.Value.FullName,
            Email = summary.Value.Email,
            RegisteredAt = summary.Value.RegisteredAt,
            HasPhone = summary.Value.HasPhone,
            HasBirthday = summary.Value.HasBirthday,
            TwoFactorEnabled = twoFactorStatus.IsEnabled,
            TwoFactorMethod = twoFactorStatus.PreferredMethod,
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
        {
            // 重新填充 View 層所需的非表單屬性
            await PopulateEditProfileViewPropertiesAsync(model);
            return View(model);
        }

        var userId = GetAuthorizedUserId();

        var (success, errors) = await _userService.UpdateProfileAsync(userId, model);

        if (success)
        {
            TempData[TempDataKeys.Success] = "個人資料已更新";
            return RedirectToAction("Index");
        }

        foreach (var error in errors)
            ModelState.AddModelError(string.Empty, error);

        await PopulateEditProfileViewPropertiesAsync(model);
        return View(model);
    }

    /// <summary>
    /// 填充編輯個人資料頁面的非表單屬性（IsExternalOnly、RegisteredAt）
    /// </summary>
    private async Task PopulateEditProfileViewPropertiesAsync(EditProfileViewModel model)
    {
        var userId = GetAuthorizedUserId();
        var fullProfile = await _userService.GetEditProfileAsync(userId);
        if (fullProfile != null)
        {
            model.IsExternalOnly = fullProfile.IsExternalOnly;
            model.RegisteredAt = fullProfile.RegisteredAt;
        }
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

    // ==================== 兩步驟驗證 (2FA) 設定 ====================

    // GET: /Account/TwoFactorAuthentication
    // 兩步驟驗證管理頁面
    [Authorize]
    public async Task<IActionResult> TwoFactorAuthentication()
    {
        var userId = GetAuthorizedUserId();
        var model = await _userService.GetTwoFactorStatusAsync(userId);
        return View(model);
    }

    // GET: /Account/SetupAuthenticator
    // TOTP 驗證器設定頁面（重設金鑰並顯示 QR Code）
    [Authorize]
    public async Task<IActionResult> SetupAuthenticator()
    {
        var userId = GetAuthorizedUserId();
        var model = await _userService.SetupAuthenticatorAsync(userId);
        model.QrCodeBase64 = GenerateQrCodeBase64(model.AuthenticatorUri);
        return View(model);
    }

    // POST: /Account/SetupAuthenticator
    // 驗證並啟用 TOTP 驗證器
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> SetupAuthenticator(SetupAuthenticatorViewModel model)
    {
        var userId = GetAuthorizedUserId();

        if (!ModelState.IsValid)
        {
            // 驗證失敗時取得現有金鑰，不重設（避免使用者需重新掃描 QR Code）
            var existing = await _userService.GetAuthenticatorSetupAsync(userId);
            model.SharedKey = existing.SharedKey;
            model.QrCodeBase64 = GenerateQrCodeBase64(existing.AuthenticatorUri);
            return View(model);
        }

        var (success, message) = await _userService.EnableAuthenticatorAsync(userId, model.VerificationCode);

        if (success)
        {
            TempData[TempDataKeys.Success] = message;
            return RedirectToAction(nameof(TwoFactorAuthentication));
        }

        // 驗證碼錯誤，保留現有金鑰讓使用者重試
        var current = await _userService.GetAuthenticatorSetupAsync(userId);
        model.SharedKey = current.SharedKey;
        model.QrCodeBase64 = GenerateQrCodeBase64(current.AuthenticatorUri);
        ModelState.AddModelError(string.Empty, message);
        return View(model);
    }

    // GET: /Account/SetupEmailTwoFactor
    // Email OTP 設定頁面（會寄送驗證碼，需限制速率）
    [Authorize]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> SetupEmailTwoFactor()
    {
        var userId = GetAuthorizedUserId();
        var (success, message, code) = await _userService.GenerateEmailTwoFactorSetupCodeAsync(userId);

        if (!success)
        {
            TempData[TempDataKeys.Error] = message;
            return RedirectToAction(nameof(TwoFactorAuthentication));
        }

        // 寄送驗證碼 Email
        var summary = await _userService.GetAccountSummaryAsync(userId);
        if (summary != null && code != null && !string.IsNullOrEmpty(summary.Value.Email))
        {
            await _emailService.SendEmailAsync(summary.Value.Email, "MusicShop - 兩步驟驗證設定",
                $@"<h3>兩步驟驗證設定</h3>
                <p>您好，您正在設定 Email 兩步驟驗證。</p>
                <p>您的驗證碼為：<strong style='font-size: 1.5em; letter-spacing: 3px;'>{code}</strong></p>
                <p>此驗證碼將在數分鐘後失效。</p>
                <p>如果這不是您本人的操作，請忽略此信。</p>
                <p>── MusicShop 團隊</p>");
        }

        ViewBag.Email = summary?.Email;
        return View();
    }

    // POST: /Account/SetupEmailTwoFactor
    // 確認 Email OTP 設定
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> SetupEmailTwoFactor(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            ModelState.AddModelError(string.Empty, "請輸入驗證碼");
            return View();
        }

        var userId = GetAuthorizedUserId();
        var (success, message) = await _userService.EnableEmailTwoFactorAsync(userId, code);

        if (success)
        {
            TempData[TempDataKeys.Success] = message;
            return RedirectToAction(nameof(TwoFactorAuthentication));
        }

        ModelState.AddModelError(string.Empty, message);
        return View();
    }

    // POST: /Account/DisableTwoFactor
    // 停用 2FA
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> DisableTwoFactor()
    {
        var userId = GetAuthorizedUserId();
        var (success, message) = await _userService.DisableTwoFactorAsync(userId);

        TempData[success ? TempDataKeys.Success : TempDataKeys.Error] = message;
        return RedirectToAction(nameof(TwoFactorAuthentication));
    }

    // ==================== 2FA 登入流程 ====================

    // GET: /Account/LoginTwoFactor
    // 2FA 驗證頁面（密碼驗證後跳轉到此，Email 方式會自動寄送驗證碼）
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> LoginTwoFactor(bool rememberMe)
    {
        var (userId, preferredMethod, email) = await _userService.GetTwoFactorUserInfoAsync();
        if (userId == null)
        {
            TempData[TempDataKeys.Error] = "驗證階段已過期，請重新登入";
            return RedirectToAction(nameof(Login));
        }

        // 如果是 Email 方式，自動寄送驗證碼
        if (preferredMethod == "Email")
        {
            var (success, _, code) = await _userService.GenerateTwoFactorEmailCodeAsync();
            if (success && code != null && email != null)
                await SendTwoFactorCodeEmailAsync(email, code);
        }

        var model = new TwoFactorLoginViewModel
        {
            RememberMe = rememberMe,
            Method = preferredMethod ?? "Authenticator"
        };

        ViewBag.MaskedEmail = email != null ? MaskEmail(email) : null;
        return View(model);
    }

    // POST: /Account/LoginTwoFactor
    // 處理 2FA 驗證
    [HttpPost, ValidateAntiForgeryToken]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> LoginTwoFactor(TwoFactorLoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var (success, fullName, isLockedOut, lockoutMinutes) = model.Method == "Email"
            ? await _userService.TwoFactorEmailLoginAsync(model.Code, model.RememberMe)
            : await _userService.TwoFactorAuthenticatorLoginAsync(model.Code, model.RememberMe);

        if (success)
        {
            TempData[TempDataKeys.Success] = "登入成功！";
            return RedirectToAction("Index", "Home");
        }

        if (isLockedOut)
        {
            TempData[TempDataKeys.Error] = $"帳號已被鎖定，請在 {lockoutMinutes} 分鐘後再試。";
            return RedirectToAction(nameof(Login));
        }

        ModelState.AddModelError(string.Empty, "驗證碼不正確，請重新輸入");
        return View(model);
    }

    // POST: /Account/ResendTwoFactorCode
    // 重新寄送 Email OTP（登入流程中）
    [HttpPost, ValidateAntiForgeryToken]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ResendTwoFactorCode(bool rememberMe)
    {
        var (_, _, email) = await _userService.GetTwoFactorUserInfoAsync();
        if (email == null)
        {
            TempData[TempDataKeys.Error] = "驗證階段已過期，請重新登入";
            return RedirectToAction(nameof(Login));
        }

        var (success, _, code) = await _userService.GenerateTwoFactorEmailCodeAsync();
        if (success && code != null)
        {
            await SendTwoFactorCodeEmailAsync(email, code);
            TempData[TempDataKeys.Success] = "驗證碼已重新寄送";
        }

        return RedirectToAction(nameof(LoginTwoFactor), new { rememberMe });
    }

    // ==================== 私有輔助方法 ====================

    /// <summary>
    /// 遮罩 Email（例如 te***@gmail.com）
    /// </summary>
    private static string MaskEmail(string email)
    {
        var parts = email.Split('@');
        if (parts.Length != 2 || parts[0].Length <= 2)
            return email;

        var name = parts[0];
        var masked = name[..2] + new string('*', Math.Min(name.Length - 2, 5));
        return $"{masked}@{parts[1]}";
    }

    /// <summary>
    /// 寄送 2FA 登入驗證碼 Email
    /// </summary>
    private async Task SendTwoFactorCodeEmailAsync(string email, string code)
    {
        await _emailService.SendEmailAsync(email, "MusicShop - 登入驗證碼",
            $@"<h3>登入驗證碼</h3>
            <p>您好，您正在登入 MusicShop。</p>
            <p>您的驗證碼為：<strong style='font-size: 1.5em; letter-spacing: 3px;'>{code}</strong></p>
            <p>此驗證碼將在數分鐘後失效。</p>
            <p>如果這不是您本人的操作，請立即更改密碼。</p>
            <p>── MusicShop 團隊</p>");
    }

    /// <summary>
    /// 使用 QRCoder 產生 QR Code 的 Base64 PNG 圖片
    /// </summary>
    private static string GenerateQrCodeBase64(string text)
    {
        using var qrGenerator = new QRCoder.QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(text, QRCoder.QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new QRCoder.PngByteQRCode(qrCodeData);
        var pngBytes = qrCode.GetGraphic(5);
        return Convert.ToBase64String(pngBytes);
    }

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
