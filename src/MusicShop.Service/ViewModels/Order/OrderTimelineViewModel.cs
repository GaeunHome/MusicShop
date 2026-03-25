using MusicShop.Library.Enums;

namespace MusicShop.Service.ViewModels.Order;

/// <summary>
/// 訂單時間軸 ViewModel
/// 將時間軸的業務邏輯（進度計算、步驟狀態判斷）從 View 搬至此處，
/// View 只負責顯示預先計算好的結果。
/// </summary>
public class OrderTimelineViewModel
{
    /// <summary>
    /// 是否為已取消狀態
    /// </summary>
    public bool IsCancelled { get; set; }

    /// <summary>
    /// 進度條寬度百分比（0-100）
    /// </summary>
    public int ProgressPercent { get; set; }

    /// <summary>
    /// 時間軸步驟列表（含預先計算好的 CSS class 和 icon）
    /// </summary>
    public List<OrderTimelineStepViewModel> Steps { get; set; } = new();

    /// <summary>
    /// 正常流程的步驟定義
    /// </summary>
    private static readonly (OrderStatus Status, string Icon, string Label, string Desc)[] DefaultSteps =
    {
        (OrderStatus.Pending,   "bi-clipboard-check", "待處理", "訂單已建立"),
        (OrderStatus.Paid,      "bi-credit-card",     "已付款", "款項已確認"),
        (OrderStatus.Shipped,   "bi-truck",           "已出貨", "商品已寄出"),
        (OrderStatus.Completed, "bi-check-circle",    "已完成", "交易完成")
    };

    /// <summary>
    /// 根據訂單狀態建立時間軸 ViewModel。
    /// 封裝進度百分比計算與步驟 CSS class 判斷邏輯。
    /// </summary>
    public static OrderTimelineViewModel Create(OrderStatus status)
    {
        var isCancelled = status == OrderStatus.Cancelled;
        var currentIndex = isCancelled
            ? -1
            : Array.FindIndex(DefaultSteps, s => s.Status == status);

        // 計算進度條寬度（每個步驟佔 1/(n-1) 的進度）
        var progressPercent = 0;
        if (!isCancelled && currentIndex > 0)
        {
            progressPercent = (int)Math.Round((double)currentIndex / (DefaultSteps.Length - 1) * 100);
        }

        // 建立步驟列表，預先計算每個步驟的 CSS class 和 icon
        var steps = new List<OrderTimelineStepViewModel>();
        for (var i = 0; i < DefaultSteps.Length; i++)
        {
            var step = DefaultSteps[i];
            string stepClass;
            string iconClass;

            if (isCancelled)
            {
                stepClass = "future";
                iconClass = step.Icon;
            }
            else if (i < currentIndex)
            {
                stepClass = "completed";
                iconClass = "bi-check-lg";
            }
            else if (i == currentIndex)
            {
                stepClass = "current";
                iconClass = step.Icon;
            }
            else
            {
                stepClass = "future";
                iconClass = step.Icon;
            }

            steps.Add(new OrderTimelineStepViewModel
            {
                IconClass = iconClass,
                Label = step.Label,
                Description = step.Desc,
                StepClass = stepClass
            });
        }

        // 取消狀態額外加上取消步驟
        if (isCancelled)
        {
            steps.Add(new OrderTimelineStepViewModel
            {
                IconClass = "bi-x-lg",
                Label = "已取消",
                Description = "訂單已取消",
                StepClass = "cancelled"
            });
        }

        return new OrderTimelineViewModel
        {
            IsCancelled = isCancelled,
            ProgressPercent = progressPercent,
            Steps = steps
        };
    }
}

/// <summary>
/// 時間軸單一步驟 ViewModel
/// </summary>
public class OrderTimelineStepViewModel
{
    /// <summary>
    /// Bootstrap Icon class（如 bi-clipboard-check、bi-check-lg）
    /// </summary>
    public string IconClass { get; set; } = string.Empty;

    /// <summary>
    /// 步驟標籤（如「待處理」、「已付款」）
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// 步驟描述（如「訂單已建立」）
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 步驟的 CSS class（completed / current / future / cancelled）
    /// </summary>
    public string StepClass { get; set; } = string.Empty;
}
