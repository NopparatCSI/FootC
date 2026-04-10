namespace WebApp1.ViewModels;

public class AdminDashboardViewModel
{
    public string AdminName { get; set; } = "Admin";
    public string AdminRole { get; set; } = "Admin";

    public int TotalProducts { get; set; }
    public int ActiveProducts { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int PaidOrders { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public decimal RevenueLastMonth { get; set; }
    public int SelectedRangeDays { get; set; } = 30;
    public decimal RevenueInRange { get; set; }
    public decimal RevenueInPreviousRange { get; set; }

    public List<DashboardCountItemViewModel> OrderStatusBreakdown { get; set; } = [];
    public List<DashboardMonthlyRevenueItemViewModel> MonthlyRevenue { get; set; } = [];
    public List<DashboardCountItemViewModel> ProductByCategory { get; set; } = [];
    public List<DashboardDailyRevenueItemViewModel> DailyRevenue { get; set; } = [];
    public List<DashboardDailyStatusItemViewModel> DailyOrderStatus { get; set; } = [];

    public int FunnelCreatedOrders { get; set; }
    public int FunnelPaidOrders { get; set; }
    public int FunnelShippedOrders { get; set; }
    public int FunnelDeliveredOrders { get; set; }

    public decimal RevenueGrowthPercent
    {
        get
        {
            if (RevenueLastMonth <= 0)
            {
                return RevenueThisMonth > 0 ? 100m : 0m;
            }

            return ((RevenueThisMonth - RevenueLastMonth) / RevenueLastMonth) * 100m;
        }
    }

    public decimal RevenueRangeGrowthPercent
    {
        get
        {
            if (RevenueInPreviousRange <= 0)
            {
                return RevenueInRange > 0 ? 100m : 0m;
            }

            return ((RevenueInRange - RevenueInPreviousRange) / RevenueInPreviousRange) * 100m;
        }
    }
}

public class DashboardCountItemViewModel
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class DashboardMonthlyRevenueItemViewModel
{
    public string Label { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class DashboardDailyRevenueItemViewModel
{
    public string Label { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class DashboardDailyStatusItemViewModel
{
    public string Label { get; set; } = string.Empty;
    public int Pending { get; set; }
    public int Processing { get; set; }
    public int Shipped { get; set; }
    public int Delivered { get; set; }
    public int Cancelled { get; set; }
}