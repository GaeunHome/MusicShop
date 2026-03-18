namespace MusicShop.Data.Entities;

/// <summary>
/// 軟刪除介面：實作此介面的實體在刪除時不會從資料庫移除，
/// 而是標記 IsDeleted = true 並記錄刪除時間，保留資料完整性。
/// 搭配 EF Core Global Query Filter 自動過濾已刪除的資料。
/// </summary>
public interface ISoftDeletable
{
    /// <summary>
    /// 是否已軟刪除
    /// </summary>
    bool IsDeleted { get; set; }

    /// <summary>
    /// 軟刪除時間（UTC）
    /// </summary>
    DateTime? DeletedAt { get; set; }
}
