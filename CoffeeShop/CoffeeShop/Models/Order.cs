namespace CoffeeShop.Models;

public class Order
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public int? BaristaId { get; set; }
    public int? ShiftId { get; set; }
    public string Status { get; set; } = "pending";
    public decimal TotalAmount { get; set; }
    public decimal BonusUsed { get; set; }
    public decimal FinalAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public User Client { get; set; } = null!;
    public User? Barista { get; set; }
    public Shift? Shift { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<LoyaltyTransaction> LoyaltyTransactions { get; set; } = new List<LoyaltyTransaction>();

    public string ItemsSummary => OrderItems.Any()
        ? string.Join(", ", OrderItems.Select(oi =>
            oi.Quantity > 1 ? $"{oi.MenuItem?.Name} × {oi.Quantity}" : oi.MenuItem?.Name ?? "?"))
        : "";
}
