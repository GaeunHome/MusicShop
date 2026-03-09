# Repositories - Interface

此資料夾包含所有資料存取層的介面定義（Data Access Layer Interfaces）。

## 架構說明

本專案採用**三層式架構（Three-Tier Architecture）**：
- **展示層（Presentation）**：Controllers + Views
- **商業邏輯層（Business Logic）**：Services
- **資料存取層（Data Access）**：Repositories ⬅️ **此資料夾**

## Repository 模式

Repository 模式將資料存取邏輯封裝起來，提供統一的資料操作介面。

### 優點
- ✅ **抽象化資料存取**：隱藏 EF Core 實作細節
- ✅ **可測試性**：可使用 Mock Repository 進行單元測試
- ✅ **集中管理**：統一的資料存取邏輯，避免重複程式碼
- ✅ **可替換性**：未來可輕鬆更換 ORM 或資料庫

## 介面與實作分離

所有 Repository 介面定義於此資料夾，實作於 `Repositories/Implementation/` 資料夾。

## Repository 列表

| 介面 | 說明 | 實作位置 |
|------|------|----------|
| `IAlbumRepository` | 專輯資料存取 | `Implementation/AlbumRepository.cs` |
| `IArtistRepository` | 藝人資料存取 | `Implementation/ArtistRepository.cs` |
| `IArtistCategoryRepository` | 藝人分類資料存取 | `Implementation/ArtistCategoryRepository.cs` |
| `IProductTypeRepository` | 商品類型資料存取 | `Implementation/ProductTypeRepository.cs` |
| `ICartRepository` | 購物車資料存取 | `Implementation/CartRepository.cs` |
| `IOrderRepository` | 訂單資料存取 | `Implementation/OrderRepository.cs` |
| `IStatisticsRepository` | 統計資料存取 | `Implementation/StatisticsRepository.cs` |

## 設計原則

1. **僅負責資料存取**：不包含業務邏輯驗證
2. **使用 EF Core**：透過 `DbContext` 進行資料庫操作
3. **IDbContextFactory 模式**：使用工廠模式建立 DbContext，避免並行問題
4. **Include/ThenInclude**：使用預先載入（Eager Loading）避免 N+1 查詢

## 使用範例

```csharp
// Service 中透過建構子注入 Repository 介面
public class AlbumService : IAlbumService
{
    private readonly IAlbumRepository _albumRepository;

    public AlbumService(IAlbumRepository albumRepository)
    {
        _albumRepository = albumRepository;
    }

    public async Task<IEnumerable<Album>> GetAlbumsAsync()
    {
        // Repository 負責資料存取
        return await _albumRepository.GetAllAlbumsAsync();
    }
}
```

## 常用查詢模式

### 預先載入（Eager Loading）
```csharp
return await context.Albums
    .Include(a => a.Artist)
        .ThenInclude(ar => ar.ArtistCategory)
    .Include(a => a.ProductType)
    .ToListAsync();
```

### 篩選查詢
```csharp
return await context.Albums
    .Where(a => a.Price > 100)
    .OrderBy(a => a.CreatedAt)
    .ToListAsync();
```

### IDbContextFactory 使用
```csharp
public async Task<Album?> GetAlbumByIdAsync(int id)
{
    await using var context = await _contextFactory.CreateDbContextAsync();
    return await context.Albums.FindAsync(id);
}
```

## 注意事項

- ⚠️ **不應包含業務邏輯**：驗證、計算等邏輯應在 Service 層
- ⚠️ **避免 N+1 查詢**：適當使用 `.Include()` 預先載入關聯資料
- ✅ **使用 async/await**：所有資料庫操作應為非同步
- ✅ **使用 UTC 時間**：統一使用 `DateTime.UtcNow`
