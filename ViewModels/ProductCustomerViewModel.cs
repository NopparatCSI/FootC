namespace WebApp1.ViewModels;

public class ProductCustomerViewModel
{
    public int Id { get; set; }

    public bool IsCustomerLoggedIn { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Brand { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Gender { get; set; } = string.Empty;

    public decimal RegularPrice { get; set; }

    public decimal? SalePrice { get; set; }

    public decimal? DiscountPercent { get; set; }

    public string ShortDescription { get; set; } = string.Empty;

    public string? FullDescription { get; set; }

    public string? HighlightFeaturesHtml { get; set; }

    public string? TechnicalDetails { get; set; }

    // Images are not yet persisted in DB; keep placeholders for now.
    public string? ThumbnailUrl { get; set; }

    public List<string> GalleryUrls { get; set; } = new();

    public List<string> ColorNames { get; set; } = new();
}
