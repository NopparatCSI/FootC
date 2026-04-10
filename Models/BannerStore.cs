using System.Text.Json;

namespace WebApp1.Models;

public static class BannerStore
{
    public static readonly string[] SlideSlotKeys =
    [
        "banner-slide-1",
        "banner-slide-2"
    ];

    public static readonly string[] SingleSlotKeys =
    [
        "category-new-arrivals",
        "category-sales",
        "category-running",
        "category-training"
    ];

    public static readonly string[] SlotKeys =
    [
        "banner-slide-1",
        "banner-slide-2",
        "category-new-arrivals",
        "category-sales",
        "category-running",
        "category-training"
    ];

    public static readonly Dictionary<string, string> SlotLabels = new()
    {
        ["banner-slide-1"]          = "Banner Slide 1 (ขนาดใหญ่)",
        ["banner-slide-2"]          = "Banner Slide 2 (ขนาดกลาง)",
        ["category-new-arrivals"]   = "New Arrivals",
        ["category-sales"]          = "Sales",
        ["category-running"]        = "Running Shoes",
        ["category-training"]       = "Training Shoes"
    };

    public sealed class BannerStoreData
    {
        public Dictionary<string, List<string>> SlideImages { get; set; } = new();
        public Dictionary<string, string?> SingleImages { get; set; } = new();
    }

    private static string GetFilePath(string contentRootPath) =>
        Path.Combine(contentRootPath, "App_Data", "banners.json");

    public static BannerStoreData Load(string contentRootPath)
    {
        var path = GetFilePath(contentRootPath);
        var result = CreateEmptyData();

        if (!File.Exists(path))
            return result;

        try
        {
            var json = File.ReadAllText(path);
            var newData = JsonSerializer.Deserialize<BannerStoreData>(json);
            if (newData != null)
            {
                foreach (var key in SlideSlotKeys)
                {
                    if (newData.SlideImages.TryGetValue(key, out var images) && images != null)
                    {
                        result.SlideImages[key] = images.Where(v => !string.IsNullOrWhiteSpace(v)).ToList();
                    }
                }

                foreach (var key in SingleSlotKeys)
                {
                    if (newData.SingleImages.TryGetValue(key, out var image))
                    {
                        result.SingleImages[key] = string.IsNullOrWhiteSpace(image) ? null : image;
                    }
                }

                return result;
            }
        }
        catch
        {
            // Fallback to old format below.
        }

        try
        {
            var json = File.ReadAllText(path);
            var legacy = JsonSerializer.Deserialize<Dictionary<string, string?>>(json);
            if (legacy != null)
            {
                foreach (var key in SlideSlotKeys)
                {
                    if (legacy.TryGetValue(key, out var image) && !string.IsNullOrWhiteSpace(image))
                    {
                        result.SlideImages[key] = [image];
                    }
                }

                foreach (var key in SingleSlotKeys)
                {
                    if (legacy.TryGetValue(key, out var image))
                    {
                        result.SingleImages[key] = string.IsNullOrWhiteSpace(image) ? null : image;
                    }
                }
            }
        }
        catch
        {
            // Return empty slots on corrupt file.
        }

        return result;
    }

    public static void Save(string contentRootPath, BannerStoreData data)
    {
        var dir = Path.Combine(contentRootPath, "App_Data");
        Directory.CreateDirectory(dir);
        var path = GetFilePath(contentRootPath);
        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(path, JsonSerializer.Serialize(data, options));
    }

    private static BannerStoreData CreateEmptyData()
    {
        var result = new BannerStoreData();
        foreach (var key in SlideSlotKeys)
        {
            result.SlideImages[key] = [];
        }

        foreach (var key in SingleSlotKeys)
        {
            result.SingleImages[key] = null;
        }

        return result;
    }
}
