namespace CoffeeShop.Models;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public bool IsActive { get; set; } = true;
    public decimal BonusBalance { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public Role Role { get; set; } = null!;
    public ICollection<Order> ClientOrders { get; set; } = new List<Order>();
    public ICollection<Order> BaristaOrders { get; set; } = new List<Order>();
    public ICollection<LoyaltyTransaction> LoyaltyTransactions { get; set; } = new List<LoyaltyTransaction>();
    public ICollection<StaffEarning> StaffEarnings { get; set; } = new List<StaffEarning>();
    public ICollection<Shift> OpenedShifts { get; set; } = new List<Shift>();
}
