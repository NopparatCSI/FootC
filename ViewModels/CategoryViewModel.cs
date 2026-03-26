namespace WebApp1.ViewModels;

public class CategoryViewModel
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public int? ParentId { get; set; }
    public string? ParentName { get; set; }
    public bool IsActive { get; set; }
    public string CreatedAt { get; set; } = "";
}
