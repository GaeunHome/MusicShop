using MusicShop.Library.Helpers;

namespace MusicShop.Web.Infrastructure.Implementation;

/// <summary>
/// 驗證碼服務實作
/// 使用 CaptchaGenerator 產生圖片，透過 Session 儲存驗證碼答案
/// </summary>
public class CaptchaService : ICaptchaService
{
    private const string SessionKey = "CaptchaCode";

    public byte[] GenerateCaptchaImage(ISession session)
    {
        var (code, imageBytes) = CaptchaGenerator.GenerateCaptcha();
        session.SetString(SessionKey, code);
        return imageBytes;
    }

    public bool ValidateCaptcha(ISession session, string userInput)
    {
        var expected = session.GetString(SessionKey);
        session.Remove(SessionKey); // 使用後立即清除，防止重放攻擊

        if (string.IsNullOrEmpty(expected) || string.IsNullOrEmpty(userInput))
            return false;

        return string.Equals(expected, userInput.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
