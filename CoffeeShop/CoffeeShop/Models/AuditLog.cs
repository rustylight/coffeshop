namespace CoffeeShop.Models;

public class AuditLog
{
    public int Id { get; set; }
    public string TableName { get; set; } = string.Empty;
    public int RecordId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public int? ChangedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public User? ChangedByUser { get; set; }
}
