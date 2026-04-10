namespace WebApp1.ViewModels;

public class BannerSlotViewModel
{
    public string Key { get; set; } = "";
    public string Label { get; set; } = "";
    public bool SupportsMultiple { get; set; }
    public List<string> ImagePaths { get; set; } = new();
    public string? ImagePath => ImagePaths.FirstOrDefault();
}

public class BannerManagementViewModel
{
    public List<BannerSlotViewModel> SlideSlots { get; set; } = new();
    public List<BannerSlotViewModel> CategorySlots { get; set; } = new();
}
