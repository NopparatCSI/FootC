using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp1.Models.Db;
using WebApp1.ViewModels;

namespace WebApp1.Controllers;

[AllowAnonymous]
public class ShopController : Controller
{
    private readonly Csi402dbContext _db;

    public ShopController(Csi402dbContext db)
    {
        _db = db;
    }

    public IActionResult NewArrivals()
    {
        var vm = BuildListingPage(
            "Fresh Drop",
            "New Arrivals",
            "Latest releases and recently updated products from the current collection.",
            product => true,
            products => products.OrderByDescending(product => product.UpdatedAt ?? product.CreatedAt));

        return View(vm);
    }

    public IActionResult RunningShoes()
    {
        var vm = BuildListingPage(
            "Road Ready",
            "Running Shoes",
            "Responsive and lightweight models for daily runs, speed work, and long miles.",
            product => ContainsIgnoreCase(product.Category.CategoryName, "running"),
            products => products.OrderByDescending(product => product.UpdatedAt ?? product.CreatedAt));

        return View(vm);
    }

    public IActionResult TrainingShoes()
    {
        var vm = BuildListingPage(
            "Gym Floor",
            "Training Shoes",
            "Stable silhouettes built for workouts, lifting, and movement sessions.",
            product => ContainsIgnoreCase(product.Category.CategoryName, "training"),
            products => products.OrderByDescending(product => product.UpdatedAt ?? product.CreatedAt));

        return View(vm);
    }

    public IActionResult Sales()
    {
        var vm = BuildListingPage(
            "Marked Down",
            "Sales",
            "Products currently priced below regular retail, ready for quick checkout.",
            product => product.SalePrice.HasValue && product.SalePrice.Value > 0 && product.SalePrice.Value < product.RegularPrice,
            products => products.OrderByDescending(product => product.DiscountPercent ?? 0));

        return View(vm);
    }

    public IActionResult Men()
    {
        var vm = BuildListingPage(
            "Menswear",
            "Men",
            "Shop the current men-specific assortment and unisex pairs styled for everyday wear.",
            product => string.Equals(product.Gender, "Men", StringComparison.OrdinalIgnoreCase),
            products => products.OrderByDescending(product => product.UpdatedAt ?? product.CreatedAt));

        return View(vm);
    }

    public IActionResult Women()
    {
        var vm = BuildListingPage(
            "Womenswear",
            "Women",
            "Explore women-focused fits and standout performance pairs from the latest lineup.",
            product => string.Equals(product.Gender, "Women", StringComparison.OrdinalIgnoreCase),
            products => products.OrderByDescending(product => product.UpdatedAt ?? product.CreatedAt));

        return View(vm);
    }

    [HttpGet]
    public IActionResult Search(string? q)
    {
        var keyword = q?.Trim() ?? string.Empty;
        var vm = BuildListingPage(
            "Search",
            string.IsNullOrWhiteSpace(keyword) ? "Search Products" : $"Results for \"{keyword}\"",
            string.IsNullOrWhiteSpace(keyword)
                ? "Search by product name, brand, category, gender, or description."
                : "Matching products from the current storefront collection.",
            product => string.IsNullOrWhiteSpace(keyword) || MatchesKeyword(product, keyword),
            products => products.OrderByDescending(product => product.UpdatedAt ?? product.CreatedAt),
            keyword,
            string.IsNullOrWhiteSpace(keyword) ? "Start a search" : "No products found",
            string.IsNullOrWhiteSpace(keyword)
                ? "Use the search field in the navbar to look for products or categories."
                : "Try another keyword or browse one of the collections from the navigation.");

        return View(vm);
    }

    public IActionResult Wishlist()
    {
        var vm = BuildUtilityPage(
            "Saved For Later",
            "Wishlist",
            "Keep track of shoes you want to compare or buy later.",
            "Wishlist persistence is not implemented yet, so this page acts as a starter placeholder.",
            "Browse New Arrivals",
            "NewArrivals");

        return View(vm);
    }

    public IActionResult Cart()
    {
        var vm = BuildUtilityPage(
            "Shopping Bag",
            "Cart",
            "Review selected products before the checkout flow is added.",
            "Cart state and checkout are not wired yet, so this page currently shows a preview state.",
            "Continue Shopping",
            "NewArrivals");

        return View(vm);
    }

    private ShopListingPageViewModel BuildListingPage(
        string eyebrow,
        string title,
        string description,
        Func<Product, bool> filter,
        Func<IEnumerable<Product>, IEnumerable<Product>> orderBy,
        string? searchQuery = null,
        string? emptyTitle = null,
        string? emptyDescription = null)
    {
        var products = orderBy(LoadActiveProducts().Where(filter))
            .Select(MapCard)
            .ToList();

        return new ShopListingPageViewModel
        {
            Eyebrow = eyebrow,
            Title = title,
            Description = description,
            SearchQuery = searchQuery,
            EmptyTitle = emptyTitle ?? $"No products in {title}",
            EmptyDescription = emptyDescription ?? "Add products in the admin area or broaden the filter criteria.",
            Products = products
        };
    }

    private ShopUtilityPageViewModel BuildUtilityPage(
        string eyebrow,
        string title,
        string description,
        string message,
        string actionText,
        string actionName)
    {
        var recommendedProducts = LoadActiveProducts()
            .OrderByDescending(product => product.UpdatedAt ?? product.CreatedAt)
            .Take(4)
            .Select(MapCard)
            .ToList();

        return new ShopUtilityPageViewModel
        {
            Eyebrow = eyebrow,
            Title = title,
            Description = description,
            Message = message,
            ActionText = actionText,
            ActionName = actionName,
            RecommendedProducts = recommendedProducts
        };
    }

    private List<Product> LoadActiveProducts()
    {
        return _db.Products
            .AsNoTracking()
            .Include(product => product.Brand)
            .Include(product => product.Category)
            .Include(product => product.ProductImages)
            .Where(product => product.Status == "Active")
            .ToList();
    }

    private static ShopProductCardViewModel MapCard(Product product)
    {
        var imageUrl = product.ProductImages
            .Where(image => string.Equals(image.ImageType, "Thumbnail", StringComparison.OrdinalIgnoreCase))
            .OrderBy(image => image.SortOrder)
            .Select(image => image.ImageUrl)
            .FirstOrDefault()
            ?? product.ProductImages
                .OrderBy(image => image.SortOrder)
                .Select(image => image.ImageUrl)
                .FirstOrDefault()
            ?? "/images/Running-Shoe-2.jpeg";

        return new ShopProductCardViewModel
        {
            ProductId = product.ProductId,
            Name = product.ProductName,
            BrandName = product.Brand.BrandName,
            CategoryName = product.Category.CategoryName,
            Gender = product.Gender,
            ShortDescription = product.ShortDescription,
            ImageUrl = imageUrl,
            RegularPrice = product.RegularPrice,
            SalePrice = product.SalePrice,
            DiscountPercent = product.DiscountPercent
        };
    }

    private static bool MatchesKeyword(Product product, string keyword)
    {
        return ContainsIgnoreCase(product.ProductName, keyword)
            || ContainsIgnoreCase(product.Brand.BrandName, keyword)
            || ContainsIgnoreCase(product.Category.CategoryName, keyword)
            || ContainsIgnoreCase(product.Gender, keyword)
            || ContainsIgnoreCase(product.ShortDescription, keyword);
    }

    private static bool ContainsIgnoreCase(string? source, string value)
    {
        return !string.IsNullOrWhiteSpace(source)
            && source.Contains(value, StringComparison.OrdinalIgnoreCase);
    }
}