using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
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

    public AdminController(Csi402dbContext context)
    {
        _context = context;
    }

    public IActionResult Dashboard()
    {
        return View();
    }

    [AllowAnonymous]
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

        return RedirectToAction(nameof(Dashboard));
    }

    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Remove("AdminFullName");
        HttpContext.Session.Remove("AdminRole");
        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [Authorize(Policy = AdminPolicies.ManageAdmins)]
    public IActionResult AdminList()
    {
        return View(BuildAdminListViewModel(new AdminCreateViewModel()));
    }

    [HttpPost]
    [Authorize(Policy = AdminPolicies.ManageAdmins)]
    [ValidateAntiForgeryToken]
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

    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

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

    private static List<SelectListItem> BuildRoleOptions(string selectedRole)
    {
        return AdminRoles.All
            .Select(role => new SelectListItem(role, role, string.Equals(role, selectedRole, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    private static string BuildFullName(string? firstName, string? lastName)
    {
        var fullName = string.Join(' ', new[] { firstName, lastName }.Where(value => !string.IsNullOrWhiteSpace(value)));
        return string.IsNullOrWhiteSpace(fullName) ? "Unknown Admin" : fullName;
    }

    private static string GenerateAdminId(string? currentAdminId)
    {
        if (!string.IsNullOrWhiteSpace(currentAdminId) && currentAdminId.Length > 3 && int.TryParse(currentAdminId[3..], out var number))
        {
            return $"ADM{number + 1:000}";
        }

        return "ADM001";
    }

    private static string GetRoleBadgeClass(string? roleName)
    {
        return AdminRoles.Normalize(roleName) switch
        {
            AdminRoles.SuperAdmin => "role-super-admin",
            AdminRoles.CatalogManager => "role-catalog-manager",
            AdminRoles.OrderManager => "role-order-manager",
            _ => "role-support"
        };
    }
}