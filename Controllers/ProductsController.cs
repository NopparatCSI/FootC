using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp1.Models;
using WebApp1.Models.Db;
using WebApp1.ViewModels;

namespace WebApp1.Controllers;

public class ProductsController : Controller
{
    private readonly Csi402dbContext _db;
    private readonly IWebHostEnvironment _env;

    public ProductsController(Csi402dbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    public IActionResult Index()
    {
        return View();
    }

    // Customer-facing product page
    // Shows only what customers should see (no cost price / internal status flags / stock numbers)
    public IActionResult Product(int? id)
    {
        // Mock page: show only what customers should see.
        // (No DB read yet — UI demo only)
        var vm = new ProductCustomerViewModel
        {
            Id = id ?? 1,
            Name = "Example Runner",
            Brand = "BrandA",
            Category = "RunningShoes",
            Gender = "Unisex",
            RegularPrice = 3200m,
            SalePrice = 2990m,
            DiscountPercent = 6.56m,
            ShortDescription = "Lightweight running shoe with breathable mesh and responsive cushioning.",
            FullDescription = "รองเท้าวิ่งน้ำหนักเบา ใส่สบาย เหมาะสำหรับการวิ่งระยะสั้นถึงกลาง และการใช้งานประจำวัน",
            HighlightFeaturesHtml = SanitizeRichText("<ul><li><strong>Lightweight</strong> upper for all-day comfort</li><li>Breathable mesh keeps feet cool</li><li>Responsive cushioning for smooth rides</li></ul>"),
            TechnicalDetails = "Drop: 10mm; Material: Engineered mesh; Midsole: EVA foam; Outsole: Rubber; Weight: 250g",
            ThumbnailUrl = "/images/Running-Shoe-2.jpeg",
            GalleryUrls = new List<string> { "/images/Running-Shoe-2.jpeg", "/images/Untitled-design-5-2.jpg", "/images/images.jpg" },
            ColorNames = new List<string> { "Black", "White", "Red" }
        };

        return View(vm);
    }

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

    public IActionResult ProductsView()
    {
        return View();
    }

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
    public async Task<IActionResult> AddProduct(ProductAddViewModel form, IFormFile? thumbnailImage, List<IFormFile>? galleryImages)
    {
        // ── Determine status from action button ──
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

        // ── Save thumbnail image ──
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

        // ── Save gallery images ──
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

        // ── Save colors & sizes ──
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
    public IActionResult DeleteProduct(int id)
    {
        // ค้นหาสินค้าพร้อมข้อมูลที่เกี่ยวข้อง
        var product = _db.Products
            .Include(p => p.ProductColors)
                .ThenInclude(c => c.ProductSizes)
            .Include(p => p.ProductColors)
                .ThenInclude(c => c.ProductColorImages)
            .Include(p => p.ProductImages)
            .FirstOrDefault(p => p.ProductId == id);

        if (product == null)
            return Json(new { success = false, message = "Product not found." });

        // ลบข้อมูลที่เกี่ยวข้องทั้งหมด
        foreach (var color in product.ProductColors)
        {
            _db.ProductSizes.RemoveRange(color.ProductSizes);
            _db.ProductColorImages.RemoveRange(color.ProductColorImages);
        }
        _db.ProductColors.RemoveRange(product.ProductColors);
        _db.ProductImages.RemoveRange(product.ProductImages);
        _db.Products.Remove(product);
        _db.SaveChanges();

        // ลบไฟล์รูป
        string imgDir = Path.Combine(_env.WebRootPath, "images", "products", id.ToString());
        if (Directory.Exists(imgDir))
            Directory.Delete(imgDir, true);

        return Json(new { success = true });
    }

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
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

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