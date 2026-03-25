/**
 * 後台 Dashboard 圖表
 * 使用 Chart.js 繪製銷售趨勢折線圖與熱門商品排行長條圖
 */

let salesChart = null;
let topAlbumsChart = null;

/**
 * 載入銷售趨勢圖表
 */
async function loadSalesTrend(days) {
    try {
        const response = await fetch(`/Admin/Dashboard/SalesTrend?days=${days}`);
        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }
        const data = await response.json();

        const ctx = document.getElementById('salesTrendChart');
        if (!ctx) return;

        if (salesChart) salesChart.destroy();

        salesChart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: data.labels,
                datasets: [
                    {
                        label: '銷售額 (NT$)',
                        data: data.amounts,
                        borderColor: '#dc3545',
                        backgroundColor: 'rgba(220, 53, 69, 0.1)',
                        fill: true,
                        tension: 0.3,
                        yAxisID: 'y'
                    },
                    {
                        label: '訂單數',
                        data: data.counts,
                        borderColor: '#0d6efd',
                        backgroundColor: 'rgba(13, 110, 253, 0.1)',
                        fill: false,
                        tension: 0.3,
                        yAxisID: 'y1'
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: {
                    mode: 'index',
                    intersect: false
                },
                scales: {
                    y: {
                        type: 'linear',
                        display: true,
                        position: 'left',
                        title: { display: true, text: '銷售額 (NT$)' },
                        ticks: {
                            callback: value => 'NT$ ' + value.toLocaleString()
                        }
                    },
                    y1: {
                        type: 'linear',
                        display: true,
                        position: 'right',
                        title: { display: true, text: '訂單數' },
                        grid: { drawOnChartArea: false },
                        ticks: {
                            stepSize: 1
                        }
                    }
                },
                plugins: {
                    tooltip: {
                        callbacks: {
                            label: function (context) {
                                if (context.datasetIndex === 0) {
                                    return `銷售額：NT$ ${context.raw.toLocaleString()}`;
                                }
                                return `訂單數：${context.raw}`;
                            }
                        }
                    }
                }
            }
        });
    } catch (error) {
        console.error('載入銷售趨勢圖表失敗：', error);
        const ctx = document.getElementById('salesTrendChart');
        if (ctx) {
            const container = ctx.closest('.card-body') || ctx.parentElement;
            container.innerHTML = '<div class="text-center text-muted py-4">' +
                '<i class="bi bi-exclamation-triangle fs-3 d-block mb-2"></i>' +
                '無法載入銷售趨勢資料，請稍後重試。</div>';
        }
    }
}

/**
 * 載入熱門商品排行圖表
 */
async function loadTopAlbums() {
    try {
        const response = await fetch('/Admin/Dashboard/TopSellingAlbums?count=10');
        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }
        const data = await response.json();

        const ctx = document.getElementById('topAlbumsChart');
        if (!ctx) return;

        if (topAlbumsChart) topAlbumsChart.destroy();

        // 截斷過長的標題
        const truncatedLabels = data.labels.map(label =>
            label.length > 12 ? label.substring(0, 12) + '...' : label
        );

        topAlbumsChart = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: truncatedLabels,
                datasets: [{
                    label: '銷售數量',
                    data: data.quantities,
                    backgroundColor: [
                        '#dc3545', '#fd7e14', '#ffc107', '#198754', '#0dcaf0',
                        '#0d6efd', '#6610f2', '#d63384', '#6c757d', '#20c997'
                    ]
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                indexAxis: 'y',
                scales: {
                    x: {
                        title: { display: true, text: '銷售數量' },
                        ticks: { stepSize: 1 }
                    }
                },
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        callbacks: {
                            title: function (tooltipItems) {
                                // 顯示完整標題
                                return data.labels[tooltipItems[0].dataIndex];
                            }
                        }
                    }
                }
            }
        });
    } catch (error) {
        console.error('載入熱門商品排行圖表失敗：', error);
        const ctx = document.getElementById('topAlbumsChart');
        if (ctx) {
            const container = ctx.closest('.card-body') || ctx.parentElement;
            container.innerHTML = '<div class="text-center text-muted py-4">' +
                '<i class="bi bi-exclamation-triangle fs-3 d-block mb-2"></i>' +
                '無法載入熱門商品資料，請稍後重試。</div>';
        }
    }
}

// 頁面載入時初始化圖表
document.addEventListener('DOMContentLoaded', () => {
    loadSalesTrend(30);
    loadTopAlbums();

    // 天數切換
    const daysSelect = document.getElementById('salesTrendDays');
    if (daysSelect) {
        daysSelect.addEventListener('change', (e) => {
            loadSalesTrend(parseInt(e.target.value));
        });
    }
});
