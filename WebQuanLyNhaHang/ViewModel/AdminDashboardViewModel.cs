using System;
using System.Collections.Generic;

namespace WebQuanLyNhaHang.ViewModel;

public class AdminDashboardViewModel
{
    public string AdminDisplayName { get; set; } = "Admin";

    public string AdminAccount { get; set; } = "admin";

    public string AdminRoleLabel { get; set; } = "Quan tri he thong";

    public string Initials { get; set; } = "AD";

    public string SnapshotLabel { get; set; } = string.Empty;

    public DashboardMetricCard RevenueMetric { get; set; } = new();

    public DashboardMetricCard OrderMetric { get; set; } = new();

    public DashboardMetricCard CustomerMetric { get; set; } = new();

    public DashboardMetricCard ProductMetric { get; set; } = new();

    public List<DashboardInsightCard> InsightCards { get; set; } = new();

    public string DashboardPayloadJson { get; set; } = "{}";
}

public class DashboardMetricCard
{
    public string Label { get; set; } = string.Empty;

    public string Value { get; set; } = "0";

    public string TrendText { get; set; } = string.Empty;

    public string ComparisonText { get; set; } = string.Empty;

    public string TrendCssClass { get; set; } = "is-neutral";

    public string HintText { get; set; } = string.Empty;
}

public class DashboardInsightCard
{
    public string Label { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public string Note { get; set; } = string.Empty;

    public string ToneCssClass { get; set; } = string.Empty;
}

public class DashboardPayload
{
    public string SnapshotLabel { get; set; } = string.Empty;

    public Dictionary<string, DashboardValueSeries> RevenueSeries { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, DashboardCountSeries> OrderSeries { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, DashboardComparisonSeries> RevenueComparisonSeries { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, List<DashboardTopProductItem>> TopProducts { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public class DashboardValueSeries
{
    public List<string> Labels { get; set; } = new();

    public List<decimal> Values { get; set; } = new();

    public string TotalLabel { get; set; } = string.Empty;

    public string AverageLabel { get; set; } = string.Empty;

    public string PeakLabel { get; set; } = string.Empty;
}

public class DashboardCountSeries
{
    public List<string> Labels { get; set; } = new();

    public List<int> Values { get; set; } = new();

    public string TotalLabel { get; set; } = string.Empty;

    public string AverageLabel { get; set; } = string.Empty;

    public string PeakLabel { get; set; } = string.Empty;
}

public class DashboardComparisonSeries
{
    public List<string> Labels { get; set; } = new();

    public List<decimal> DineInValues { get; set; } = new();

    public List<decimal> TakeAwayValues { get; set; } = new();

    public string DineInLabel { get; set; } = string.Empty;

    public string TakeAwayLabel { get; set; } = string.Empty;

    public string TotalLabel { get; set; } = string.Empty;
}

public class DashboardTopProductItem
{
    public string Name { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal Revenue { get; set; }

    public string ShareLabel { get; set; } = string.Empty;
}
