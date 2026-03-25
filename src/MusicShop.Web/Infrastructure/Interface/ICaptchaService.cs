namespace MusicShop.Web.Infrastructure;

/// <summary>
/// 驗證碼服務介面（Web 層基礎設施）
/// 依賴 Session 儲存驗證碼答案，屬於 Web 層關注點
/// </summary>
public interface ICaptchaService
{
    /// <summary>
    /// 產生驗證碼圖片並將答案存入 Session
    /// </summary>
    byte[] GenerateCaptchaImage(ISession session);

    /// <summary>
    /// 驗證使用者輸入的驗證碼（不區分大小寫），驗證後清除 Session 防止重放攻擊
    /// </summary>
    bool ValidateCaptcha(ISession session, string userInput);
}
