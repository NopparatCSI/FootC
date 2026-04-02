using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApp1.Models;
using WebApp1.Models.Db;

namespace WebApp1.Controllers;

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

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(string email, string password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ModelState.AddModelError("", "Email and password are required.");
            return View();
        }

        var admin = _context.Adminusers.FirstOrDefault(a => a.EmailAdmin == email && a.Password == password);
        if (admin != null)
        {
            // Save admin full name to session
            HttpContext.Session.SetString("AdminFullName", $"{admin.NameAdmin} {admin.LastnameAdmin}");
            // Login success, redirect to dashboard
            return RedirectToAction("Dashboard", "Admin");
        }
        else
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return View();
        }
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Remove("AdminFullName");
        HttpContext.Session.Clear();
        return RedirectToAction("Login", "Admin");
    }

    public IActionResult AdminList()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}