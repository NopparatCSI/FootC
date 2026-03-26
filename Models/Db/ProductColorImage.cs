using System;
using System.Collections.Generic;

namespace WebApp1.Models.Db;

public partial class ProductColorImage
{
    public int ColorImageId { get; set; }

    public int ColorId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public int SortOrder { get; set; }

    public virtual ProductColor Color { get; set; } = null!;
}
