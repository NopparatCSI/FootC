namespace WebApp1.Models.Db;

public partial class PromotionUsage
{
    public int UsageId { get; set; }

    public int PromotionId { get; set; }

    public int? OrderId { get; set; }

    public int? UserId { get; set; }

    public string? PromotionCode { get; set; }

    public decimal DiscountAmount { get; set; }

    public DateTime UsedAt { get; set; }
}
