using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp1.Models.Db;
using WebApp1.Security;

namespace WebApp1.Controllers;

[Authorize(Policy = AdminPolicies.ManageUsers)]
public class UserController : Controller
{
    private readonly Csi402dbContext _context;

    public UserController(Csi402dbContext context)
    {
        _context = context;
    }

    // Function: Index
    // Purpose: Executes the Index workflow for this module.
    public IActionResult Index()
    {
        var customers = _context.Customers.ToList();
        return View("UserList", customers);
    }

    // Function: UserList
    // Purpose: Executes the UserList workflow for this module.
    public IActionResult UserList()
    {
        var customers = _context.Customers.ToList();
        return View(customers);
    }

    // Function: UserEdit
    // Purpose: Executes the UserEdit workflow for this module.
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
    // Function: UserEdit
    // Purpose: Executes the UserEdit workflow for this module.
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
