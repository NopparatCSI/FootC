namespace WebApp1.ViewModels;

public class HomeIndexViewModel
{
    public Dictionary<string, List<string>> SlideBanners { get; set; } = new();

    public Dictionary<string, string?> CategoryBanners { get; set; } = new();

    public List<HomeIndexProductCardViewModel> PopularProducts { get; set; } = new();

    public List<HomeIndexProductCardViewModel> SaleProducts { get; set; } = new();
}

public class HomeIndexProductCardViewModel
{
    public int ProductId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Brand { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = "/images/Running-Shoe-2.jpeg";

    public decimal RegularPrice { get; set; }

    public decimal? SalePrice { get; set; }

    public decimal? DiscountPercent { get; set; }

    public bool IsOnSale => SalePrice.HasValue && SalePrice.Value > 0 && SalePrice.Value < RegularPrice;

    public decimal DisplayPrice => IsOnSale ? SalePrice!.Value : RegularPrice;
}
