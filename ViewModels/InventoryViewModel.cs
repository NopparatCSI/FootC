namespace WebApp1.ViewModels;

public class InventoryViewModel
{
    public List<InventoryProductViewModel> Products { get; set; } = new();
    public int TotalProducts { get; set; }
    public int TotalStock { get; set; }
    public int TotalSizes { get; set; }
    public int LowStockSizes { get; set; }
    public int OutOfStockSizes { get; set; }
}

public class InventoryProductViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int TotalStock { get; set; }
    public List<InventoryColorViewModel> Colors { get; set; } = new();
}

public class InventoryColorViewModel
{
    public int ColorId { get; set; }
    public string ColorName { get; set; } = string.Empty;
    public List<InventorySizeViewModel> Sizes { get; set; } = new();
}

public class InventorySizeViewModel
{
    public int SizeId { get; set; }
    public string SizeType { get; set; } = string.Empty;
    public string SizeValue { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class UpdateStockRequest
{
    public int SizeId { get; set; }
    public int StockQuantity { get; set; }
}
