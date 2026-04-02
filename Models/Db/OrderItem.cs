using System;

namespace WebApp1.Models.Db;

public partial class OrderItem
{
    public int OrderItemId { get; set; }

    public int OrderId { get; set; }

    public int SizeId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineTotal { get; set; } // STORED computed column

    // Navigation
    public virtual Order? Order { get; set; }

    public virtual ProductSize? ProductSize { get; set; }
}
