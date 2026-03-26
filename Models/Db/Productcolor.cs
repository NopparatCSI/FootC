using System;
using System.Collections.Generic;

namespace WebApp1.Models.Db;

public partial class ProductColor
{
    public int ColorId { get; set; }

    public int ProductId { get; set; }

    public string ColorName { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<ProductColorImage> ProductColorImages { get; set; } = new List<ProductColorImage>();

    public virtual ICollection<ProductSize> ProductSizes { get; set; } = new List<ProductSize>();
}
