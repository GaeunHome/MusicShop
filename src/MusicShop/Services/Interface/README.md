# Services - Interface

此資料夾包含所有服務層的介面定義（Business Logic Layer Interfaces）。

## 架構說明

本專案採用**三層式架構（Three-Tier Architecture）**：
- **展示層（Presentation）**：Controllers + Views
- **商業邏輯層（Business Logic）**：Services ⬅️ **此資料夾**
- **資料存取層（Data Access）**：Repositories

## 介面與實作分離

所有服務介面定義於此資料夾，實作於 `Services/Implementation/` 資料夾。

這種設計模式提供：
- ✅ **低耦合**：依賴介面而非具體實作
- ✅ **可測試性**：可輕鬆使用 Mock 物件進行單元測試
- ✅ **可維護性**：修改實作時不影響介面契約
- ✅ **依賴注入**：透過 DI 容器注入實作

## 服務列表

| 介面 | 說明 | 實作位置 |
|------|------|----------|
| `IAlbumService` | 專輯業務邏輯 | `Implementation/AlbumService.cs` |
| `IArtistService` | 藝人業務邏輯 | `Implementation/ArtistService.cs` |
| `IArtistCategoryService` | 藝人分類業務邏輯 | `Implementation/ArtistCategoryService.cs` |
| `IProductTypeService` | 商品類型業務邏輯 | `Implementation/ProductTypeService.cs` |
| `ICartService` | 購物車業務邏輯 | `Implementation/CartService.cs` |
| `IOrderService` | 訂單業務邏輯 | `Implementation/OrderService.cs` |
| `IOrderValidationService` | 訂單驗證邏輯 | `Implementation/OrderValidationService.cs` |
| `IUserService` | 使用者管理業務邏輯 | `Implementation/UserService.cs` |
| `IStatisticsService` | 統計資訊業務邏輯 | `Implementation/StatisticsService.cs` |

## 設計原則

1. **單一職責原則（SRP）**：每個服務只負責一個業務領域
2. **介面隔離原則（ISP）**：介面設計精簡，不包含不必要的方法
3. **依賴反轉原則（DIP）**：Controller 依賴介面，不依賴具體實作

## 使用範例

```csharp
// Controller 中透過建構子注入服務介面
public class AlbumController : Controller
{
    private readonly IAlbumService _albumService;

    public AlbumController(IAlbumService albumService)
    {
        _albumService = albumService;
    }

    public async Task<IActionResult> Index()
    {
        var albums = await _albumService.GetAlbumsAsync();
        return View(albums);
    }
}
```

## 注意事項

- ⚠️ **介面不應包含業務邏輯**：只定義方法簽章，不包含實作
- ⚠️ **保持向後相容**：修改介面時需考慮現有實作的影響
- ✅ **使用 async/await**：所有 I/O 操作方法應為非同步
