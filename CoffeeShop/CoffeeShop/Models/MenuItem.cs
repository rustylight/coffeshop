using System.Windows.Media;

namespace CoffeeShop.Models;

public class MenuItem
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; } = true;
    public string? ImagePath { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public MenuCategory Category { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public string StatusText => IsAvailable ? "Доступен" : "Недоступен";
    public Brush StatusBg => new SolidColorBrush(
        (Color)ColorConverter.ConvertFromString(IsAvailable ? "#D1FAE5" : "#FEE2E2"));
    public Brush StatusFg => new SolidColorBrush(
        (Color)ColorConverter.ConvertFromString(IsAvailable ? "#065F46" : "#DC2626"));
}
