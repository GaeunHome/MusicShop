// ─────────────────────────────────────────────────────────────
// AdminController.User.cs - User management partial class
// Responsibility: User listing and admin role toggle actions
// ─────────────────────────────────────────────────────────────

using Microsoft.AspNetCore.Mvc;
using MusicShop.Web.Infrastructure;
using System.Security.Claims;

namespace MusicShop.Controllers
{
    public partial class AdminController
    {
        // ─── 使用者管理 ───────────────────────────────────────

        /// <summary>
        /// 使用者管理頁面 - 顯示所有使用者和其角色
        /// </summary>
        public async Task<IActionResult> Users()
        {
            // 透過 Service 層取得所有使用者及其角色資訊
            var userViewModels = await _userService.GetAllUsersWithRolesAsync();

            return View("User/Index", userViewModels);
        }

        /// <summary>
        /// 切換使用者的管理員角色（指派或移除）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAdminRole(string userId)
        {
            // 取得當前管理員的 ID
            var currentAdminId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentAdminId))
            {
                TempData[TempDataKeys.Error] = "無法取得當前使用者資訊";
                return RedirectToAction(nameof(Users));
            }

            // 透過 Service 層執行角色切換
            var (success, message) = await _userService.ToggleAdminRoleAsync(userId, currentAdminId);

            if (success)
            {
                TempData[TempDataKeys.Success] = message;
            }
            else
            {
                TempData[TempDataKeys.Error] = message;
            }

            return RedirectToAction(nameof(Users));
        }
    }
}
