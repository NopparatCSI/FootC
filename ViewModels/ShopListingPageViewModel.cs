namespace WebApp1.ViewModels;

public class ShopListingPageViewModel
{
    public string Eyebrow { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string EmptyTitle { get; set; } = string.Empty;

    public string EmptyDescription { get; set; } = string.Empty;

    public string? SearchQuery { get; set; }

    public List<string> AvailableCategories { get; set; } = new();

    public List<string> AvailableGenders { get; set; } = new();

    public List<string> AvailableBrands { get; set; } = new();

    public List<string> AvailableSizes { get; set; } = new();

    public List<ShopProductCardViewModel> Products { get; set; } = new();
}