namespace WebApp1.ViewModels;

public class BrandViewModel
{
    public int BrandId { get; set; }
    public string BrandName { get; set; } = "";
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public string CreatedAt { get; set; } = "";
}
