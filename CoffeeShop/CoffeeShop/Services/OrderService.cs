using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Models;

namespace CoffeeShop.Services;

public class OrderService
{
    public async Task<int> CreateOrderAsync(int clientId, List<(int MenuItemId, int Quantity)> items, decimal bonusUsed)
    {
        await using var db = new AppDbContext();

        var itemsJson = "[" + string.Join(",",
            items.Select(i => $"{{\"menu_item_id\":{i.MenuItemId},\"quantity\":{i.Quantity}}}")) + "]";

        var result = await db.Database.ExecuteSqlRawAsync(
            "CALL sp_create_order({0}, {1}::jsonb, {2}, NULL)",
            clientId, itemsJson, bonusUsed);

        var order = await db.Orders
            .Where(o => o.ClientId == clientId)
            .OrderByDescending(o => o.CreatedAt)
            .FirstAsync();

        return order.Id;
    }

    public async Task<Order?> GetOrderWithItemsAsync(int orderId)
    {
        await using var db = new AppDbContext();
        return await db.Orders
            .Include(o => o.OrderItems).ThenInclude(oi => oi.MenuItem)
            .Include(o => o.Client)
            .Include(o => o.Barista)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task<List<Order>> GetActiveOrdersAsync()
    {
        await using var db = new AppDbContext();
        return await db.Orders
            .Include(o => o.OrderItems).ThenInclude(oi => oi.MenuItem)
            .Include(o => o.Client)
            .Include(o => o.Barista)
            .Where(o => o.Status != "completed")
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Order>> GetRecentOrdersAsync(int count = 10)
    {
        await using var db = new AppDbContext();
        return await db.Orders
            .Include(o => o.Client)
            .OrderByDescending(o => o.Id)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<Order>> GetCompletedOrdersByBaristaAsync(int baristaId, int? shiftId = null)
    {
        await using var db = new AppDbContext();
        var query = db.Orders
            .Include(o => o.OrderItems).ThenInclude(oi => oi.MenuItem)
            .Include(o => o.Client)
            .Where(o => o.BaristaId == baristaId && o.Status == "completed");

        if (shiftId.HasValue)
            query = query.Where(o => o.ShiftId == shiftId.Value);

        return await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
    }

    public async Task<List<Order>> GetClientOrdersAsync(int clientId)
    {
        await using var db = new AppDbContext();
        return await db.Orders
            .Include(o => o.OrderItems).ThenInclude(oi => oi.MenuItem)
            .Include(o => o.Barista)
            .Where(o => o.ClientId == clientId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<Order?> GetClientActiveOrderAsync(int clientId)
    {
        await using var db = new AppDbContext();
        return await db.Orders
            .Include(o => o.OrderItems).ThenInclude(oi => oi.MenuItem)
            .Where(o => o.ClientId == clientId && o.Status != "completed")
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateStatusAsync(int orderId, string newStatus, int? baristaId = null)
    {
        await using var db = new AppDbContext();
        var order = await db.Orders.FindAsync(orderId);
        if (order == null) return;

        order.Status = newStatus;
        if (baristaId.HasValue)
            order.BaristaId = baristaId.Value;

        await db.SaveChangesAsync();
    }

    public async Task CompleteOrderAsync(int orderId, int baristaId)
    {
        await using var db = new AppDbContext();
        await db.Database.ExecuteSqlRawAsync(
            "CALL sp_complete_order({0}, {1})", orderId, baristaId);
    }

    public async Task<int> GetShiftOrdersCountAsync(int? shiftId)
    {
        if (!shiftId.HasValue) return 0;
        await using var db = new AppDbContext();
        return await db.Orders.CountAsync(o => o.ShiftId == shiftId.Value);
    }

    public async Task<decimal> GetShiftRevenueAsync(int? shiftId)
    {
        if (!shiftId.HasValue) return 0;
        await using var db = new AppDbContext();
        return await db.Orders
            .Where(o => o.ShiftId == shiftId.Value && o.Status == "completed")
            .SumAsync(o => o.FinalAmount);
    }
}
