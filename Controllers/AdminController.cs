using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp1.Models;
using WebApp1.Models.Db;
using WebApp1.Security;
using WebApp1.ViewModels;

namespace WebApp1.Controllers;

[Authorize(Policy = AdminPolicies.AccessAdminArea)]
public class AdminController : Controller
{
    private readonly Csi402dbContext _context;
    private readonly IWebHostEnvironment _env;

    public AdminController(Csi402dbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // Function: Dashboard
    // Purpose: Executes the Dashboard workflow for this module.
    public IActionResult Dashboard(int days = 30)
    {
        var allowedRanges = new[] { 7, 30, 90 };
        if (!allowedRanges.Contains(days))
        {
            days = 30;
        }

        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var lastMonthStart = monthStart.AddMonths(-1);
        var rangeStart = now.Date.AddDays(-(days - 1));
        var previousRangeStart = rangeStart.AddDays(-days);

        var adminName = User.Identity?.Name ?? "Admin";
        var adminRole = User.FindFirstValue(ClaimTypes.Role) ?? AdminRoles.Admin;

        var allOrders = _context.Orders.AsNoTracking().ToList();
        var rangeOrders = allOrders
            .Where(o => o.OrderDate.Date >= rangeStart && o.OrderDate.Date <= now.Date)
            .ToList();

        var previousRangeOrders = allOrders
            .Where(o => o.OrderDate.Date >= previousRangeStart && o.OrderDate.Date < rangeStart)
            .ToList();

        var paidRangeRevenue = rangeOrders
            .Where(o => o.PaymentStatus == "Paid" && o.Status != "Cancelled")
            .Sum(o => o.TotalAmount);

        var paidPreviousRangeRevenue = previousRangeOrders
            .Where(o => o.PaymentStatus == "Paid" && o.Status != "Cancelled")
            .Sum(o => o.TotalAmount);

        var model = new AdminDashboardViewModel
        {
            AdminName = adminName,
            AdminRole = adminRole,
            SelectedRangeDays = days,
            TotalProducts = _context.Products.Count(),
            ActiveProducts = _context.Products.Count(p => p.Status == "Active"),
            TotalCustomers = _context.Customers.Count(),
            TotalOrders = _context.Orders.Count(),
            PendingOrders = _context.Orders.Count(o => o.Status == "Pending"),
            PaidOrders = _context.Orders.Count(o => o.PaymentStatus == "Paid"),
            RevenueInRange = paidRangeRevenue,
            RevenueInPreviousRange = paidPreviousRangeRevenue,
            RevenueThisMonth = _context.Orders
                .Where(o => o.PaymentStatus == "Paid" && o.Status != "Cancelled" && o.OrderDate >= monthStart)
                .Select(o => (decimal?)o.TotalAmount)
                .Sum() ?? 0m,
            RevenueLastMonth = _context.Orders
                .Where(o => o.PaymentStatus == "Paid" && o.Status != "Cancelled" && o.OrderDate >= lastMonthStart && o.OrderDate < monthStart)
                .Select(o => (decimal?)o.TotalAmount)
                .Sum() ?? 0m
        };

        model.OrderStatusBreakdown = _context.Orders
            .GroupBy(o => o.Status)
            .Select(g => new DashboardCountItemViewModel
            {
                Label = string.IsNullOrWhiteSpace(g.Key) ? "Unknown" : g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .Take(6)
            .ToList();

        var paidRevenueByDate = rangeOrders
            .Where(o => o.PaymentStatus == "Paid" && o.Status != "Cancelled")
            .GroupBy(o => o.OrderDate.Date)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.TotalAmount));

        model.DailyRevenue = Enumerable.Range(0, days)
            .Select(offset => rangeStart.AddDays(offset))
            .Select(date => new DashboardDailyRevenueItemViewModel
            {
                Label = date.ToString("dd MMM", System.Globalization.CultureInfo.InvariantCulture),
                Amount = paidRevenueByDate.GetValueOrDefault(date)
            })
            .ToList();

        var statusByDate = rangeOrders
            .GroupBy(o => o.OrderDate.Date)
            .ToDictionary(
                g => g.Key,
                g => new DashboardDailyStatusItemViewModel
                {
                    Pending = g.Count(x => x.Status == "Pending"),
                    Processing = g.Count(x => x.Status == "Processing"),
                    Shipped = g.Count(x => x.Status == "Shipped"),
                    Delivered = g.Count(x => x.Status == "Delivered"),
                    Cancelled = g.Count(x => x.Status == "Cancelled")
                });

        model.DailyOrderStatus = Enumerable.Range(0, days)
            .Select(offset => rangeStart.AddDays(offset))
            .Select(date =>
            {
                var value = statusByDate.GetValueOrDefault(date) ?? new DashboardDailyStatusItemViewModel();
                value.Label = date.ToString("dd MMM", System.Globalization.CultureInfo.InvariantCulture);
                return value;
            })
            .ToList();

        model.FunnelCreatedOrders = rangeOrders.Count;
        model.FunnelPaidOrders = rangeOrders.Count(o => o.PaymentStatus == "Paid");
        model.FunnelShippedOrders = rangeOrders.Count(o => o.Status == "Shipped" || o.Status == "Delivered");
        model.FunnelDeliveredOrders = rangeOrders.Count(o => o.Status == "Delivered");

        var monthlyData = _context.Orders
            .Where(o => o.PaymentStatus == "Paid" && o.Status != "Cancelled" && o.OrderDate >= monthStart.AddMonths(-5))
            .AsEnumerable()
            .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
            .ToDictionary(
                g => $"{g.Key.Year:D4}-{g.Key.Month:D2}",
                g => g.Sum(o => o.TotalAmount));

        model.MonthlyRevenue = Enumerable.Range(0, 6)
            .Select(offset => monthStart.AddMonths(-5 + offset))
            .Select(month =>
            {
                var key = $"{month.Year:D4}-{month.Month:D2}";
                return new DashboardMonthlyRevenueItemViewModel
                {
                    Label = month.ToString("MMM", System.Globalization.CultureInfo.InvariantCulture),
                    Amount = monthlyData.GetValueOrDefault(key)
                };
            })
            .ToList();

        model.ProductByCategory = _context.Products
            .Include(p => p.Category)
            .AsEnumerable()
            .GroupBy(p => p.Category?.CategoryName ?? "Uncategorized")
            .Select(g => new DashboardCountItemViewModel
            {
                Label = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToList();

        return View(model);
    }

    [AllowAnonymous]
    // Function: Login
    // Purpose: Executes the Login workflow for this module.
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(nameof(Dashboard));
        }

        return View(new AdminLoginViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    // Function: Login
    // Purpose: Executes the Login workflow for this module.
    public async Task<IActionResult> Login(AdminLoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var email = model.Email.Trim();
        var admin = _context.Adminusers.FirstOrDefault(a => a.EmailAdmin == email && a.Password == model.Password);
        if (admin == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        var roleName = AdminRoles.Normalize(admin.RoleName);
        var fullName = BuildFullName(admin.NameAdmin, admin.LastnameAdmin);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, admin.AdminUserId),
            new(ClaimTypes.Name, fullName),
            new(ClaimTypes.Email, admin.EmailAdmin ?? string.Empty),
            new(ClaimTypes.Role, roleName)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(14) : DateTimeOffset.UtcNow.AddHours(8)
            });

        HttpContext.Session.SetString("AdminFullName", fullName);
        HttpContext.Session.SetString("AdminRole", roleName);

        if (string.Equals(roleName, AdminRoles.Packer, StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction("OrdersList", "Order");
        }

        return RedirectToAction(nameof(Dashboard));
    }

    // Function: Logout
    // Purpose: Executes the Logout workflow for this module.
    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Remove("AdminFullName");
        HttpContext.Session.Remove("AdminRole");
        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [Authorize(Policy = AdminPolicies.ManageAdmins)]
    // Function: AdminList
    // Purpose: Executes the AdminList workflow for this module.
    public IActionResult AdminList()
    {
        return View(BuildAdminListViewModel(new AdminCreateViewModel()));
    }

    [HttpPost]
    [Authorize(Policy = AdminPolicies.ManageAdmins)]
    [ValidateAntiForgeryToken]
    // Function: CreateAdmin
    // Purpose: Executes the CreateAdmin workflow for this module.
    public IActionResult CreateAdmin(AdminCreateViewModel model)
    {
        if (!AdminRoles.IsValid(model.RoleName))
        {
            ModelState.AddModelError(nameof(model.RoleName), "Invalid role selected.");
        }

        var email = model.Email.Trim();
        if (_context.Adminusers.Any(a => a.EmailAdmin != null && a.EmailAdmin == email))
        {
            ModelState.AddModelError(nameof(model.Email), "This email is already used by another admin.");
        }

        if (!ModelState.IsValid)
        {
            return View("AdminList", BuildAdminListViewModel(model));
        }

        var lastAdmin = _context.Adminusers
            .OrderByDescending(a => a.AdminUserId)
            .FirstOrDefault();

        var admin = new Adminuser
        {
            AdminUserId = GenerateAdminId(lastAdmin?.AdminUserId),
            NameAdmin = model.FirstName.Trim(),
            LastnameAdmin = model.LastName.Trim(),
            EmailAdmin = email,
            Password = model.Password,
            PhonenumberAdmin = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : model.PhoneNumber.Trim(),
            BirthDate = model.BirthDate,
            RoleName = AdminRoles.Normalize(model.RoleName),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Adminusers.Add(admin);
        _context.SaveChanges();

        TempData["AdminMessage"] = $"Created admin {BuildFullName(admin.NameAdmin, admin.LastnameAdmin)} with role {admin.RoleName}.";
        return RedirectToAction(nameof(AdminList));
    }

    [HttpPost]
    [Authorize(Policy = AdminPolicies.ManageAdmins)]
    [ValidateAntiForgeryToken]
    // Function: UpdateRole
    // Purpose: Executes the UpdateRole workflow for this module.
    public IActionResult UpdateRole(AdminRoleUpdateViewModel model)
    {
        if (!ModelState.IsValid || !AdminRoles.IsValid(model.RoleName))
        {
            TempData["AdminError"] = "Unable to update role because the selected role is invalid.";
            return RedirectToAction(nameof(AdminList));
        }

        var admin = _context.Adminusers.FirstOrDefault(a => a.AdminUserId == model.AdminUserId);
        if (admin == null)
        {
            return NotFound();
        }

        admin.RoleName = AdminRoles.Normalize(model.RoleName);
        admin.UpdatedAt = DateTime.UtcNow;
        _context.SaveChanges();

        TempData["AdminMessage"] = $"Updated role for {BuildFullName(admin.NameAdmin, admin.LastnameAdmin)} to {admin.RoleName}.";
        return RedirectToAction(nameof(AdminList));
    }

    [Authorize(Policy = AdminPolicies.ManageAdmins)]
    // Function: AdminEdit
    // Purpose: Executes the AdminEdit workflow for this module.
    public IActionResult AdminEdit(string id)
    {
        var admin = _context.Adminusers.FirstOrDefault(a => a.AdminUserId == id);
        if (admin == null)
        {
            return NotFound();
        }

        var model = new AdminEditViewModel
        {
            AdminUserId = admin.AdminUserId,
            FirstName = admin.NameAdmin ?? string.Empty,
            LastName = admin.LastnameAdmin ?? string.Empty,
            Email = admin.EmailAdmin ?? string.Empty,
            PhoneNumber = admin.PhonenumberAdmin,
            BirthDate = admin.BirthDate,
            RoleName = AdminRoles.Normalize(admin.RoleName)
        };

        ViewBag.RoleOptions = BuildRoleOptions(model.RoleName);
        return View(model);
    }

    [HttpPost]
    [Authorize(Policy = AdminPolicies.ManageAdmins)]
    [ValidateAntiForgeryToken]
    // Function: AdminEdit
    // Purpose: Executes the AdminEdit workflow for this module.
    public IActionResult AdminEdit(AdminEditViewModel model)
    {
        if (!AdminRoles.IsValid(model.RoleName))
        {
            ModelState.AddModelError(nameof(model.RoleName), "Invalid role selected.");
        }

        var email = model.Email.Trim();
        if (_context.Adminusers.Any(a => a.AdminUserId != model.AdminUserId && a.EmailAdmin != null && a.EmailAdmin == email))
        {
            ModelState.AddModelError(nameof(model.Email), "This email is already used by another admin.");
        }

        var admin = _context.Adminusers.FirstOrDefault(a => a.AdminUserId == model.AdminUserId);
        if (admin == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            ViewBag.RoleOptions = BuildRoleOptions(model.RoleName);
            return View(model);
        }

        admin.NameAdmin = model.FirstName.Trim();
        admin.LastnameAdmin = model.LastName.Trim();
        admin.EmailAdmin = email;
        admin.PhonenumberAdmin = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : model.PhoneNumber.Trim();
        admin.BirthDate = model.BirthDate;
        admin.RoleName = AdminRoles.Normalize(model.RoleName);
        admin.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            admin.Password = model.Password;
        }

        _context.SaveChanges();

        TempData["AdminMessage"] = $"Updated admin {BuildFullName(admin.NameAdmin, admin.LastnameAdmin)} successfully.";
        return RedirectToAction(nameof(AdminList));
    }

    [HttpPost]
    [Authorize(Policy = AdminPolicies.ManageAdmins)]
    [ValidateAntiForgeryToken]
    // Function: DeleteAdmin
    // Purpose: Executes the DeleteAdmin workflow for this module.
    public IActionResult DeleteAdmin(string adminUserId)
    {
        if (string.IsNullOrWhiteSpace(adminUserId))
        {
            TempData["AdminError"] = "Unable to delete admin because the identifier is missing.";
            return RedirectToAction(nameof(AdminList));
        }

        var currentAdminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.Equals(currentAdminId, adminUserId, StringComparison.OrdinalIgnoreCase))
        {
            TempData["AdminError"] = "You cannot delete your own admin account.";
            return RedirectToAction(nameof(AdminList));
        }

        var admin = _context.Adminusers.FirstOrDefault(a => a.AdminUserId == adminUserId);
        if (admin == null)
        {
            TempData["AdminError"] = "Admin account not found.";
            return RedirectToAction(nameof(AdminList));
        }

        var fullName = BuildFullName(admin.NameAdmin, admin.LastnameAdmin);
        _context.Adminusers.Remove(admin);
        _context.SaveChanges();

        TempData["AdminMessage"] = $"Deleted admin {fullName} successfully.";
        return RedirectToAction(nameof(AdminList));
    }

    // ---- Banner Management ----

    [Authorize(Policy = AdminPolicies.ManageCatalog)]
    public IActionResult BannerManagement()
    {
        var banners = BannerStore.Load(_env.ContentRootPath);
        var model = new BannerManagementViewModel
        {
            SlideSlots = BannerStore.SlideSlotKeys
                .Select(k => new BannerSlotViewModel
                {
                    Key = k,
                    Label = BannerStore.SlotLabels[k],
                    SupportsMultiple = true,
                    ImagePaths = banners.SlideImages.TryGetValue(k, out var images)
                        ? images.Where(v => !string.IsNullOrWhiteSpace(v)).ToList()
                        : []
                })
                .ToList(),
            CategorySlots = BannerStore.SingleSlotKeys
                .Select(k => new BannerSlotViewModel
                {
                    Key = k,
                    Label = BannerStore.SlotLabels[k],
                    SupportsMultiple = false,
                    ImagePaths = banners.SingleImages.TryGetValue(k, out var image) && !string.IsNullOrWhiteSpace(image)
                        ? [image!]
                        : []
                })
                .ToList()
        };
        return View(model);
    }

    [HttpPost]
    [Authorize(Policy = AdminPolicies.ManageCatalog)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadBannerImage(string slotKey, IFormFile? image, List<IFormFile>? images)
    {
        if (!BannerStore.SlotKeys.Contains(slotKey))
            return BadRequest();

        var isSlideSlot = BannerStore.SlideSlotKeys.Contains(slotKey);
        var files = isSlideSlot
            ? (images ?? []).Where(f => f != null && f.Length > 0).ToList()
            : (image != null && image.Length > 0 ? [image] : []);

        if (files.Count == 0)
        {
            TempData["BannerError"] = "กรุณาเลือกไฟล์รูปภาพ";
            return RedirectToAction(nameof(BannerManagement));
        }
        string[] allowedExt = [".jpg", ".jpeg", ".png", ".webp", ".gif"];

        foreach (var file in files)
        {
            if (file.Length > 10L * 1024 * 1024)
            {
                TempData["BannerError"] = "ไฟล์มีขนาดใหญ่เกิน 10 MB";
                return RedirectToAction(nameof(BannerManagement));
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExt.Contains(ext))
            {
                TempData["BannerError"] = "รองรับเฉพาะไฟล์ภาพ (.jpg, .jpeg, .png, .webp, .gif)";
                return RedirectToAction(nameof(BannerManagement));
            }
        }

        var banners = BannerStore.Load(_env.ContentRootPath);

        var bannersDir = Path.Combine(_env.WebRootPath, "images", "banners");
        Directory.CreateDirectory(bannersDir);

        if (isSlideSlot)
        {
            foreach (var file in files)
            {
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                var fileName = slotKey + "_" + Guid.NewGuid().ToString("N") + ext;
                var filePath = Path.Combine(bannersDir, fileName);
                using (var fs = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fs);
                }

                banners.SlideImages[slotKey].Add("/images/banners/" + fileName);
            }

            BannerStore.Save(_env.ContentRootPath, banners);
            TempData["BannerMessage"] = $"อัพโหลดรูปภาพ {files.Count} ไฟล์สำหรับ {BannerStore.SlotLabels[slotKey]} เรียบร้อยแล้ว";
            return RedirectToAction(nameof(BannerManagement));
        }

        if (banners.SingleImages.TryGetValue(slotKey, out var oldImage) && !string.IsNullOrWhiteSpace(oldImage))
        {
            DeleteImageFileIfExists(oldImage);
        }

        var singleFile = files[0];
        var singleExt = Path.GetExtension(singleFile.FileName).ToLowerInvariant();
        var singleFileName = slotKey + "_" + Guid.NewGuid().ToString("N") + singleExt;
        var singleFilePath = Path.Combine(bannersDir, singleFileName);
        using (var fs = new FileStream(singleFilePath, FileMode.Create))
        {
            await singleFile.CopyToAsync(fs);
        }

        banners.SingleImages[slotKey] = "/images/banners/" + singleFileName;
        BannerStore.Save(_env.ContentRootPath, banners);

        TempData["BannerMessage"] = $"อัพโหลดรูปภาพสำหรับ {BannerStore.SlotLabels[slotKey]} เรียบร้อยแล้ว";
        return RedirectToAction(nameof(BannerManagement));
    }

    [HttpPost]
    [Authorize(Policy = AdminPolicies.ManageCatalog)]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteBannerImage(string slotKey, string? imagePath)
    {
        if (!BannerStore.SlotKeys.Contains(slotKey))
            return BadRequest();

        var banners = BannerStore.Load(_env.ContentRootPath);
        if (BannerStore.SlideSlotKeys.Contains(slotKey))
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                TempData["BannerError"] = "ไม่พบรูปภาพที่ต้องการลบ";
                return RedirectToAction(nameof(BannerManagement));
            }

            if (banners.SlideImages.TryGetValue(slotKey, out var images))
            {
                var target = images.FirstOrDefault(x => string.Equals(x, imagePath, StringComparison.Ordinal));
                if (target != null)
                {
                    DeleteImageFileIfExists(target);
                    images.Remove(target);
                    BannerStore.Save(_env.ContentRootPath, banners);
                }
            }

            TempData["BannerMessage"] = $"ลบรูปภาพจาก {BannerStore.SlotLabels[slotKey]} เรียบร้อยแล้ว";
            return RedirectToAction(nameof(BannerManagement));
        }

        if (banners.SingleImages.TryGetValue(slotKey, out var singleImage) && !string.IsNullOrWhiteSpace(singleImage))
        {
            DeleteImageFileIfExists(singleImage);
            banners.SingleImages[slotKey] = null;
            BannerStore.Save(_env.ContentRootPath, banners);
        }

        TempData["BannerMessage"] = $"ลบรูปภาพสำหรับ {BannerStore.SlotLabels[slotKey]} เรียบร้อยแล้ว";
        return RedirectToAction(nameof(BannerManagement));
    }

    [Authorize(Policy = AdminPolicies.ManageCatalog)]
    public IActionResult PromotionManagement(string? status)
    {
        return View(BuildPromotionManagementViewModel(new PromotionCreateViewModel(), status));
    }

    [HttpPost]
    [Authorize(Policy = AdminPolicies.ManageCatalog)]
    [ValidateAntiForgeryToken]
    public IActionResult CreatePromotion(PromotionManagementViewModel input)
    {
        var model = input.CreatePromotion ?? new PromotionCreateViewModel();
        var normalizedType = NormalizePromotionType(model.PromotionType);
        var normalizedStatus = NormalizePromotionStatus(model.Status);
        var normalizedCode = string.IsNullOrWhiteSpace(model.Code)
            ? null
            : model.Code.Trim().ToUpperInvariant();

        if (model.EndAt <= model.StartAt)
        {
            ModelState.AddModelError(nameof(model.EndAt), "End date must be later than start date.");
        }

        if (normalizedType == "Percent" && (model.DiscountValue <= 0 || model.DiscountValue > 100))
        {
            ModelState.AddModelError(nameof(model.DiscountValue), "Percent discount must be between 0.01 and 100.");
        }

        if (!string.IsNullOrWhiteSpace(normalizedCode)
            && _context.Promotions.Any(p => p.Code != null && p.Code == normalizedCode))
        {
            ModelState.AddModelError(nameof(model.Code), "This promotion code is already used.");
        }

        if (!ModelState.IsValid)
        {
            model.PromotionType = normalizedType;
            model.Status = normalizedStatus;
            model.Code = normalizedCode;
            return View(nameof(PromotionManagement), BuildPromotionManagementViewModel(model, null));
        }

        var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var promotion = new Promotion
        {
            Code = normalizedCode,
            Name = model.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim(),
            PromotionType = normalizedType,
            DiscountValue = model.DiscountValue,
            MinOrderAmount = model.MinOrderAmount,
            MaxDiscountAmount = model.MaxDiscountAmount,
            StartAt = model.StartAt,
            EndAt = model.EndAt,
            UsageLimitTotal = model.UsageLimitTotal,
            UsageLimitPerUser = model.UsageLimitPerUser,
            IsStackable = model.IsStackable,
            Priority = model.Priority,
            Status = normalizedStatus,
            CreatedBy = string.IsNullOrWhiteSpace(adminUserId) ? null : adminUserId,
            CreatedAt = DateTime.Now
        };

        _context.Promotions.Add(promotion);
        _context.SaveChanges();

        TempData["PromotionMessage"] = $"Created promotion {promotion.Name} successfully.";
        return RedirectToAction(nameof(PromotionManagement));
    }

    [HttpPost]
    [Authorize(Policy = AdminPolicies.ManageCatalog)]
    [ValidateAntiForgeryToken]
    public IActionResult EditPromotion(PromotionEditViewModel input)
    {
        var normalizedType = NormalizePromotionType(input.PromotionType);
        var normalizedStatus = NormalizePromotionStatus(input.Status);
        var normalizedCode = string.IsNullOrWhiteSpace(input.Code)
            ? null
            : input.Code.Trim().ToUpperInvariant();

        if (input.EndAt <= input.StartAt)
            ModelState.AddModelError(nameof(input.EndAt), "End date must be later than start date.");

        if (normalizedType == "Percent" && (input.DiscountValue < 0 || input.DiscountValue > 100))
            ModelState.AddModelError(nameof(input.DiscountValue), "Percent discount must be between 0 and 100.");

        if (!string.IsNullOrWhiteSpace(normalizedCode)
            && _context.Promotions.Any(p => p.Code == normalizedCode && p.PromotionId != input.PromotionId))
            ModelState.AddModelError(nameof(input.Code), "This promotion code is already used.");

        if (!ModelState.IsValid)
        {
            TempData["PromotionError"] = string.Join(" | ", ModelState.Values
                .SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return RedirectToAction(nameof(PromotionManagement));
        }

        var promotion = _context.Promotions.Find(input.PromotionId);
        if (promotion == null)
            return NotFound();

        promotion.Code = normalizedCode;
        promotion.Name = input.Name.Trim();
        promotion.Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim();
        promotion.PromotionType = normalizedType;
        promotion.DiscountValue = input.DiscountValue;
        promotion.MinOrderAmount = input.MinOrderAmount;
        promotion.MaxDiscountAmount = input.MaxDiscountAmount;
        promotion.StartAt = input.StartAt;
        promotion.EndAt = input.EndAt;
        promotion.UsageLimitTotal = input.UsageLimitTotal;
        promotion.UsageLimitPerUser = input.UsageLimitPerUser;
        promotion.IsStackable = input.IsStackable;
        promotion.Priority = input.Priority;
        promotion.Status = normalizedStatus;
        promotion.UpdatedAt = DateTime.Now;

        _context.SaveChanges();

        TempData["PromotionMessage"] = $"Updated promotion \"{promotion.Name}\" successfully.";
        return RedirectToAction(nameof(PromotionManagement));
    }

    [HttpPost]
    [Authorize(Policy = AdminPolicies.ManageCatalog)]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePromotion(int promotionId)
    {
        var promotion = _context.Promotions.Find(promotionId);
        if (promotion == null)
            return NotFound();

        _context.Promotions.Remove(promotion);
        _context.SaveChanges();

        TempData["PromotionMessage"] = $"Deleted promotion \"{promotion.Name}\".";
        return RedirectToAction(nameof(PromotionManagement));
    }

    [AllowAnonymous]
    // Function: AccessDenied
    // Purpose: Executes the AccessDenied workflow for this module.
    public IActionResult AccessDenied()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    // Function: Error
    // Purpose: Executes the Error workflow for this module.
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    // Function: BuildAdminListViewModel
    // Purpose: Executes the BuildAdminListViewModel workflow for this module.
    private AdminListViewModel BuildAdminListViewModel(AdminCreateViewModel createAdmin)
    {
        var admins = _context.Adminusers
            .OrderBy(a => a.NameAdmin)
            .ThenBy(a => a.LastnameAdmin)
            .Select(a => new AdminListItemViewModel
            {
                AdminUserId = a.AdminUserId,
                FullName = BuildFullName(a.NameAdmin, a.LastnameAdmin),
                Email = a.EmailAdmin ?? "-",
                PhoneNumber = a.PhonenumberAdmin ?? "-",
                BirthDateText = a.BirthDate.HasValue ? a.BirthDate.Value.ToString("yyyy-MM-dd") : "-",
                RoleName = AdminRoles.Normalize(a.RoleName),
                RoleBadgeClass = GetRoleBadgeClass(a.RoleName)
            })
            .ToList();

        return new AdminListViewModel
        {
            Admins = admins,
            CreateAdmin = createAdmin,
            RoleOptions = AdminRoles.All
                .Select(role => new SelectListItem(role, role, string.Equals(role, createAdmin.RoleName, StringComparison.OrdinalIgnoreCase)))
                .ToList(),
            CurrentAdminRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty
        };
    }

    // Function: BuildRoleOptions
    // Purpose: Executes the BuildRoleOptions workflow for this module.
    private static List<SelectListItem> BuildRoleOptions(string selectedRole)
    {
        return AdminRoles.All
            .Select(role => new SelectListItem(role, role, string.Equals(role, selectedRole, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    // Function: BuildFullName
    // Purpose: Executes the BuildFullName workflow for this module.
    private static string BuildFullName(string? firstName, string? lastName)
    {
        var fullName = string.Join(' ', new[] { firstName, lastName }.Where(value => !string.IsNullOrWhiteSpace(value)));
        return string.IsNullOrWhiteSpace(fullName) ? "Unknown Admin" : fullName;
    }

    // Function: GenerateAdminId
    // Purpose: Executes the GenerateAdminId workflow for this module.
    private static string GenerateAdminId(string? currentAdminId)
    {
        if (!string.IsNullOrWhiteSpace(currentAdminId) && currentAdminId.Length > 3 && int.TryParse(currentAdminId[3..], out var number))
        {
            return $"ADM{number + 1:000}";
        }

        return "ADM001";
    }

    private PromotionManagementViewModel BuildPromotionManagementViewModel(PromotionCreateViewModel createPromotion, string? statusFilter)
    {
        var normalizedStatusFilter = NormalizePromotionStatusFilter(statusFilter);

        var query = _context.Promotions.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(normalizedStatusFilter))
        {
            query = query.Where(p => p.Status == normalizedStatusFilter);
        }

        var promotions = query
            .OrderByDescending(p => p.CreatedAt)
            .ThenBy(p => p.Priority)
            .Select(p => new PromotionListItemViewModel
            {
                PromotionId = p.PromotionId,
                Code = p.Code,
                Name = p.Name,
                Description = p.Description,
                PromotionType = p.PromotionType,
                DiscountValue = p.DiscountValue,
                MinOrderAmount = p.MinOrderAmount,
                MaxDiscountAmount = p.MaxDiscountAmount,
                UsageLimitTotal = p.UsageLimitTotal,
                UsageLimitPerUser = p.UsageLimitPerUser,
                StartAt = p.StartAt,
                EndAt = p.EndAt,
                Status = p.Status,
                IsStackable = p.IsStackable,
                Priority = p.Priority,
                CreatedBy = p.CreatedBy
            })
            .ToList();

        return new PromotionManagementViewModel
        {
            SelectedStatus = normalizedStatusFilter ?? string.Empty,
            Promotions = promotions,
            CreatePromotion = createPromotion,
            TotalPromotions = _context.Promotions.Count(),
            ActivePromotions = _context.Promotions.Count(p => p.Status == "Active")
        };
    }

    private static string NormalizePromotionType(string? promotionType)
    {
        if (string.Equals(promotionType, "FixedAmount", StringComparison.OrdinalIgnoreCase))
        {
            return "FixedAmount";
        }

        if (string.Equals(promotionType, "FreeShipping", StringComparison.OrdinalIgnoreCase))
        {
            return "FreeShipping";
        }

        return "Percent";
    }

    private static string NormalizePromotionStatus(string? status)
    {
        if (string.Equals(status, "Draft", StringComparison.OrdinalIgnoreCase))
        {
            return "Draft";
        }

        if (string.Equals(status, "Paused", StringComparison.OrdinalIgnoreCase))
        {
            return "Paused";
        }

        if (string.Equals(status, "Expired", StringComparison.OrdinalIgnoreCase))
        {
            return "Expired";
        }

        return "Active";
    }

    private static string? NormalizePromotionStatusFilter(string? statusFilter)
    {
        if (string.IsNullOrWhiteSpace(statusFilter))
        {
            return null;
        }

        if (string.Equals(statusFilter, "Draft", StringComparison.OrdinalIgnoreCase))
        {
            return "Draft";
        }

        if (string.Equals(statusFilter, "Paused", StringComparison.OrdinalIgnoreCase))
        {
            return "Paused";
        }

        if (string.Equals(statusFilter, "Expired", StringComparison.OrdinalIgnoreCase))
        {
            return "Expired";
        }

        if (string.Equals(statusFilter, "Active", StringComparison.OrdinalIgnoreCase))
        {
            return "Active";
        }

        return null;
    }

    // Function: GetRoleBadgeClass
    // Purpose: Executes the GetRoleBadgeClass workflow for this module.
    private static string GetRoleBadgeClass(string? roleName)
    {
        return AdminRoles.Normalize(roleName) switch
        {
            AdminRoles.SuperAdmin => "role-super-admin",
            AdminRoles.Admin => "role-catalog-manager",
            AdminRoles.Packer => "role-order-manager",
            _ => "role-catalog-manager"
        };
    }

    private void DeleteImageFileIfExists(string imagePath)
    {
        var oldFile = Path.Combine(
            _env.WebRootPath,
            imagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (System.IO.File.Exists(oldFile))
        {
            System.IO.File.Delete(oldFile);
        }
    }
}
