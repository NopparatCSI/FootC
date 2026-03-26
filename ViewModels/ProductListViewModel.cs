using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp1.ViewModels;

/// <summary>
/// ViewModel สำหรับหน้า ProductsList (ข้อมูลสินค้า + dropdown filter)
/// </summary>
public class ProductListViewModel
{
    public List<ProductListItemViewModel> Products { get; set; } = new();
    public List<SelectListItem> Categories { get; set; } = new();
    public List<SelectListItem> Brands { get; set; } = new();
}
