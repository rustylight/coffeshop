using Microsoft.EntityFrameworkCore;
using CoffeeShop.Models;

namespace CoffeeShop.Data;

public class AppDbContext : DbContext
{
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<MenuCategory> MenuCategories => Set<MenuCategory>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<LoyaltyTransaction> LoyaltyTransactions => Set<LoyaltyTransaction>();
    public DbSet<StaffEarning> StaffEarnings => Set<StaffEarning>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=coffeeshop;Username=postgres;Password=root");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // roles
        modelBuilder.Entity<Role>(e =>
        {
            e.ToTable("roles");
            e.HasKey(r => r.Id);
            e.Property(r => r.Id).HasColumnName("id");
            e.Property(r => r.Name).HasColumnName("name").HasMaxLength(50);
            e.Property(r => r.Description).HasColumnName("description");
        });

        // users
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(u => u.Id);
            e.Property(u => u.Id).HasColumnName("id");
            e.Property(u => u.FullName).HasColumnName("full_name").HasMaxLength(200);
            e.Property(u => u.Phone).HasColumnName("phone").HasMaxLength(20);
            e.Property(u => u.Email).HasColumnName("email").HasMaxLength(200);
            e.Property(u => u.PasswordHash).HasColumnName("password_hash").HasMaxLength(255);
            e.Property(u => u.RoleId).HasColumnName("role_id");
            e.Property(u => u.IsActive).HasColumnName("is_active");
            e.Property(u => u.BonusBalance).HasColumnName("bonus_balance").HasColumnType("decimal(10,2)");
            e.Property(u => u.CreatedAt).HasColumnName("created_at");
            e.Property(u => u.UpdatedAt).HasColumnName("updated_at");

            e.HasOne(u => u.Role).WithMany(r => r.Users).HasForeignKey(u => u.RoleId);
        });

        // menu_categories
        modelBuilder.Entity<MenuCategory>(e =>
        {
            e.ToTable("menu_categories");
            e.HasKey(c => c.Id);
            e.Property(c => c.Id).HasColumnName("id");
            e.Property(c => c.Name).HasColumnName("name").HasMaxLength(100);
            e.Property(c => c.SortOrder).HasColumnName("sort_order");
        });

        // menu_items
        modelBuilder.Entity<MenuItem>(e =>
        {
            e.ToTable("menu_items");
            e.HasKey(m => m.Id);
            e.Property(m => m.Id).HasColumnName("id");
            e.Property(m => m.CategoryId).HasColumnName("category_id");
            e.Property(m => m.Name).HasColumnName("name").HasMaxLength(200);
            e.Property(m => m.Description).HasColumnName("description");
            e.Property(m => m.Price).HasColumnName("price").HasColumnType("decimal(10,2)");
            e.Property(m => m.IsAvailable).HasColumnName("is_available");
            e.Property(m => m.ImagePath).HasColumnName("image_path").HasMaxLength(500);
            e.Property(m => m.CreatedAt).HasColumnName("created_at");
            e.Property(m => m.UpdatedAt).HasColumnName("updated_at");

            e.HasOne(m => m.Category).WithMany(c => c.MenuItems).HasForeignKey(m => m.CategoryId);
        });

        // shifts
        modelBuilder.Entity<Shift>(e =>
        {
            e.ToTable("shifts");
            e.HasKey(s => s.Id);
            e.Property(s => s.Id).HasColumnName("id");
            e.Property(s => s.OpenedBy).HasColumnName("opened_by");
            e.Property(s => s.OpenedAt).HasColumnName("opened_at");
            e.Property(s => s.ClosedAt).HasColumnName("closed_at");
            e.Property(s => s.IsClosed).HasColumnName("is_closed");
            e.Property(s => s.TotalRevenue).HasColumnName("total_revenue").HasColumnType("decimal(10,2)");
            e.Property(s => s.CreatedAt).HasColumnName("created_at");
            e.Property(s => s.UpdatedAt).HasColumnName("updated_at");

            e.HasOne(s => s.Admin).WithMany(u => u.OpenedShifts).HasForeignKey(s => s.OpenedBy);
        });

        // orders
        modelBuilder.Entity<Order>(e =>
        {
            e.ToTable("orders");
            e.HasKey(o => o.Id);
            e.Property(o => o.Id).HasColumnName("id");
            e.Property(o => o.ClientId).HasColumnName("client_id");
            e.Property(o => o.BaristaId).HasColumnName("barista_id");
            e.Property(o => o.ShiftId).HasColumnName("shift_id");
            e.Property(o => o.Status).HasColumnName("status").HasMaxLength(50);
            e.Property(o => o.TotalAmount).HasColumnName("total_amount").HasColumnType("decimal(10,2)");
            e.Property(o => o.BonusUsed).HasColumnName("bonus_used").HasColumnType("decimal(10,2)");
            e.Property(o => o.FinalAmount).HasColumnName("final_amount").HasColumnType("decimal(10,2)");
            e.Property(o => o.CreatedAt).HasColumnName("created_at");
            e.Property(o => o.UpdatedAt).HasColumnName("updated_at");

            e.HasOne(o => o.Client).WithMany(u => u.ClientOrders).HasForeignKey(o => o.ClientId);
            e.HasOne(o => o.Barista).WithMany(u => u.BaristaOrders).HasForeignKey(o => o.BaristaId);
            e.HasOne(o => o.Shift).WithMany(s => s.Orders).HasForeignKey(o => o.ShiftId);
        });

        // order_items
        modelBuilder.Entity<OrderItem>(e =>
        {
            e.ToTable("order_items");
            e.HasKey(oi => oi.Id);
            e.Property(oi => oi.Id).HasColumnName("id");
            e.Property(oi => oi.OrderId).HasColumnName("order_id");
            e.Property(oi => oi.MenuItemId).HasColumnName("menu_item_id");
            e.Property(oi => oi.Quantity).HasColumnName("quantity");
            e.Property(oi => oi.UnitPrice).HasColumnName("unit_price").HasColumnType("decimal(10,2)");
            e.Property(oi => oi.Subtotal).HasColumnName("subtotal").HasColumnType("decimal(10,2)");
            e.Property(oi => oi.CreatedAt).HasColumnName("created_at");

            e.HasOne(oi => oi.Order).WithMany(o => o.OrderItems).HasForeignKey(oi => oi.OrderId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(oi => oi.MenuItem).WithMany(m => m.OrderItems).HasForeignKey(oi => oi.MenuItemId);
        });

        // loyalty_transactions
        modelBuilder.Entity<LoyaltyTransaction>(e =>
        {
            e.ToTable("loyalty_transactions");
            e.HasKey(lt => lt.Id);
            e.Property(lt => lt.Id).HasColumnName("id");
            e.Property(lt => lt.UserId).HasColumnName("user_id");
            e.Property(lt => lt.OrderId).HasColumnName("order_id");
            e.Property(lt => lt.Amount).HasColumnName("amount").HasColumnType("decimal(10,2)");
            e.Property(lt => lt.TransactionType).HasColumnName("transaction_type").HasMaxLength(50);
            e.Property(lt => lt.Description).HasColumnName("description");
            e.Property(lt => lt.CreatedAt).HasColumnName("created_at");

            e.HasOne(lt => lt.User).WithMany(u => u.LoyaltyTransactions).HasForeignKey(lt => lt.UserId);
            e.HasOne(lt => lt.Order).WithMany(o => o.LoyaltyTransactions).HasForeignKey(lt => lt.OrderId);
        });

        // staff_earnings
        modelBuilder.Entity<StaffEarning>(e =>
        {
            e.ToTable("staff_earnings");
            e.HasKey(se => se.Id);
            e.Property(se => se.Id).HasColumnName("id");
            e.Property(se => se.UserId).HasColumnName("user_id");
            e.Property(se => se.ShiftId).HasColumnName("shift_id");
            e.Property(se => se.OrdersCount).HasColumnName("orders_count");
            e.Property(se => se.OrdersTotal).HasColumnName("orders_total").HasColumnType("decimal(10,2)");
            e.Property(se => se.EarningPercent).HasColumnName("earning_percent").HasColumnType("decimal(5,2)");
            e.Property(se => se.EarnedAmount).HasColumnName("earned_amount").HasColumnType("decimal(10,2)");
            e.Property(se => se.CreatedAt).HasColumnName("created_at");
            e.Property(se => se.UpdatedAt).HasColumnName("updated_at");

            e.HasOne(se => se.User).WithMany(u => u.StaffEarnings).HasForeignKey(se => se.UserId);
            e.HasOne(se => se.Shift).WithMany(s => s.StaffEarnings).HasForeignKey(se => se.ShiftId);
            e.HasIndex(se => new { se.UserId, se.ShiftId }).IsUnique();
        });

        // audit_log
        modelBuilder.Entity<AuditLog>(e =>
        {
            e.ToTable("audit_log");
            e.HasKey(a => a.Id);
            e.Property(a => a.Id).HasColumnName("id");
            e.Property(a => a.TableName).HasColumnName("table_name").HasMaxLength(100);
            e.Property(a => a.RecordId).HasColumnName("record_id");
            e.Property(a => a.Action).HasColumnName("action").HasMaxLength(50);
            e.Property(a => a.OldValues).HasColumnName("old_values").HasColumnType("jsonb");
            e.Property(a => a.NewValues).HasColumnName("new_values").HasColumnType("jsonb");
            e.Property(a => a.ChangedBy).HasColumnName("changed_by");
            e.Property(a => a.CreatedAt).HasColumnName("created_at");

            e.HasOne(a => a.ChangedByUser).WithMany().HasForeignKey(a => a.ChangedBy);
        });
    }
}
