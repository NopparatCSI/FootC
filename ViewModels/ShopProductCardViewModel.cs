namespace WebApp1.ViewModels;

public class ShopProductCardViewModel
{
    public int ProductId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string BrandName { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public string Gender { get; set; } = string.Empty;

    public string ShortDescription { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = "/images/Running-Shoe-2.jpeg";

    public decimal RegularPrice { get; set; }

    public decimal? SalePrice { get; set; }

    public decimal? DiscountPercent { get; set; }

    public List<string> Sizes { get; set; } = new();
}