using Microsoft.AspNetCore.Mvc;
using WebApp1.Models.Db;

namespace WebApp1.Controllers;

public class UserController : Controller
{
    private readonly Csi402dbContext _context;

    public UserController(Csi402dbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var customers = _context.Customers.ToList();
        return View("UserList", customers);
    }

    public IActionResult UserList()
    {
        var customers = _context.Customers.ToList();
        return View(customers);
    }

    public IActionResult UserEdit(int id)
    {
        var customer = _context.Customers.FirstOrDefault(c => c.UserId == id);
        if (customer == null)
        {
            return NotFound();
        }
        return View(customer);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UserEdit(Customer data)
    {
        var customer = _context.Customers.FirstOrDefault(c => c.UserId == data.UserId);
        if (customer == null)
        {
            return NotFound();
        }

        customer.NameCustomer = data.NameCustomer;
        customer.LastnameCustomer = data.LastnameCustomer;
        customer.EmailCustomer = data.EmailCustomer;
        customer.Phonenumber = data.Phonenumber;

        _context.Customers.Update(customer);
        _context.SaveChanges();

        return RedirectToAction("UserList");
    }
}
