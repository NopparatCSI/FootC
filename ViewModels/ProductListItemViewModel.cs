namespace WebApp1.ViewModels;

public class ProductListItemViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public decimal RegularPrice { get; set; }
    public decimal? SalePrice { get; set; }
    public string? UpdatedAt { get; set; }
}
