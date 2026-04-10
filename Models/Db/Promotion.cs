namespace WebApp1.Models.Db;

public partial class Promotion
{
    public int PromotionId { get; set; }

    public string? Code { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string PromotionType { get; set; } = null!;

    public decimal DiscountValue { get; set; }

    public decimal? MinOrderAmount { get; set; }

    public decimal? MaxDiscountAmount { get; set; }

    public DateTime StartAt { get; set; }

    public DateTime EndAt { get; set; }

    public int? UsageLimitTotal { get; set; }

    public int? UsageLimitPerUser { get; set; }

    public bool IsStackable { get; set; }

    public int Priority { get; set; }

    public string Status { get; set; } = null!;

    public string? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
