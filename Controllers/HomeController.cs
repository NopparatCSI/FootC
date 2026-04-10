using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp1.Models;
using WebApp1.Models.Db;
using WebApp1.ViewModels;

namespace WebApp1.Controllers;

public class HomeController : Controller
{
    private readonly Csi402dbContext _db;
    private readonly IWebHostEnvironment _env;
    public HomeController(Csi402dbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    // Function: lab8
    // Purpose: Executes the lab8 workflow for this module.
    public IActionResult lab8()
    {
        // var user = (from u in _db.Labstudents select u).ToList();
        // return View(user);
        return View();
    }

    // Function: lab9
    // Purpose: Executes the lab9 workflow for this module.
    public IActionResult lab9()
    {
        return View();
    }

    // [HttpPost]
    // public IActionResult lab9(Lab9User data)
    // {
    //     var u = new Labstudent();
    //     u.StdId = data.UserId;
    //     u.StdName = data.Name;
    //     u.StdLastName = data.Lastname;
    //     u.StdPassword = data.Password;
    //     // _db.Labstudents.Add(u);
    //     _db.SaveChanges();
    //     return RedirectToAction("Lab9List", "Home");
    // }
    // public IActionResult Lab9List()
    // {
    //     var user = (from u in _db.Labstudents select u).ToList();
    //     return View(user);
    // }

    // public IActionResult Lab9List()
    // {
    //     var users = (from u in _db.Labstudents select new Lab9User
    //     {
    //         UserId = u.StdId,
    //         Name = u.StdName,
    //         Lastname = u.StdLastName,
    //         Password = u.StdPassword
    //     }).ToList();
    //     return View(users);
    // }
    // Function: Lab9List
    // Purpose: Executes the Lab9List workflow for this module.
    public IActionResult Lab9List()
    {
        var customers = (from c in _db.Customers select c).ToList();
        return View(customers);
    }

    // Function: Lab10
    // Purpose: Executes the Lab10 workflow for this module.
    public IActionResult Lab10(int UserId)
    {
        var customer = _db.Customers.FirstOrDefault(u => u.UserId == UserId);
        if (customer == null)
        {
            return RedirectToAction("Lab9List", "Home");
        }

        return View(customer);
    }

    [HttpPost]
    // Function: Lab10
    // Purpose: Executes the Lab10 workflow for this module.
    public IActionResult Lab10(Customer data)
    {
        var user = (from u in _db.Customers where u.UserId == data.UserId select u).FirstOrDefault();
        if (user == null)
        {
            return RedirectToAction("Lab9List", "Home");
        }

        user.NameCustomer = data.NameCustomer;
        user.LastnameCustomer = data.LastnameCustomer;
        user.Password = data.Password;
        _db.Customers.Update(user);
        _db.SaveChanges();
        return RedirectToAction("Lab9List", "Home");
    }

    // Function: Lab10D
    // Purpose: Executes the Lab10D workflow for this module.
    public IActionResult Lab10D(int UserId)
    {
        var user =(from u in _db.Customers where u.UserId == UserId select u).FirstOrDefault();
        if (user == null)
        {
            return RedirectToAction("Lab9List", "Home");
        }
        _db.Customers.Remove(user);
        _db.SaveChanges();
        return RedirectToAction("Lab9List", "Home");
    }
    // Function: Index
    // Purpose: Executes the Index workflow for this module.
    public IActionResult Index()
    {
        string name, language;
        int year, room, total;
        name = "Nopparat Suwanna";
        year = 6;
        room = 2;
        language = "C# Programming";

        ViewBag.Name = name;
        ViewBag.Year = year;
        ViewBag.Room = room;
        ViewBag.Language = language;

        int p1 = 3, p2 = 7, p3 = 9, p4 = 4, p5 = 8, p6 = 7, p7 = 9, p8 = 1, p9 = 7, p10 = 6;
        total = p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9 + p10;
        int[] scores = { p1, p2, p3, p4, p5, p6, p7, p8, p9, p10 };

        if (total >= 80)
        {
            ViewBag.Grade = "A";
        }
        else if (total >= 76)
        {
            ViewBag.Grade = "B+";
        }
        else if (total >= 70)
        {
            ViewBag.Grade = "B";
        }
        else if (total >= 66)
        {
            ViewBag.Grade = "C+";
        }
        else if (total >= 60)
        {
            ViewBag.Grade = "C";
        }
        else if (total >= 56)
        {
            ViewBag.Grade = "D+";
        }
        else if (total >= 50)
        {
            ViewBag.Grade = "D";
        }
        else
        {
            ViewBag.Grade = "F";
        }

        List<string> lowPointsL = new List<string>();
        for (int i = 0; i < scores.Length; i++)
        {
            if (scores[i] < 5)
            {
                lowPointsL.Add("Pattern" + (i + 1) + " à¸¡à¸µà¸„à¸°à¹à¸™à¸™: " + scores[i]);
            }
        }

        var banners = BannerStore.Load(_env.ContentRootPath);
        var activeProducts = _db.Products
            .AsNoTracking()
            .Include(product => product.Brand)
            .Include(product => product.Category)
            .Include(product => product.ProductImages)
            .Where(product => product.Status == "Active")
            .ToList();

        var popularProducts = activeProducts
            .Where(product => product.ShowOnHomepage)
            .OrderByDescending(product => product.UpdatedAt ?? product.CreatedAt)
            .Take(4)
            .Select(MapHomeProductCard)
            .ToList();

        var saleProducts = activeProducts
            .Where(product => (product.SalePrice.HasValue && product.SalePrice.Value > 0 && product.SalePrice.Value < product.RegularPrice)
                || (product.DiscountPercent.HasValue && product.DiscountPercent.Value > 0))
            .OrderByDescending(product => product.DiscountPercent ?? 0)
            .ThenByDescending(product => product.UpdatedAt ?? product.CreatedAt)
            .Take(4)
            .Select(MapHomeProductCard)
            .ToList();

        ViewBag.lowPoints = lowPointsL;
        ViewBag.Total = total;

        var vm = new HomeIndexViewModel
        {
            SlideBanners = banners.SlideImages,
            CategoryBanners = banners.SingleImages,
            PopularProducts = popularProducts,
            SaleProducts = saleProducts
        };

        return View(vm);
    }

    // Function: Privacy
    // Purpose: Executes the Privacy workflow for this module.
    public IActionResult Privacy()
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

    private static HomeIndexProductCardViewModel MapHomeProductCard(Product product)
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

        return new HomeIndexProductCardViewModel
        {
            ProductId = product.ProductId,
            Name = product.ProductName,
            Brand = product.Brand.BrandName,
            Category = product.Category.CategoryName,
            ImageUrl = imageUrl,
            RegularPrice = product.RegularPrice,
            SalePrice = product.SalePrice,
            DiscountPercent = product.DiscountPercent
        };
    }
}
