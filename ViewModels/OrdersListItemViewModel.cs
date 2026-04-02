namespace WebApp1.ViewModels
{
    public class OrdersListItemViewModel
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string CustomerEmail { get; set; } = "";
        public int ItemCount { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = "";
        public string PaymentStatus { get; set; } = ""; // Paid | Pending | Failed
        public string Status { get; set; } = "";        // Pending | Processing | Shipped | Delivered | Cancelled
        public string ShippingMethod { get; set; } = "";
        public string TrackingNumber { get; set; } = "";
        public DateTime OrderDate { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
