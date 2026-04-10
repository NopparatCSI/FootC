using System.ComponentModel.DataAnnotations;

namespace WebApp1.ViewModels;

public class PromotionManagementViewModel
{
    public string SelectedStatus { get; set; } = string.Empty;

    public List<PromotionListItemViewModel> Promotions { get; set; } = [];

    public PromotionCreateViewModel CreatePromotion { get; set; } = new();

    public int TotalPromotions { get; set; }

    public int ActivePromotions { get; set; }
}

public class PromotionListItemViewModel
{
    public int PromotionId { get; set; }

    public string? Code { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string PromotionType { get; set; } = string.Empty;

    public decimal DiscountValue { get; set; }

    public decimal? MinOrderAmount { get; set; }

    public decimal? MaxDiscountAmount { get; set; }

    public int? UsageLimitTotal { get; set; }

    public int? UsageLimitPerUser { get; set; }

    public DateTime StartAt { get; set; }

    public DateTime EndAt { get; set; }

    public string Status { get; set; } = string.Empty;

    public bool IsStackable { get; set; }

    public int Priority { get; set; }

    public string? CreatedBy { get; set; }
}

public class PromotionEditViewModel
{
    [Required]
    public int PromotionId { get; set; }

    [StringLength(50)]
    public string? Code { get; set; }

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    public string PromotionType { get; set; } = "Percent";

    [Range(0, 999999.99)]
    public decimal DiscountValue { get; set; }

    [Range(0, 999999.99)]
    public decimal? MinOrderAmount { get; set; }

    [Range(0, 999999.99)]
    public decimal? MaxDiscountAmount { get; set; }

    [Required]
    public DateTime StartAt { get; set; }

    [Required]
    public DateTime EndAt { get; set; }

    [Range(1, int.MaxValue)]
    public int? UsageLimitTotal { get; set; }

    [Range(1, int.MaxValue)]
    public int? UsageLimitPerUser { get; set; }

    public bool IsStackable { get; set; }

    [Range(1, 9999)]
    public int Priority { get; set; } = 100;

    [Required]
    public string Status { get; set; } = "Active";
}

public class PromotionCreateViewModel
{
    [StringLength(50)]
    public string? Code { get; set; }

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    public string PromotionType { get; set; } = "Percent";

    [Range(0.01, 999999.99)]
    public decimal DiscountValue { get; set; } = 10m;

    [Range(0, 999999.99)]
    public decimal? MinOrderAmount { get; set; }

    [Range(0, 999999.99)]
    public decimal? MaxDiscountAmount { get; set; }

    [Required]
    public DateTime StartAt { get; set; } = DateTime.Now;

    [Required]
    public DateTime EndAt { get; set; } = DateTime.Now.AddDays(30);

    [Range(1, int.MaxValue)]
    public int? UsageLimitTotal { get; set; }

    [Range(1, int.MaxValue)]
    public int? UsageLimitPerUser { get; set; }

    public bool IsStackable { get; set; }

    [Range(1, 9999)]
    public int Priority { get; set; } = 100;

    [Required]
    public string Status { get; set; } = "Active";
}
