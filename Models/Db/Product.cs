using System;
using System.Collections.Generic;

namespace WebApp1.Models.Db;

public partial class Product
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public string Sku { get; set; } = null!;

    public int BrandId { get; set; }

    public int CategoryId { get; set; }

    public string Gender { get; set; } = null!;

    public string Status { get; set; } = null!;

    public decimal RegularPrice { get; set; }

    public decimal? SalePrice { get; set; }

    public decimal? DiscountPercent { get; set; }

    public decimal? CostPrice { get; set; }

    public string ShortDescription { get; set; } = null!;

    public string? FullDescription { get; set; }

    public string? HighlightFeatures { get; set; }

    public string? TechnicalDetails { get; set; }

    public bool ShowOnHomepage { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Brand Brand { get; set; } = null!;

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<ProductColor> ProductColors { get; set; } = new List<ProductColor>();

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
}
