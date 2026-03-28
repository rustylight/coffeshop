using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Models;

namespace CoffeeShop.Services;

public class StaffService
{
    public async Task<List<User>> GetBaristasAsync()
    {
        await using var db = new AppDbContext();
        return await db.Users
            .Include(u => u.Role)
            .Where(u => u.Role.Name == "Бариста" && u.IsActive)
            .OrderBy(u => u.FullName)
            .ToListAsync();
    }

    public async Task DeactivateUserAsync(int userId)
    {
        await using var db = new AppDbContext();
        var user = await db.Users.FindAsync(userId);
        if (user != null)
        {
            user.IsActive = false;
            await db.SaveChangesAsync();
        }
    }

    public async Task ActivateUserAsync(int userId)
    {
        await using var db = new AppDbContext();
        var user = await db.Users.FindAsync(userId);
        if (user != null)
        {
            user.IsActive = true;
            await db.SaveChangesAsync();
        }
    }

    public async Task UpdateBaristaAsync(int userId, string fullName, string phone)
    {
        await using var db = new AppDbContext();
        var user = await db.Users.FindAsync(userId);
        if (user != null)
        {
            user.FullName = fullName;
            user.Phone = phone;
            await db.SaveChangesAsync();
        }
    }

    public async Task<int> GetActiveBaristaCountAsync()
    {
        await using var db = new AppDbContext();
        return await db.Users
            .Include(u => u.Role)
            .CountAsync(u => u.Role.Name == "Бариста" && u.IsActive);
    }

    public async Task<int> GetBaristasOnShiftCountAsync(int? shiftId)
    {
        if (shiftId == null) return 0;
        await using var db = new AppDbContext();
        return await db.StaffEarnings
            .Where(se => se.ShiftId == shiftId)
            .Join(db.Users.Include(u => u.Role),
                se => se.UserId, u => u.Id,
                (se, u) => u)
            .Where(u => u.Role.Name == "Бариста")
            .CountAsync();
    }

    public async Task<StaffEarning?> GetBaristaCurrentShiftEarningsAsync(int baristaId, int shiftId)
    {
        await using var db = new AppDbContext();
        return await db.StaffEarnings
            .FirstOrDefaultAsync(se => se.UserId == baristaId && se.ShiftId == shiftId);
    }
}
