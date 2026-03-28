namespace CoffeeShop.Models;

public class Shift
{
    public int Id { get; set; }
    public int OpenedBy { get; set; }
    public DateTime OpenedAt { get; set; } = DateTime.Now;
    public DateTime? ClosedAt { get; set; }
    public bool IsClosed { get; set; }
    public decimal TotalRevenue { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public User Admin { get; set; } = null!;
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<StaffEarning> StaffEarnings { get; set; } = new List<StaffEarning>();
}
