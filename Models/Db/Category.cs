using System;
using System.Collections.Generic;

namespace WebApp1.Models.Db;

public partial class Category
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    /// <summary>
    /// NULL = parent category, มีค่า = sub-category
    /// </summary>
    public int? ParentId { get; set; }

    public bool? IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Category> InverseParent { get; set; } = new List<Category>();

    public virtual Category? Parent { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
