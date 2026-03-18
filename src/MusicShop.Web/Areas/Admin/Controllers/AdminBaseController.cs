using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicShop.Controllers;

namespace MusicShop.Web.Areas.Admin.Controllers;

/// <summary>
/// 後台管理 Controller 基底類別
/// 繼承 BaseController（共用 GetCurrentUserId / GetAuthorizedUserId），
/// 並統一設定 Area 與權限驗證，所有後台 Controller 繼承此類別即可。
/// </summary>
[Area("Admin")]
[Authorize(Roles = "Admin")]
public abstract class AdminBaseController : BaseController
{
}
