using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApp1.Models;

namespace WebApp1.Controllers;

public class AdminController : Controller
{
    public IActionResult Dashboard()
    {
        return View();
    }
    public IActionResult Login()
    {
        return View();
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