using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp1.Models;
using WebApp1.ViewModels;
using WebApp1.Models.Db;

namespace WebApp1.Controllers;

public class AccountController : Controller
{
    private readonly Csi402dbContext _db;
    public AccountController(Csi402dbContext db)
    {
        _db = db;
    }
    // Function: Login
    // Purpose: Executes the Login workflow for this module.
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;

        if (HttpContext.Session.GetInt32("CustomerUserId").HasValue)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // Function: Login
    // Purpose: Executes the Login workflow for this module.
    public IActionResult Login(LoginUserViewModels data, string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;

        var email = (data.Email ?? string.Empty).Trim();
        var password = (data.Password ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError(string.Empty, "Please enter email and password.");
            return View(data);
        }

        var customer = _db.Customers
            .AsNoTracking()
            .FirstOrDefault(c => c.EmailCustomer == email && c.Password == password);

        if (customer == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(data);
        }

        HttpContext.Session.SetInt32("CustomerUserId", customer.UserId);
        HttpContext.Session.SetString("CustomerDisplayName", $"{customer.NameCustomer} {customer.LastnameCustomer}".Trim());
        HttpContext.Session.SetString("CustomerEmail", customer.EmailCustomer ?? email);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // Function: Logout
    // Purpose: Executes the Logout workflow for this module.
    public IActionResult Logout()
    {
        HttpContext.Session.Remove("CustomerUserId");
        HttpContext.Session.Remove("CustomerDisplayName");
        HttpContext.Session.Remove("CustomerEmail");

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    // Function: Register
    // Purpose: Executes the Register workflow for this module.
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // Function: Register
    // Purpose: Executes the Register workflow for this module.
    public IActionResult Register(RegisterViewModel data)
    {
        if (!ModelState.IsValid)
        {
            return View(data);
        }

        var email = (data.Email ?? string.Empty).Trim();
        var phoneNumber = (data.PhoneNumber ?? string.Empty).Trim();
        var emailExists = _db.Customers.Any(c => c.EmailCustomer != null && c.EmailCustomer == email);
        if (emailExists)
        {
            ModelState.AddModelError(nameof(data.Email), "This email is already registered.");
            return View(data);
        }

        var phoneExists = _db.Customers.Any(c => c.Phonenumber != null && c.Phonenumber == phoneNumber);
        if (phoneExists)
        {
            ModelState.AddModelError(nameof(data.PhoneNumber), "This phone number is already registered.");
            return View(data);
        }

        var c = new Customer
        {
            NameCustomer = data.Name,
            LastnameCustomer = data.LastName,
            EmailCustomer = email,
            Password = data.Password,
            Phonenumber = phoneNumber,
            BirthDate = data.DateOfBirth
        };

        try
        {
            _db.Customers.Add(c);
            _db.SaveChanges();
        }
        catch (DbUpdateException ex)
        {
            var dbMessage = ex.InnerException?.Message ?? ex.Message;
            if (dbMessage.Contains("UX_customer_Email_Customer", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(data.Email), "This email is already registered.");
            }
            else if (dbMessage.Contains("UX_customer_Phonenumber", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(data.PhoneNumber), "This phone number is already registered.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Unable to save data. Please try again.");
            }
            return View(data);
        }

        TempData["SuccessMessage"] = "Registration successful. Please log in.";
        return RedirectToAction("Login", "Account");
    }
    // Function: ForgotPassword
    // Purpose: Executes the ForgotPassword workflow for this module.
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // Function: ForgotPassword
    // Purpose: Executes the ForgotPassword workflow for this module.
    public IActionResult ForgotPassword(ForgotPasswordViewModel data)
    {
        if (!ModelState.IsValid)
        {
            return View(data);
        }

        ViewBag.Message = "If an account exists for that email, a reset link will be sent.";
        return View();
    }

    // Function: Lab4
    // Purpose: Executes the Lab4 workflow for this module.
    public IActionResult Lab4()
    {
        return View();
    }

    // Function: Lab5
    // Purpose: Executes the Lab5 workflow for this module.
    public IActionResult Lab5()
    {
        // var user = new LabUserViewModels();
        // user.UserId = "S66068773";
        // user.Name = "Nopparat"; 
        // user.LastName = "Suwanna";
        // user.Age = 22;
        // user.Address = "123/45 Moo 6 T. Bangna A. Bangna, Bangkok 10260";
        // user.Height = 175.5M;
        // user.Weight = 500.75M;
        // var user = new List<LabUserViewModels>
        // {
        //     new LabUserViewModels {UserId="S66068773", Name="Nopparat", LastName="Suwanna", Age=22, Address="123/45 Moo 6 T. Bangna A. Bangna, Bangkok 10260", Height=175.5M, Weight=500.75M},
        //     new LabUserViewModels {UserId="S66068774", Name="Somchai", LastName="Jaidee", Age=23, Address="456/78 Moo 7 T. Latkrabang A. Latkrabang, Bangkok 10520", Height=180.2M, Weight=650.30M},
        //     new LabUserViewModels {UserId="S66068775", Name="Somsri", LastName="Meesuk", Age=21, Address="789/12 Moo 8 T. Bangplee A. Bangplee, Samutprakarn 10540", Height=165.0M, Weight=480.00M}
        // };
        return View();
    }
    [HttpPost]
    // Function: Lab5
    // Purpose: Executes the Lab5 workflow for this module.
    public IActionResult Lab5(LabUserViewModels data)
    {
        string a, b, c;
        a = data.UserId;
        b = data.Name;
        c = data.LastName;
        @ViewBag.UserId = a;
        @ViewBag.Name = b;
        @ViewBag.LastName = c;
        return View();
    }

    // Function: Test1
    // Purpose: Executes the Test1 workflow for this module.
    public IActionResult Test1(string Email, string Password)
    {
        @ViewBag.Email = Email;
        @ViewBag.Password = Password;
        return View();
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

        ViewBag.lowPoints = lowPointsL;
        ViewBag.Total = total;
        return View();
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
}
