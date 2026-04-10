using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Text.Json;
using System.Text.Json.Serialization;
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

    // Function: NewArrivals
    // Purpose: Executes the NewArrivals workflow for this module.
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

    // Function: RunningShoes
    // Purpose: Executes the RunningShoes workflow for this module.
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

    // Function: TrainingShoes
    // Purpose: Executes the TrainingShoes workflow for this module.
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

    // Function: Sales
    // Purpose: Executes the Sales workflow for this module.
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

    // Function: Men
    // Purpose: Executes the Men workflow for this module.
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

    // Function: Women
    // Purpose: Executes the Women workflow for this module.
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
    // Function: Search
    // Purpose: Executes the Search workflow for this module.
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

    // Function: Wishlist
    // Purpose: Executes the Wishlist workflow for this module.
    public IActionResult Wishlist()
    {
        if (!HttpContext.Session.GetInt32("CustomerUserId").HasValue)
        {
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Wishlist", "Shop") });
        }

        var products = LoadActiveProducts()
            .OrderByDescending(product => product.UpdatedAt ?? product.CreatedAt)
            .Select(MapCard)
            .ToList();

        var vm = new ShopUtilityPageViewModel
        {
            Eyebrow = "Saved For Later",
            Title = "Wishlist",
            Description = "Keep track of shoes you want to compare or buy later.",
            Message = "Your favorites are stored on this device. Sign in to keep and review them here.",
            ActionText = "Browse New Arrivals",
            ActionName = "NewArrivals",
            RecommendedProducts = products
        };

        return View(vm);
    }

    // Function: Cart
    // Purpose: Executes the Cart workflow for this module.
    public IActionResult Cart()
    {
        if (!HttpContext.Session.GetInt32("CustomerUserId").HasValue)
        {
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Cart", "Shop") });
        }

        var products = LoadActiveProducts()
            .OrderByDescending(product => product.UpdatedAt ?? product.CreatedAt)
            .Select(MapCard)
            .ToList();

        var vm = new ShopUtilityPageViewModel
        {
            Eyebrow = "Shopping Bag",
            Title = "Cart",
            Description = "Review items before checkout.",
            Message = "Items in your cart are stored on this device.",
            ActionText = "Continue Shopping",
            ActionName = "NewArrivals",
            RecommendedProducts = products
        };

        return View(vm);
    }

    [HttpGet]
    // Function: Checkout
    // Purpose: Executes the Checkout workflow for this module.
    public IActionResult Checkout()
    {
        var customerUserId = HttpContext.Session.GetInt32("CustomerUserId");
        if (!customerUserId.HasValue)
        {
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Checkout", "Shop") });
        }

        var customer = _db.Customers.AsNoTracking().FirstOrDefault(c => c.UserId == customerUserId.Value);
        if (customer == null)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Checkout", "Shop") });
        }

        var vm = new CheckoutViewModel
        {
            FullName = $"{customer.NameCustomer} {customer.LastnameCustomer}".Trim(),
            Email = customer.EmailCustomer ?? string.Empty,
            Phone = customer.Phonenumber ?? string.Empty,
            PaymentMethod = "COD",
            CartItemsJson = "[]"
        };

        return View(vm);
    }

    [HttpGet]
    public IActionResult PreviewPromotion(string? code, string? cartItemsJson)
    {
        var customerUserId = HttpContext.Session.GetInt32("CustomerUserId");
        if (!customerUserId.HasValue)
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            return Json(new
            {
                ok = false,
                message = "Enter a promo code.",
                discount = 0m,
                subtotal = 0m,
                total = 0m
            });
        }

        var cartItems = ParseCheckoutItems(cartItemsJson);
        if (cartItems.Count == 0)
        {
            return Json(new
            {
                ok = false,
                message = "Your cart is empty.",
                discount = 0m,
                subtotal = 0m,
                total = 0m
            });
        }

        var lineItems = BuildLineItems(cartItems);
        if (lineItems.Errors.Count > 0)
        {
            return Json(new
            {
                ok = false,
                message = lineItems.Errors[0],
                discount = 0m,
                subtotal = lineItems.Subtotal,
                total = lineItems.Subtotal
            });
        }

        var promo = EvaluatePromotion(code, lineItems.Subtotal, customerUserId.Value);
        var shippingFee = promo.IsFreeShipping ? 0m : 0m;
        var total = Math.Max(0m, lineItems.Subtotal + shippingFee - promo.DiscountAmount);

        return Json(new
        {
            ok = promo.IsValid,
            message = promo.Message,
            code = promo.NormalizedCode,
            discount = promo.DiscountAmount,
            subtotal = lineItems.Subtotal,
            shippingFee,
            total,
            promotionType = promo.PromotionType,
            minOrderAmount = promo.MinOrderAmount,
            maxDiscountAmount = promo.MaxDiscountAmount,
            discountValue = promo.DiscountValue
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // Function: Checkout
    // Purpose: Executes the Checkout workflow for this module.
    public IActionResult Checkout(CheckoutViewModel model)
    {
        var customerUserId = HttpContext.Session.GetInt32("CustomerUserId");
        if (!customerUserId.HasValue)
        {
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Checkout", "Shop") });
        }

        if (!string.Equals(model.PaymentMethod, "COD", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(model.PaymentMethod), "Only Cash on Delivery is available right now.");
        }

        var cartItems = ParseCheckoutItems(model.CartItemsJson);

        if (cartItems.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Your cart is empty. Add products before checkout.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var lineItems = BuildLineItems(cartItems);
        foreach (var error in lineItems.Errors)
        {
            ModelState.AddModelError(string.Empty, error);
        }

        if (!ModelState.IsValid || lineItems.Items.Count == 0)
        {
            return View(model);
        }

        var shippingFee = 0m;
        var promo = EvaluatePromotion(model.PromoCode, lineItems.Subtotal, customerUserId.Value);
        if (!string.IsNullOrWhiteSpace(model.PromoCode) && !promo.IsValid)
        {
            ModelState.AddModelError(nameof(model.PromoCode), promo.Message ?? "This promo code cannot be used.");
            return View(model);
        }

        if (promo.IsFreeShipping)
        {
            shippingFee = 0m;
        }

        var discount = promo.DiscountAmount;
        var subtotal = lineItems.Subtotal;
        var totalAmount = subtotal + shippingFee - discount;
        var shippingAddress = $"{model.AddressLine}, {model.District}, {model.Province}, {model.PostalCode}";
        if (!string.IsNullOrWhiteSpace(model.Notes))
        {
            shippingAddress += $" | Notes: {model.Notes.Trim()}";
        }

        using var transaction = _db.Database.BeginTransaction();
        try
        {
            var order = new Order
            {
                OrderNumber = GenerateOrderNumber(),
                UserId = customerUserId.Value,
                TotalAmount = totalAmount,
                ShippingFee = shippingFee,
                Discount = discount,
                PromotionId = promo.PromotionId,
                PromotionCode = promo.NormalizedCode,
                PromotionDiscountTotal = promo.DiscountAmount,
                PaymentMethod = "Cash on Delivery (COD)",
                PaymentStatus = "Pending",
                Status = "Pending",
                ShippingAddress = shippingAddress,
                ShippingMethod = "Standard Delivery",
                OrderDate = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _db.Orders.Add(order);
            _db.SaveChanges();

            foreach (var lineItem in lineItems.Items)
            {
                _db.OrderItems.Add(new OrderItem
                {
                    OrderId = order.OrderId,
                    SizeId = lineItem.SizeId,
                    Quantity = lineItem.Quantity,
                    UnitPrice = lineItem.UnitPrice
                });

                var size = _db.ProductSizes.First(size => size.SizeId == lineItem.SizeId);
                size.StockQuantity = Math.Max(0, size.StockQuantity - lineItem.Quantity);
            }

            if (promo.IsValid && promo.PromotionId.HasValue)
            {
                _db.PromotionUsages.Add(new PromotionUsage
                {
                    PromotionId = promo.PromotionId.Value,
                    OrderId = order.OrderId,
                    UserId = customerUserId.Value,
                    PromotionCode = promo.NormalizedCode,
                    DiscountAmount = promo.DiscountAmount,
                    UsedAt = DateTime.Now
                });
            }

            _db.SaveChanges();
            transaction.Commit();

            return RedirectToAction(nameof(CheckoutSuccess), new { id = order.OrderId });
        }
        catch
        {
            transaction.Rollback();
            ModelState.AddModelError(string.Empty, "Unable to place order right now. Please try again.");
            return View(model);
        }
    }

    [HttpGet]
    // Function: CheckoutSuccess
    // Purpose: Executes the CheckoutSuccess workflow for this module.
    public IActionResult CheckoutSuccess(int id)
    {
        var customerUserId = HttpContext.Session.GetInt32("CustomerUserId");
        if (!customerUserId.HasValue)
        {
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("CheckoutSuccess", "Shop", new { id }) });
        }

        var order = _db.Orders
            .AsNoTracking()
            .FirstOrDefault(order => order.OrderId == id && order.UserId == customerUserId.Value);

        if (order == null)
        {
            return RedirectToAction(nameof(Cart));
        }

        ViewBag.OrderNumber = order.OrderNumber;
        ViewBag.OrderDate = order.OrderDate;
        ViewBag.TotalAmount = order.TotalAmount;
        ViewBag.PaymentMethod = order.PaymentMethod;
        return View();
    }

    [HttpGet]
    // Function: OrderHistory
    // Purpose: Executes the OrderHistory workflow for this module.
    public IActionResult OrderHistory()
    {
        var customerUserId = HttpContext.Session.GetInt32("CustomerUserId");
        if (!customerUserId.HasValue)
        {
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("OrderHistory", "Shop") });
        }

        var orders = _db.Orders
            .AsNoTracking()
            .Where(order => order.UserId == customerUserId.Value)
            .Include(order => order.OrderItems)
            .OrderByDescending(order => order.OrderDate)
            .Select(order => new OrdersListItemViewModel
            {
                OrderId = order.OrderId,
                OrderNumber = order.OrderNumber,
                CustomerName = string.Empty,
                CustomerEmail = string.Empty,
                ItemCount = order.OrderItems.Sum(item => item.Quantity),
                TotalAmount = order.TotalAmount,
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
                Status = order.Status,
                ShippingMethod = order.ShippingMethod ?? "Standard Delivery",
                TrackingNumber = order.TrackingNumber ?? string.Empty,
                OrderDate = order.OrderDate,
                UpdatedAt = order.UpdatedAt
            })
            .ToList();

        var vm = new OrdersListViewModel
        {
            Orders = orders
        };

        return View(vm);
    }

    [HttpGet]
    // Function: OrderDetail
    // Purpose: Executes the OrderDetail workflow for this module.
    public IActionResult OrderDetail(int id)
    {
        var customerUserId = HttpContext.Session.GetInt32("CustomerUserId");
        if (!customerUserId.HasValue)
        {
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("OrderDetail", "Shop", new { id }) });
        }

        var order = _db.Orders
            .AsNoTracking()
            .Include(current => current.Customer)
            .Include(current => current.OrderItems)
                .ThenInclude(item => item.ProductSize!)
                    .ThenInclude(size => size.Color)
                        .ThenInclude(color => color.Product)
                            .ThenInclude(product => product.ProductImages)
            .FirstOrDefault(current => current.OrderId == id && current.UserId == customerUserId.Value);

        if (order == null)
        {
            return RedirectToAction(nameof(OrderHistory));
        }

        var vm = new OrderStatusViewModel
        {
            OrderId = order.OrderId,
            OrderNumber = order.OrderNumber,
            Status = order.Status,
            PaymentStatus = order.PaymentStatus,
            PaymentMethod = order.PaymentMethod,
            OrderDate = order.OrderDate,
            UpdatedAt = order.UpdatedAt,
            CustomerName = (order.Customer?.NameCustomer ?? "") + " " + (order.Customer?.LastnameCustomer ?? ""),
            CustomerEmail = order.Customer?.EmailCustomer ?? string.Empty,
            CustomerPhone = order.Customer?.Phonenumber ?? string.Empty,
            ShippingAddress = order.ShippingAddress ?? string.Empty,
            ShippingMethod = order.ShippingMethod ?? "Standard Delivery",
            TrackingNumber = order.TrackingNumber ?? "-",
            ShippingFee = order.ShippingFee,
            Discount = order.Discount,
            Items = order.OrderItems.Select(item => new OrderItemViewModel
            {
                ProductName = item.ProductSize?.Color?.Product?.ProductName ?? "-",
                Sku = item.ProductSize?.Color?.Product?.Sku ?? "-",
                Color = item.ProductSize?.Color?.ColorName ?? "-",
                Size = $"{item.ProductSize?.SizeType} {item.ProductSize?.SizeValue}",
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                ImageUrl = item.ProductSize?.Color?.Product?.ProductImages
                    .FirstOrDefault(image => image.ImageType == "Thumbnail")?.ImageUrl
            }).ToList(),
            Timeline = BuildCustomerTimeline(order.Status, order.OrderDate, order.UpdatedAt)
        };

        return View(vm);
    }

    // Function: BuildListingPage
    // Purpose: Executes the BuildListingPage workflow for this module.
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
        var allProducts = LoadActiveProducts();
        var products = orderBy(allProducts.Where(filter))
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
            AvailableCategories = allProducts
                .Select(product => product.Category.CategoryName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name)
                .ToList(),
            AvailableGenders = allProducts
                .Select(product => product.Gender)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name)
                .ToList(),
            AvailableBrands = allProducts
                .Select(product => product.Brand.BrandName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name)
                .ToList(),
            AvailableSizes = allProducts
                .SelectMany(product => product.ProductColors)
                .SelectMany(color => color.ProductSizes)
                .Where(size => size.StockQuantity > 0 && string.Equals(size.Status, "Active", StringComparison.OrdinalIgnoreCase))
                .Select(size => $"{size.SizeValue} {size.SizeType}".Trim())
                .Where(size => !string.IsNullOrWhiteSpace(size))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(size => size)
                .ToList(),
            Products = products
        };
    }

    // Function: BuildUtilityPage
    // Purpose: Executes the BuildUtilityPage workflow for this module.
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

    // Function: LoadActiveProducts
    // Purpose: Executes the LoadActiveProducts workflow for this module.
    private List<Product> LoadActiveProducts()
    {
        return _db.Products
            .AsNoTracking()
            .Include(product => product.Brand)
            .Include(product => product.Category)
            .Include(product => product.ProductImages)
            .Include(product => product.ProductColors)
                .ThenInclude(color => color.ProductSizes)
            .Where(product => product.Status == "Active")
            .ToList();
    }

    // Function: MapCard
    // Purpose: Executes the MapCard workflow for this module.
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
            DiscountPercent = product.DiscountPercent,
            Sizes = product.ProductColors
                .SelectMany(color => color.ProductSizes)
                .Where(size => size.StockQuantity > 0 && string.Equals(size.Status, "Active", StringComparison.OrdinalIgnoreCase))
                .Select(size => $"{size.SizeValue} {size.SizeType}".Trim())
                .Where(size => !string.IsNullOrWhiteSpace(size))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(size => size)
                .ToList()
        };
    }

    // Function: MatchesKeyword
    // Purpose: Executes the MatchesKeyword workflow for this module.
    private static bool MatchesKeyword(Product product, string keyword)
    {
        return ContainsIgnoreCase(product.ProductName, keyword)
            || ContainsIgnoreCase(product.Brand.BrandName, keyword)
            || ContainsIgnoreCase(product.Category.CategoryName, keyword)
            || ContainsIgnoreCase(product.Gender, keyword)
            || ContainsIgnoreCase(product.ShortDescription, keyword);
    }

    // Function: ContainsIgnoreCase
    // Purpose: Executes the ContainsIgnoreCase workflow for this module.
    private static bool ContainsIgnoreCase(string? source, string value)
    {
        return !string.IsNullOrWhiteSpace(source)
            && source.Contains(value, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsMissingTable(MySqlException ex, string tableName)
    {
        return ex.Number == 1146
            && ex.Message.Contains(tableName, StringComparison.OrdinalIgnoreCase);
    }

    private static List<CheckoutCartItemInput> ParseCheckoutItems(string? cartItemsJson)
    {
        try
        {
            return JsonSerializer.Deserialize<List<CheckoutCartItemInput>>(
                cartItemsJson ?? "[]",
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                })
                ?? new List<CheckoutCartItemInput>();
        }
        catch
        {
            return new List<CheckoutCartItemInput>();
        }
    }

    private CheckoutLineItemsResult BuildLineItems(List<CheckoutCartItemInput> cartItems)
    {
        var requestedProductIds = cartItems
            .Select(item => item.ProductId)
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        var products = _db.Products
            .Include(product => product.ProductColors)
                .ThenInclude(color => color.ProductSizes)
            .Where(product => requestedProductIds.Contains(product.ProductId) && product.Status == "Active")
            .ToList();

        var productsById = products.ToDictionary(product => product.ProductId);
        var lineItems = new List<CheckoutLineItem>();
        var errors = new List<string>();
        decimal subtotal = 0m;

        foreach (var item in cartItems)
        {
            if (!productsById.TryGetValue(item.ProductId, out var product))
            {
                errors.Add($"Some products are no longer available (Product ID: {item.ProductId}).");
                continue;
            }

            var quantity = Math.Max(1, item.Qty);
            var normalizedColor = (item.Color ?? string.Empty).Trim();
            var normalizedSizeValue = (item.Size ?? string.Empty).Trim();
            var normalizedSizeType = (item.SizeType ?? string.Empty).Trim();

            var sizeCandidates = product.ProductColors
                .Where(color => string.IsNullOrWhiteSpace(normalizedColor)
                    || normalizedColor == "Default"
                    || string.Equals(color.ColorName, normalizedColor, StringComparison.OrdinalIgnoreCase))
                .SelectMany(color => color.ProductSizes)
                .Where(size => string.Equals(size.Status, "Active", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!string.IsNullOrWhiteSpace(normalizedSizeValue) && normalizedSizeValue != "-")
            {
                sizeCandidates = sizeCandidates
                    .Where(size => string.Equals(size.SizeValue, normalizedSizeValue, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(normalizedSizeType))
            {
                sizeCandidates = sizeCandidates
                    .Where(size => string.Equals(size.SizeType, normalizedSizeType, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var selectedSize = sizeCandidates.FirstOrDefault(size => size.StockQuantity >= quantity)
                ?? sizeCandidates.FirstOrDefault();

            if (selectedSize == null)
            {
                errors.Add($"{product.ProductName}: selected size/color is unavailable.");
                continue;
            }

            if (selectedSize.StockQuantity < quantity)
            {
                errors.Add($"{product.ProductName}: not enough stock for requested quantity.");
                continue;
            }

            var hasSale = product.SalePrice.HasValue && product.SalePrice.Value > 0 && product.SalePrice.Value < product.RegularPrice;
            var unitPrice = hasSale ? product.SalePrice!.Value : product.RegularPrice;

            lineItems.Add(new CheckoutLineItem(selectedSize.SizeId, quantity, unitPrice));
            subtotal += unitPrice * quantity;
        }

        return new CheckoutLineItemsResult(lineItems, subtotal, errors);
    }

    private PromotionEvaluationResult EvaluatePromotion(string? promoCode, decimal subtotal, int customerUserId)
    {
        var normalizedCode = string.IsNullOrWhiteSpace(promoCode)
            ? null
            : promoCode.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(normalizedCode))
        {
            return PromotionEvaluationResult.None();
        }

        var now = DateTime.Now;
        var promotion = _db.Promotions
            .AsNoTracking()
            .FirstOrDefault(p => p.Code != null
                && p.Code == normalizedCode
                && p.Status == "Active"
                && p.StartAt <= now
                && p.EndAt >= now);

        if (promotion == null)
        {
            return PromotionEvaluationResult.Invalid("Promo code is invalid, inactive, or expired.");
        }

        if (promotion.MinOrderAmount.HasValue && subtotal < promotion.MinOrderAmount.Value)
        {
            return PromotionEvaluationResult.Invalid($"This promo requires minimum order of ฿{promotion.MinOrderAmount.Value:N2}.");
        }

        try
        {
            if (promotion.UsageLimitTotal.HasValue)
            {
                var totalUsed = _db.PromotionUsages.Count(u => u.PromotionId == promotion.PromotionId);
                if (totalUsed >= promotion.UsageLimitTotal.Value)
                {
                    return PromotionEvaluationResult.Invalid("This promo code has reached its usage limit.");
                }
            }

            if (promotion.UsageLimitPerUser.HasValue)
            {
                var userUsed = _db.PromotionUsages.Count(u => u.PromotionId == promotion.PromotionId && u.UserId == customerUserId);
                if (userUsed >= promotion.UsageLimitPerUser.Value)
                {
                    return PromotionEvaluationResult.Invalid("You have reached usage limit for this promo code.");
                }
            }
        }
        catch (MySqlException ex) when (IsMissingTable(ex, "promotion_usages"))
        {
            return PromotionEvaluationResult.Invalid("Promotion usage table is missing. Please run database setup for promotion_usages.");
        }

        decimal discount = 0m;
        var isFreeShipping = string.Equals(promotion.PromotionType, "FreeShipping", StringComparison.OrdinalIgnoreCase);

        if (string.Equals(promotion.PromotionType, "Percent", StringComparison.OrdinalIgnoreCase))
        {
            discount = subtotal * (promotion.DiscountValue / 100m);
        }
        else if (string.Equals(promotion.PromotionType, "FixedAmount", StringComparison.OrdinalIgnoreCase))
        {
            discount = promotion.DiscountValue;
        }

        if (promotion.MaxDiscountAmount.HasValue && discount > promotion.MaxDiscountAmount.Value)
        {
            discount = promotion.MaxDiscountAmount.Value;
        }

        discount = Math.Max(0m, Math.Min(subtotal, discount));

        return PromotionEvaluationResult.Success(
            promotion.PromotionId,
            normalizedCode,
            promotion.PromotionType,
            promotion.DiscountValue,
            promotion.MinOrderAmount,
            promotion.MaxDiscountAmount,
            discount,
            isFreeShipping,
            "Promo code applied.");
    }

    // Function: GenerateOrderNumber
    // Purpose: Executes the GenerateOrderNumber workflow for this module.
    private static string GenerateOrderNumber()
    {
        var now = DateTime.Now;
        return $"ORD{now:yyyyMMddHHmmss}{Random.Shared.Next(10, 99)}";
    }

    // Function: BuildCustomerTimeline
    // Purpose: Executes the BuildCustomerTimeline workflow for this module.
    private static List<OrderTimelineStep> BuildCustomerTimeline(string status, DateTime orderDate, DateTime? updatedAt)
    {
        var steps = new[] { "Pending", "Processing", "Shipped", "Delivered" };
        var icons = new Dictionary<string, string>
        {
            ["Pending"] = "bi-clock",
            ["Processing"] = "bi-box-seam",
            ["Shipped"] = "bi-truck",
            ["Delivered"] = "bi-check-circle"
        };

        var cancelled = string.Equals(status, "Cancelled", StringComparison.OrdinalIgnoreCase);
        var currentIndex = Array.IndexOf(steps, status);

        var timeline = steps.Select((step, index) => new OrderTimelineStep
        {
            StepName = step,
            Icon = icons[step],
            IsCompleted = !cancelled && currentIndex > index,
            IsCurrent = !cancelled && currentIndex == index,
            IsCancelled = cancelled,
            CompletedAt = !cancelled && currentIndex >= index
                ? (index == 0 ? orderDate : updatedAt)
                : null
        }).ToList();

        if (cancelled)
        {
            timeline.Add(new OrderTimelineStep
            {
                StepName = "Cancelled",
                Icon = "bi-x-circle",
                IsCurrent = true,
                IsCancelled = true,
                CompletedAt = updatedAt ?? orderDate
            });
        }

        return timeline;
    }

    private sealed class CheckoutCartItemInput
    {
        public int ProductId { get; set; }

        public string? Color { get; set; }

        public string? Size { get; set; }

        public string? SizeType { get; set; }

        public int Qty { get; set; }
    }

    private sealed record CheckoutLineItem(int SizeId, int Quantity, decimal UnitPrice);

    private sealed record CheckoutLineItemsResult(List<CheckoutLineItem> Items, decimal Subtotal, List<string> Errors);

    private sealed record PromotionEvaluationResult(
        bool IsValid,
        int? PromotionId,
        string? NormalizedCode,
        string? PromotionType,
        decimal DiscountValue,
        decimal? MinOrderAmount,
        decimal? MaxDiscountAmount,
        decimal DiscountAmount,
        bool IsFreeShipping,
        string Message)
    {
        public static PromotionEvaluationResult None() => new(
            false,
            null,
            null,
            null,
            0m,
            null,
            null,
            0m,
            false,
            string.Empty);

        public static PromotionEvaluationResult Invalid(string message) => new(
            false,
            null,
            null,
            null,
            0m,
            null,
            null,
            0m,
            false,
            message);

        public static PromotionEvaluationResult Success(
            int promotionId,
            string normalizedCode,
            string promotionType,
            decimal discountValue,
            decimal? minOrderAmount,
            decimal? maxDiscountAmount,
            decimal discountAmount,
            bool isFreeShipping,
            string message) => new(
                true,
                promotionId,
                normalizedCode,
                promotionType,
                discountValue,
                minOrderAmount,
                maxDiscountAmount,
                discountAmount,
                isFreeShipping,
                message);
    }
}
