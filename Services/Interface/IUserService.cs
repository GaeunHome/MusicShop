using MusicShop.ViewModels;

namespace MusicShop.Services.Interface;

/// <summary>
/// 使用者服務介面
/// 負責使用者相關的商業邏輯
/// </summary>
public interface IUserService
{
    /// <summary>
    /// 取得所有使用者及其角色資訊
    /// </summary>
    /// <returns>使用者管理 ViewModel 列表</returns>
    Task<List<UserManagementViewModel>> GetAllUsersWithRolesAsync();

    /// <summary>
    /// 切換使用者的管理員角色（指派或移除）
    /// </summary>
    /// <param name="userId">使用者 ID</param>
    /// <param name="currentAdminId">當前管理員 ID（防止移除自己的權限）</param>
    /// <returns>操作結果訊息</returns>
    Task<(bool Success, string Message)> ToggleAdminRoleAsync(string userId, string currentAdminId);
}
