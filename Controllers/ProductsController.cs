using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp1.Models;
using WebApp1.Models.Db;
using WebApp1.Security;
using WebApp1.ViewModels;

namespace WebApp1.Controllers;

[Authorize(Policy = AdminPolicies.ManageCatalog)]
public class ProductsController : Controller
{
    private readonly Csi402dbContext _db;
    private readonly IWebHostEnvironment _env;

    public ProductsController(Csi402dbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    // Function: Index
    // Purpose: Executes the Index workflow for this module.
    public IActionResult Index()
    {
        return View();
    }

    // Customer-facing product page
    // Shows only what customers should see (no cost price / internal status flags / stock numbers)
    [AllowAnonymous]
    // Function: Product
    // Purpose: Executes the Product workflow for this module.
    public IActionResult Product(int? id)
    {
        if (!id.HasValue)
        {
            return NotFound();
        }

        var product = _db.Products
            .AsNoTracking()
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .Include(p => p.ProductColors)
                .ThenInclude(c => c.ProductColorImages)
            .FirstOrDefault(p => p.ProductId == id.Value && p.Status == "Active");

        if (product == null)
        {
            return NotFound();
        }

        var thumbnail = product.ProductImages
            .Where(image => string.Equals(image.ImageType, "Thumbnail", StringComparison.OrdinalIgnoreCase))
            .OrderBy(image => image.SortOrder)
            .Select(image => image.ImageUrl)
            .FirstOrDefault()
            ?? product.ProductImages
                .OrderBy(image => image.SortOrder)
                .Select(image => image.ImageUrl)
                .FirstOrDefault()
            ?? product.ProductColors
                .SelectMany(color => color.ProductColorImages)
                .OrderBy(image => image.SortOrder)
                .Select(image => image.ImageUrl)
                .FirstOrDefault()
            ?? "/images/Running-Shoe-2.jpeg";

        var gallery = product.ProductImages
            .Where(image => string.Equals(image.ImageType, "Gallery", StringComparison.OrdinalIgnoreCase))
            .OrderBy(image => image.SortOrder)
            .Select(image => image.ImageUrl)
            .ToList();

        gallery.AddRange(
            product.ProductColors
                .SelectMany(color => color.ProductColorImages)
                .OrderBy(image => image.SortOrder)
                .Select(image => image.ImageUrl));

        var vm = new ProductCustomerViewModel
        {
            Id = product.ProductId,
            IsCustomerLoggedIn = HttpContext.Session.GetInt32("CustomerUserId").HasValue,
            Name = product.ProductName,
            Brand = product.Brand.BrandName,
            Category = product.Category.CategoryName,
            Gender = product.Gender,
            RegularPrice = product.RegularPrice,
            SalePrice = product.SalePrice,
            DiscountPercent = product.DiscountPercent,
            ShortDescription = product.ShortDescription,
            FullDescription = product.FullDescription,
            HighlightFeaturesHtml = SanitizeRichText(product.HighlightFeatures),
            TechnicalDetails = product.TechnicalDetails,
            ThumbnailUrl = thumbnail,
            GalleryUrls = gallery
                .Where(url => !string.IsNullOrWhiteSpace(url) && !string.Equals(url, thumbnail, StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList(),
            ColorNames = product.ProductColors
                .Select(color => color.ColorName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
        };

        return View(vm);
    }

    // Function: SanitizeRichText
    // Purpose: Executes the SanitizeRichText workflow for this module.
    private static string? SanitizeRichText(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return null;
        }

        // Very small, defense-in-depth sanitizer for admin-provided rich text.
        // Removes script blocks and obvious JS/event-handler injection.
        var sanitized = System.Text.RegularExpressions.Regex.Replace(
            html,
            "<script\\b[^<]*(?:(?!<\\/script>)<[^<]*)*<\\/script>",
            string.Empty,
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        sanitized = System.Text.RegularExpressions.Regex.Replace(
            sanitized,
            "\\son\\w+\\s*=\\s*\"[^\"]*\"",
            string.Empty,
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        sanitized = System.Text.RegularExpressions.Regex.Replace(
            sanitized,
            "\\son\\w+\\s*=\\s*'[^']*'",
            string.Empty,
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        sanitized = System.Text.RegularExpressions.Regex.Replace(
            sanitized,
            "javascript:",
            string.Empty,
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        return sanitized;
    }

    // Function: ProductsList
    // Purpose: Executes the ProductsList workflow for this module.
    public IActionResult ProductsList()
    {
        var products = _db.Products
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
            .Select(p => new ProductListItemViewModel
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Sku = p.Sku,
                BrandName = p.Brand.BrandName,
                CategoryName = p.Category.CategoryName,
                Status = p.Status,
                Gender = p.Gender,
                RegularPrice = p.RegularPrice,
                SalePrice = p.SalePrice,
                UpdatedAt = (p.UpdatedAt ?? p.CreatedAt).ToString("yyyy-MM-dd")
            })
            .ToList();

        var vm = new ProductListViewModel
        {
            Products = products,
            Categories = _db.Categories
                .Where(c => (c.IsActive ?? true))
                .OrderBy(c => c.CategoryName)
                .Select(c => new SelectListItem(c.CategoryName, c.CategoryId.ToString()))
                .ToList(),
            Brands = _db.Brands
                .Where(b => (b.IsActive ?? true))
                .OrderBy(b => b.BrandName)
                .Select(b => new SelectListItem(b.BrandName, b.BrandId.ToString()))
                .ToList()
        };

        return View(vm);
    }

    // Function: ProductsEdit
    // Purpose: Executes the ProductsEdit workflow for this module.
    public IActionResult ProductsEdit(int id)
    {
        var product = _db.Products
            .Include(p => p.ProductImages)
            .Include(p => p.ProductColors)
                .ThenInclude(c => c.ProductSizes)
            .Include(p => p.ProductColors)
                .ThenInclude(c => c.ProductColorImages)
            .FirstOrDefault(p => p.ProductId == id);

        if (product == null) return NotFound();

        var thumbnail = product.ProductImages
            .Where(i => i.ImageType == "Thumbnail")
            .OrderBy(i => i.SortOrder)
            .FirstOrDefault();

        var gallery = product.ProductImages
            .Where(i => i.ImageType == "Gallery")
            .OrderBy(i => i.SortOrder)
            .Select(i => i.ImageUrl)
            .ToList();

        var vm = new ProductEditViewModel
        {
            ProductId = product.ProductId,
            ProductName = product.ProductName,
            ProductSKU = product.Sku,
            ProductBrand = product.BrandId,
            ProductCategory = product.CategoryId,
            ProductGender = product.Gender,
            ProductStatus = product.Status,
            RegularPrice = product.RegularPrice,
            SalePrice = product.SalePrice,
            CostPrice = product.CostPrice,
            ShortDescription = product.ShortDescription,
            FullDescription = product.FullDescription,
            HighlightFeatures = product.HighlightFeatures,
            TechnicalDetails = product.TechnicalDetails,
            ShowOnHomepage = product.ShowOnHomepage,
            ThumbnailUrl = thumbnail?.ImageUrl,
            GalleryUrls = gallery,
            Brands = _db.Brands
                .Where(b => (b.IsActive ?? true))
                .OrderBy(b => b.BrandName)
                .Select(b => new SelectListItem(b.BrandName, b.BrandId.ToString()))
                .ToList(),
            Categories = _db.Categories
                .Where(c => (c.IsActive ?? true))
                .OrderBy(c => c.CategoryName)
                .Select(c => new SelectListItem(c.CategoryName, c.CategoryId.ToString()))
                .ToList(),
            Colors = product.ProductColors.Select(c => new ColorEditModel
            {
                ColorId = c.ColorId,
                Name = c.ColorName,
                SizeType = c.ProductSizes.FirstOrDefault()?.SizeType ?? "EU",
                ExistingImageUrls = c.ProductColorImages
                    .OrderBy(i => i.SortOrder)
                    .Select(i => i.ImageUrl)
                    .ToList(),
                Sizes = c.ProductSizes.Select(s => new SizeEditModel
                {
                    SizeId = s.SizeId,
                    Type = s.SizeType,
                    Value = s.SizeValue,
                    Stock = s.StockQuantity,
                    Status = s.Status
                }).ToList()
            }).ToList()
        };

        return View(vm);
    }

    // Function: ProductsView
    // Purpose: Executes the ProductsView workflow for this module.
    public IActionResult ProductsView()
    {
        return View();
    }

    // Function: ProductsAdd
    // Purpose: Executes the ProductsAdd workflow for this module.
    public IActionResult ProductsAdd()
    {
        var vm = new ProductAddViewModel
        {
            Brands = _db.Brands
                .Where(b => (b.IsActive ?? true))
                .OrderBy(b => b.BrandName)
                .Select(b => new SelectListItem(b.BrandName, b.BrandId.ToString()))
                .ToList(),
            Categories = _db.Categories
                .Where(c => (c.IsActive ?? true))
                .OrderBy(c => c.CategoryName)
                .Select(c => new SelectListItem(c.CategoryName, c.CategoryId.ToString()))
                .ToList()
        };
        return View(vm);
    }

    [HttpPost]
    // Function: AddProduct
    // Purpose: Executes the AddProduct workflow for this module.
    public async Task<IActionResult> AddProduct(ProductAddViewModel form, IFormFile? thumbnailImage, List<IFormFile>? galleryImages)
    {
        // â”€â”€ Determine status from action button â”€â”€
        var status = form.Action == "publish" ? "Active" : (string.IsNullOrEmpty(form.ProductStatus) ? "Draft" : form.ProductStatus);

        var product = new Product
        {
            ProductName = form.ProductName,
            Sku = form.ProductSKU,
            BrandId = form.ProductBrand,
            CategoryId = form.ProductCategory,
            Gender = form.ProductGender,
            Status = status,
            RegularPrice = form.RegularPrice,
            SalePrice = form.SalePrice,
            CostPrice = form.CostPrice,
            ShortDescription = form.ShortDescription,
            FullDescription = form.FullDescription,
            HighlightFeatures = form.HighlightFeatures,
            TechnicalDetails = form.TechnicalDetails,
            ShowOnHomepage = form.ShowOnHomepage,
            CreatedAt = DateTime.Now
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        // â”€â”€ Save thumbnail image â”€â”€
        var uploadsDir = Path.Combine(_env.WebRootPath, "images", "products", product.ProductId.ToString());
        Directory.CreateDirectory(uploadsDir);

        int sortOrder = 0;
        if (thumbnailImage != null && thumbnailImage.Length > 0)
        {
            var thumbName = "thumb_" + Guid.NewGuid() + Path.GetExtension(thumbnailImage.FileName);
            var thumbPath = Path.Combine(uploadsDir, thumbName);
            using (var fs = new FileStream(thumbPath, FileMode.Create))
                await thumbnailImage.CopyToAsync(fs);

            _db.ProductImages.Add(new ProductImage
            {
                ProductId = product.ProductId,
                ImageUrl = "/images/products/" + product.ProductId + "/" + thumbName,
                ImageType = "Thumbnail",
                SortOrder = 0
            });
        }

        // â”€â”€ Save gallery images â”€â”€
        if (galleryImages != null)
        {
            foreach (var file in galleryImages)
            {
                if (file.Length == 0) continue;
                sortOrder++;
                var galleryName = "gallery_" + Guid.NewGuid() + Path.GetExtension(file.FileName);
                var galleryPath = Path.Combine(uploadsDir, galleryName);
                using (var fs = new FileStream(galleryPath, FileMode.Create))
                    await file.CopyToAsync(fs);

                _db.ProductImages.Add(new ProductImage
                {
                    ProductId = product.ProductId,
                    ImageUrl = "/images/products/" + product.ProductId + "/" + galleryName,
                    ImageType = "Gallery",
                    SortOrder = sortOrder
                });
            }
        }

        // â”€â”€ Save colors & sizes â”€â”€
        if (form.Colors != null)
        {
            foreach (var colorForm in form.Colors)
            {
                if (string.IsNullOrWhiteSpace(colorForm.Name)) continue;

                var color = new ProductColor
                {
                    ProductId = product.ProductId,
                    ColorName = colorForm.Name.Trim()
                };
                _db.ProductColors.Add(color);
                await _db.SaveChangesAsync();

                if (colorForm.Sizes != null)
                {
                    foreach (var sizeForm in colorForm.Sizes)
                    {
                        _db.ProductSizes.Add(new ProductSize
                        {
                            ColorId = color.ColorId,
                            SizeType = sizeForm.Type,
                            SizeValue = sizeForm.Value,
                            StockQuantity = sizeForm.Stock,
                            Status = sizeForm.Status ?? "Active"
                        });
                    }
                }
            }
        }

        await _db.SaveChangesAsync();
        return RedirectToAction("ProductsList");
    }

    [HttpPost]
    // Function: DeleteProduct
    // Purpose: Executes the DeleteProduct workflow for this module.
    public IActionResult DeleteProduct(int id)
    {
        // à¸„à¹‰à¸™à¸«à¸²à¸ªà¸´à¸™à¸„à¹‰à¸²à¸žà¸£à¹‰à¸­à¸¡à¸‚à¹‰à¸­à¸¡à¸¹à¸¥à¸—à¸µà¹ˆà¹€à¸à¸µà¹ˆà¸¢à¸§à¸‚à¹‰à¸­à¸‡
        var product = _db.Products
            .Include(p => p.ProductColors)
                .ThenInclude(c => c.ProductSizes)
            .Include(p => p.ProductColors)
                .ThenInclude(c => c.ProductColorImages)
            .Include(p => p.ProductImages)
            .FirstOrDefault(p => p.ProductId == id);

        if (product == null)
            return Json(new { success = false, message = "Product not found." });

        // à¸¥à¸šà¸‚à¹‰à¸­à¸¡à¸¹à¸¥à¸—à¸µà¹ˆà¹€à¸à¸µà¹ˆà¸¢à¸§à¸‚à¹‰à¸­à¸‡à¸—à¸±à¹‰à¸‡à¸«à¸¡à¸”
        foreach (var color in product.ProductColors)
        {
            _db.ProductSizes.RemoveRange(color.ProductSizes);
            _db.ProductColorImages.RemoveRange(color.ProductColorImages);
        }
        _db.ProductColors.RemoveRange(product.ProductColors);
        _db.ProductImages.RemoveRange(product.ProductImages);
        _db.Products.Remove(product);
        _db.SaveChanges();

        // à¸¥à¸šà¹„à¸Ÿà¸¥à¹Œà¸£à¸¹à¸›
        string imgDir = Path.Combine(_env.WebRootPath, "images", "products", id.ToString());
        if (Directory.Exists(imgDir))
            Directory.Delete(imgDir, true);

        return Json(new { success = true });
    }

    // Function: ProductsCategories
    // Purpose: Executes the ProductsCategories workflow for this module.
    public IActionResult ProductsCategories()
    {
        var categories = _db.Categories
            .Include(c => c.Parent)
            .OrderBy(c => c.ParentId == null ? c.CategoryId : c.ParentId)
            .ThenBy(c => c.ParentId == null ? 0 : 1)
            .ThenBy(c => c.CategoryId)
            .Select(c => new CategoryViewModel
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                ParentId = c.ParentId,
                ParentName = c.Parent != null ? c.Parent.CategoryName : null,
                IsActive = c.IsActive ?? true,
                CreatedAt = c.CreatedAt.ToString("yyyy-MM-dd")
            })
            .ToList();
        return View(categories);
    }

    [HttpPost]
    // Function: AddCategory
    // Purpose: Executes the AddCategory workflow for this module.
    public IActionResult AddCategory([FromBody] CategoryViewModel dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CategoryName))
            return Json(new { success = false, message = "Category name is required." });

        // Check duplicate (same parent + name)
        if (_db.Categories.Any(c => c.CategoryName == dto.CategoryName && c.ParentId == dto.ParentId))
            return Json(new { success = false, message = "Category \"" + dto.CategoryName + "\" already exists." });

        var category = new Category
        {
            CategoryName = dto.CategoryName.Trim(),
            ParentId = dto.ParentId,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.Now
        };
        _db.Categories.Add(category);
        _db.SaveChanges();

        string? parentName = null;
        if (category.ParentId != null)
        {
            parentName = _db.Categories
                .Where(c => c.CategoryId == category.ParentId)
                .Select(c => c.CategoryName)
                .FirstOrDefault();
        }

        return Json(new
        {
            success = true,
            category = new CategoryViewModel
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                ParentId = category.ParentId,
                ParentName = parentName,
                IsActive = category.IsActive ?? true,
                CreatedAt = category.CreatedAt.ToString("yyyy-MM-dd")
            }
        });
    }

    [HttpPost]
    // Function: EditCategory
    // Purpose: Executes the EditCategory workflow for this module.
    public IActionResult EditCategory([FromBody] CategoryViewModel dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CategoryName))
            return Json(new { success = false, message = "Category name is required." });

        var category = _db.Categories.Find(dto.CategoryId);
        if (category == null)
            return Json(new { success = false, message = "Category not found." });

        // Check duplicate (exclude self)
        if (_db.Categories.Any(c => c.CategoryId != dto.CategoryId && c.CategoryName == dto.CategoryName && c.ParentId == dto.ParentId))
            return Json(new { success = false, message = "Category \"" + dto.CategoryName + "\" already exists." });

        category.CategoryName = dto.CategoryName.Trim();
        category.ParentId = dto.ParentId;
        category.IsActive = dto.IsActive;
        _db.SaveChanges();

        string? parentName = null;
        if (category.ParentId != null)
        {
            parentName = _db.Categories
                .Where(c => c.CategoryId == category.ParentId)
                .Select(c => c.CategoryName)
                .FirstOrDefault();
        }

        return Json(new
        {
            success = true,
            category = new CategoryViewModel
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                ParentId = category.ParentId,
                ParentName = parentName,
                IsActive = category.IsActive ?? true,
                CreatedAt = category.CreatedAt.ToString("yyyy-MM-dd")
            }
        });
    }

    [HttpPost]
    // Function: DeleteCategory
    // Purpose: Executes the DeleteCategory workflow for this module.
    public IActionResult DeleteCategory([FromBody] CategoryViewModel dto)
    {
        var category = _db.Categories.Find(dto.CategoryId);
        if (category == null)
            return Json(new { success = false, message = "Category not found." });

        // Check if parent has children
        var hasChildren = _db.Categories.Any(c => c.ParentId == dto.CategoryId);
        if (hasChildren)
            return Json(new { success = false, message = "Cannot delete a parent category that has sub-categories. Delete children first." });

        _db.Categories.Remove(category);
        _db.SaveChanges();

        return Json(new { success = true });
    }

    // Function: ProductsBrand
    // Purpose: Executes the ProductsBrand workflow for this module.
    public IActionResult ProductsBrand()
    {
        var brands = _db.Brands
            .OrderBy(b => b.BrandId)
            .Select(b => new BrandViewModel
            {
                BrandId = b.BrandId,
                BrandName = b.BrandName,
                Description = b.Description,
                IsActive = b.IsActive ?? true,
                CreatedAt = b.CreatedAt.ToString("yyyy-MM-dd")
            })
            .ToList();
        return View(brands);
    }

    [HttpPost]
    // Function: AddBrand
    // Purpose: Executes the AddBrand workflow for this module.
    public IActionResult AddBrand([FromBody] BrandViewModel dto)
    {
        if (string.IsNullOrWhiteSpace(dto.BrandName))
            return Json(new { success = false, message = "Brand name is required." });

        // Check duplicate
        if (_db.Brands.Any(b => b.BrandName == dto.BrandName))
            return Json(new { success = false, message = "Brand \"" + dto.BrandName + "\" already exists." });

        var brand = new Brand
        {
            BrandName = dto.BrandName.Trim(),
            Description = dto.Description?.Trim(),
            IsActive = dto.IsActive,
            CreatedAt = DateTime.Now
        };
        _db.Brands.Add(brand);
        _db.SaveChanges();

        return Json(new
        {
            success = true,
            brand = new BrandViewModel
            {
                BrandId = brand.BrandId,
                BrandName = brand.BrandName,
                Description = brand.Description,
                IsActive = brand.IsActive ?? true,
                CreatedAt = brand.CreatedAt.ToString("yyyy-MM-dd")
            }
        });
    }

    [HttpPost]
    // Function: EditBrand
    // Purpose: Executes the EditBrand workflow for this module.
    public IActionResult EditBrand([FromBody] BrandViewModel dto)
    {
        if (string.IsNullOrWhiteSpace(dto.BrandName))
            return Json(new { success = false, message = "Brand name is required." });

        var brand = _db.Brands.Find(dto.BrandId);
        if (brand == null)
            return Json(new { success = false, message = "Brand not found." });

        // Check duplicate (exclude self)
        if (_db.Brands.Any(b => b.BrandId != dto.BrandId && b.BrandName == dto.BrandName))
            return Json(new { success = false, message = "Brand \"" + dto.BrandName + "\" already exists." });

        brand.BrandName = dto.BrandName.Trim();
        brand.Description = dto.Description?.Trim();
        brand.IsActive = dto.IsActive;
        _db.SaveChanges();

        return Json(new
        {
            success = true,
            brand = new BrandViewModel
            {
                BrandId = brand.BrandId,
                BrandName = brand.BrandName,
                Description = brand.Description,
                IsActive = brand.IsActive ?? true,
                CreatedAt = brand.CreatedAt.ToString("yyyy-MM-dd")
            }
        });
    }

    [HttpPost]
    // Function: DeleteBrand
    // Purpose: Executes the DeleteBrand workflow for this module.
    public IActionResult DeleteBrand([FromBody] BrandViewModel dto)
    {
        var brand = _db.Brands.Find(dto.BrandId);
        if (brand == null)
            return Json(new { success = false, message = "Brand not found." });

        _db.Brands.Remove(brand);
        _db.SaveChanges();

        return Json(new { success = true });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    // Function: Error
    // Purpose: Executes the Error workflow for this module.
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    // Function: ProductsInventory
    // Purpose: Executes the ProductsInventory workflow for this module.
    public IActionResult ProductsInventory()
    {
        var products = _db.Products
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.ProductColors)
                .ThenInclude(c => c.ProductSizes)
            .OrderBy(p => p.ProductName)
            .Select(p => new InventoryProductViewModel
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Sku = p.Sku,
                BrandName = p.Brand.BrandName,
                CategoryName = p.Category.CategoryName,
                Status = p.Status,
                TotalStock = p.ProductColors
                    .SelectMany(c => c.ProductSizes)
                    .Sum(s => s.StockQuantity),
                Colors = p.ProductColors.Select(c => new InventoryColorViewModel
                {
                    ColorId = c.ColorId,
                    ColorName = c.ColorName,
                    Sizes = c.ProductSizes.Select(s => new InventorySizeViewModel
                    {
                        SizeId = s.SizeId,
                        SizeType = s.SizeType,
                        SizeValue = s.SizeValue,
                        StockQuantity = s.StockQuantity,
                        Status = s.Status
                    }).ToList()
                }).ToList()
            })
            .ToList();

        var allSizes = products.SelectMany(p => p.Colors).SelectMany(c => c.Sizes).ToList();

        var vm = new InventoryViewModel
        {
            Products = products,
            TotalProducts = products.Count,
            TotalStock = products.Sum(p => p.TotalStock),
            TotalSizes = allSizes.Count,
            LowStockSizes = allSizes.Count(s => s.StockQuantity > 0 && s.StockQuantity <= 10),
            OutOfStockSizes = allSizes.Count(s => s.StockQuantity == 0)
        };

        return View(vm);
    }

    [HttpPost]
    // Function: UpdateStock
    // Purpose: Executes the UpdateStock workflow for this module.
    public IActionResult UpdateStock([FromBody] UpdateStockRequest req)
    {
        var size = _db.ProductSizes.Find(req.SizeId);
        if (size == null)
            return Json(new { success = false, message = "Size not found." });

        size.StockQuantity = req.StockQuantity;
        _db.SaveChanges();

        return Json(new { success = true, stockQuantity = size.StockQuantity });
    }
}
