namespace WebApp1.ViewModels
{
    public class OrdersListViewModel
    {
        public List<OrdersListItemViewModel> Orders { get; set; } = new();

        public int TotalOrders    => Orders.Count;
        public int PendingCount   => Orders.Count(o => o.Status == "Pending");
        public int ProcessingCount=> Orders.Count(o => o.Status == "Processing");
        public int ShippedCount   => Orders.Count(o => o.Status == "Shipped");
        public int DeliveredCount => Orders.Count(o => o.Status == "Delivered");
        public int CancelledCount => Orders.Count(o => o.Status == "Cancelled");
        public decimal TotalRevenue => Orders
            .Where(o => o.Status != "Cancelled" && o.PaymentStatus == "Paid")
            .Sum(o => o.TotalAmount);
    }
}
