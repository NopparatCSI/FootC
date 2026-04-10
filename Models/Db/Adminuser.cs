using System;
using System.Collections.Generic;

namespace WebApp1.Models.Db;

public partial class Adminuser
{
    public string AdminUserId { get; set; } = null!;

    public string? NameAdmin { get; set; }

    public string? LastnameAdmin { get; set; }

    public string? EmailAdmin { get; set; }

    public string? Password { get; set; }

    public string RoleName { get; set; } = "Admin";

    public string? PhonenumberAdmin { get; set; }

    public DateOnly? BirthDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
