using System;
using System.Collections.Generic;

namespace WebApp1.Models.Db;

public partial class Customer
{
    public int UserId { get; set; }

    public string? NameCustomer { get; set; }

    public string? LastnameCustomer { get; set; }

    public string? EmailCustomer { get; set; }

    public string? Password { get; set; }

    public string? Phonenumber { get; set; }

    public DateOnly? BirthDate { get; set; }

    public DateTime? Utimestamp { get; set; }
}
