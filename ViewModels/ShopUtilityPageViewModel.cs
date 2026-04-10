namespace WebApp1.ViewModels;

public class ShopUtilityPageViewModel
{
    public string Eyebrow { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string ActionText { get; set; } = string.Empty;

    public string ActionController { get; set; } = "Shop";

    public string ActionName { get; set; } = "NewArrivals";

    public List<ShopProductCardViewModel> RecommendedProducts { get; set; } = new();
}