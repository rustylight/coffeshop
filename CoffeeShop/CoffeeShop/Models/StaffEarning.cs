namespace CoffeeShop.Models;

public class StaffEarning
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ShiftId { get; set; }
    public int OrdersCount { get; set; }
    public decimal OrdersTotal { get; set; }
    public decimal EarningPercent { get; set; }
    public decimal EarnedAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public User User { get; set; } = null!;
    public Shift Shift { get; set; } = null!;

    public string DateText => CreatedAt.ToString("dd.MM.yyyy");
    public string ShiftTimeText => Shift != null
        ? $"{Shift.OpenedAt:HH:mm}—{(Shift.ClosedAt ?? DateTime.Now):HH:mm}"
        : "";
}
