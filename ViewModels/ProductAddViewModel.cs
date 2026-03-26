using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp1.ViewModels;

/// <summary>
/// ViewModel สำหรับหน้า ProductsAdd (GET = dropdown data, POST = form binding)
/// </summary>
public class ProductAddViewModel
{
    // ── Dropdown data (populated in GET) ──
    public List<SelectListItem> Brands { get; set; } = new();
    public List<SelectListItem> Categories { get; set; } = new();

    // ── Form fields (bound on POST) ──
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

    // ── Color + Size sub-forms ──
    public List<ColorFormModel> Colors { get; set; } = new();

    // ── Submit action (save / publish) ──
    public string? Action { get; set; }
}

public class ColorFormModel
{
    public string Name { get; set; } = string.Empty;
    public string SizeType { get; set; } = "EU";
    public List<SizeFormModel> Sizes { get; set; } = new();
}

public class SizeFormModel
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int Stock { get; set; }
    public string Status { get; set; } = "Active";
}
