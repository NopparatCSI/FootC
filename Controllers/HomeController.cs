using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApp1.Models;
using WebApp1.Models.Db;
using WebApp1.ViewModels;

namespace WebApp1.Controllers;

public class HomeController : Controller
{
    private readonly Csi402dbContext _db;
    public HomeController(Csi402dbContext db)
    {
        _db = db;
    }

    public IActionResult lab8()
    {
        // var user = (from u in _db.Labstudents select u).ToList();
        // return View(user);
        return View();
    }

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
    public IActionResult Lab9List()
    {
        var customers = (from c in _db.Customers select c).ToList();
        return View(customers);
    }

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
                lowPointsL.Add("Pattern" + (i + 1) + " มีคะแนน: " + scores[i]);
            }
        }

        ViewBag.lowPoints = lowPointsL;
        ViewBag.Total = total;
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
