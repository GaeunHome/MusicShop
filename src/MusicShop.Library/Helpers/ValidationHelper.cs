namespace MusicShop.Library.Helpers;

/// <summary>
/// 共用的驗證工具類，提供統一的驗證方法
/// </summary>
public static class ValidationHelper
{
    /// <summary>
    /// 驗證字串不為空
    /// </summary>
    /// <param name="value">要驗證的字串</param>
    /// <param name="fieldName">欄位名稱（用於錯誤訊息）</param>
    /// <param name="paramName">參數名稱（用於 ArgumentException）</param>
    /// <exception cref="ArgumentException">當字串為 null、空字串或僅包含空白字元時拋出</exception>
    public static void ValidateNotEmpty(string? value, string fieldName, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{fieldName}不能為空", paramName);
        }
    }

    /// <summary>
    /// 驗證字串長度不超過指定上限
    /// </summary>
    /// <param name="value">要驗證的字串</param>
    /// <param name="fieldName">欄位名稱（用於錯誤訊息）</param>
    /// <param name="maxLength">最大長度</param>
    /// <param name="paramName">參數名稱（用於 ArgumentException）</param>
    /// <exception cref="ArgumentException">當字串長度超過 maxLength 時拋出</exception>
    public static void ValidateMaxLength(string? value, string fieldName, int maxLength, string paramName)
    {
        if (value != null && value.Length > maxLength)
        {
            throw new ArgumentException($"{fieldName}不能超過 {maxLength} 個字元", paramName);
        }
    }

    /// <summary>
    /// 驗證字串不為空且長度不超過指定上限（組合驗證）
    /// </summary>
    /// <param name="value">要驗證的字串</param>
    /// <param name="fieldName">欄位名稱（用於錯誤訊息）</param>
    /// <param name="maxLength">最大長度</param>
    /// <param name="paramName">參數名稱（用於 ArgumentException）</param>
    /// <exception cref="ArgumentException">當字串為空或長度超過 maxLength 時拋出</exception>
    public static void ValidateString(string? value, string fieldName, int maxLength, string paramName)
    {
        ValidateNotEmpty(value, fieldName, paramName);
        ValidateMaxLength(value!, fieldName, maxLength, paramName);
    }

    /// <summary>
    /// 驗證數值大於零
    /// </summary>
    /// <param name="value">要驗證的數值</param>
    /// <param name="fieldName">欄位名稱（用於錯誤訊息）</param>
    /// <param name="paramName">參數名稱（用於 ArgumentException）</param>
    /// <exception cref="ArgumentException">當數值小於或等於 0 時拋出</exception>
    public static void ValidatePositive(decimal value, string fieldName, string paramName)
    {
        if (value <= 0)
        {
            throw new ArgumentException($"{fieldName}必須大於 0", paramName);
        }
    }

    /// <summary>
    /// 驗證整數大於零
    /// </summary>
    /// <param name="value">要驗證的整數</param>
    /// <param name="fieldName">欄位名稱（用於錯誤訊息）</param>
    /// <param name="paramName">參數名稱（用於 ArgumentException）</param>
    /// <exception cref="ArgumentException">當整數小於或等於 0 時拋出</exception>
    public static void ValidatePositive(int value, string fieldName, string paramName)
    {
        if (value <= 0)
        {
            throw new ArgumentException($"{fieldName}必須大於 0", paramName);
        }
    }

    /// <summary>
    /// 驗證 ID 有效（大於 0）
    /// </summary>
    /// <param name="id">要驗證的 ID</param>
    /// <param name="fieldName">欄位名稱（用於錯誤訊息）</param>
    /// <param name="paramName">參數名稱（用於 ArgumentException）</param>
    /// <exception cref="ArgumentException">當 ID 小於或等於 0 時拋出</exception>
    public static void ValidateId(int id, string fieldName, string paramName)
    {
        if (id <= 0)
        {
            throw new ArgumentException($"{fieldName}必須為有效的 ID（大於 0）", paramName);
        }
    }

    /// <summary>
    /// 驗證實體存在
    /// </summary>
    /// <typeparam name="T">實體類型</typeparam>
    /// <param name="entity">要驗證的實體</param>
    /// <param name="entityName">實體名稱（用於錯誤訊息）</param>
    /// <param name="id">實體 ID（用於錯誤訊息）</param>
    /// <exception cref="InvalidOperationException">當實體為 null 時拋出</exception>
    public static void ValidateEntityExists<T>(T? entity, string entityName, int id) where T : class
    {
        if (entity == null)
        {
            throw new InvalidOperationException($"找不到 ID 為 {id} 的{entityName}");
        }
    }

    /// <summary>
    /// 驗證布林值為 true
    /// </summary>
    /// <param name="condition">要驗證的條件</param>
    /// <param name="errorMessage">錯誤訊息</param>
    /// <exception cref="InvalidOperationException">當條件為 false 時拋出</exception>
    public static void ValidateCondition(bool condition, string errorMessage)
    {
        if (!condition)
        {
            throw new InvalidOperationException(errorMessage);
        }
    }

    /// <summary>
    /// 驗證集合不為空
    /// </summary>
    /// <typeparam name="T">集合元素類型</typeparam>
    /// <param name="collection">要驗證的集合</param>
    /// <param name="fieldName">欄位名稱（用於錯誤訊息）</param>
    /// <exception cref="InvalidOperationException">當集合為 null 或空時拋出</exception>
    public static void ValidateCollectionNotEmpty<T>(IEnumerable<T>? collection, string fieldName)
    {
        if (collection == null || !collection.Any())
        {
            throw new InvalidOperationException($"{fieldName}不能為空");
        }
    }
}
