using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp1.ViewModels;

/// <summary>
/// ViewModel สำหรับหน้า ProductsEdit  (GET = pre-populate, POST = form binding)
/// </summary>
public class ProductEditViewModel
{
    // ── Key ──
    public int ProductId { get; set; }

    // ── Dropdown data (populated in GET) ──
    public List<SelectListItem> Brands { get; set; } = new();
    public List<SelectListItem> Categories { get; set; } = new();

    // ── Form fields ──
    public string ProductName { get; set; } = string.Empty;
    public string ProductSKU { get; set; } = string.Empty;
    public int ProductBrand { get; set; }
    public int ProductCategory { get; set; }
    public string ProductGender { get; set; } = string.Empty;
    public string ProductStatus { get; set; } = string.Empty;

    public decimal RegularPrice { get; set; }
    public decimal? SalePrice { get; set; }
    public decimal? CostPrice { get; set; }

    public string ShortDescription { get; set; } = string.Empty;
    public string? FullDescription { get; set; }
    public string? HighlightFeatures { get; set; }
    public string? TechnicalDetails { get; set; }

    public bool ShowOnHomepage { get; set; }

    // ── Existing images (for preview in GET) ──
    public string? ThumbnailUrl { get; set; }
    public List<string> GalleryUrls { get; set; } = new();

    // ── Color + Size sub-forms ──
    public List<ColorEditModel> Colors { get; set; } = new();

    // ── Submit action (save / publish) ──
    public string? Action { get; set; }
}

public class ColorEditModel
{
    public int ColorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SizeType { get; set; } = "EU";
    public List<string> ExistingImageUrls { get; set; } = new();
    public List<SizeEditModel> Sizes { get; set; } = new();
}

public class SizeEditModel
{
    public int SizeId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int Stock { get; set; }
    public string Status { get; set; } = "Active";
}
