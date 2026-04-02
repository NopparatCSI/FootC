namespace WebApp1.ViewModels
{
    public class OrderStatusViewModel
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = "";
        public string Status { get; set; } = "";        // Pending | Processing | Shipped | Delivered | Cancelled
        public string PaymentStatus { get; set; } = ""; // Paid | Pending | Failed
        public string PaymentMethod { get; set; } = "";
        public DateTime OrderDate { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Customer
        public string CustomerName { get; set; } = "";
        public string CustomerEmail { get; set; } = "";
        public string CustomerPhone { get; set; } = "";

        // Shipping
        public string ShippingAddress { get; set; } = "";
        public string ShippingMethod { get; set; } = "";
        public string TrackingNumber { get; set; } = "";

        // Items
        public List<OrderItemViewModel> Items { get; set; } = new();

        // Timeline
        public List<OrderTimelineStep> Timeline { get; set; } = new();

        public decimal ShippingFee { get; set; }
        public decimal Discount { get; set; }
        public decimal Subtotal => Items.Sum(i => i.LineTotal);
        public decimal GrandTotal => Subtotal + ShippingFee - Discount;
    }

    public class OrderItemViewModel
    {
        public string ProductName { get; set; } = "";
        public string Sku { get; set; } = "";
        public string Color { get; set; } = "";
        public string Size { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal => Quantity * UnitPrice;
        public string? ImageUrl { get; set; }
    }

    public class OrderTimelineStep
    {
        public string StepName { get; set; } = "";
        public string Icon { get; set; } = "";
        public DateTime? CompletedAt { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsCurrent { get; set; }
        public bool IsCancelled { get; set; }
    }
}
