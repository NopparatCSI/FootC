using System;
using System.Collections.Generic;

namespace WebApp1.Models.Db;

public partial class Order
{
    public int OrderId { get; set; }

    public string OrderNumber { get; set; } = null!;

    public int UserId { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal ShippingFee { get; set; }

    public decimal Discount { get; set; }

    public int? PromotionId { get; set; }

    public string? PromotionCode { get; set; }

    public decimal PromotionDiscountTotal { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string PaymentStatus { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? ShippingAddress { get; set; }

    public string? ShippingMethod { get; set; }

    public string? TrackingNumber { get; set; }

    public DateTime OrderDate { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public virtual Customer? Customer { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
