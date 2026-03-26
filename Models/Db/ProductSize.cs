using System;
using System.Collections.Generic;

namespace WebApp1.Models.Db;

public partial class ProductSize
{
    public int SizeId { get; set; }

    public int ColorId { get; set; }

    public string SizeType { get; set; } = null!;

    public string SizeValue { get; set; } = null!;

    public int StockQuantity { get; set; }

    public string Status { get; set; } = null!;

    public virtual ProductColor Color { get; set; } = null!;
}
