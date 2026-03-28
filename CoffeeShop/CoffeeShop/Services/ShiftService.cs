using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Models;

namespace CoffeeShop.Services;

public class ShiftService
{
    public async Task<Shift> OpenShiftAsync(int adminId)
    {
        await using var db = new AppDbContext();
        var shift = new Shift
        {
            OpenedBy = adminId,
            OpenedAt = DateTime.Now,
            IsClosed = false,
            TotalRevenue = 0
        };
        db.Shifts.Add(shift);
        await db.SaveChangesAsync();
        return shift;
    }

    public async Task<Shift?> GetCurrentShiftAsync()
    {
        await using var db = new AppDbContext();
        return await db.Shifts
            .Include(s => s.Admin)
            .Where(s => !s.IsClosed)
            .OrderByDescending(s => s.OpenedAt)
            .FirstOrDefaultAsync();
    }

    public async Task CloseShiftAsync(int shiftId, int adminId)
    {
        await using var db = new AppDbContext();
        await db.Database.ExecuteSqlRawAsync(
            "CALL sp_close_shift({0}, {1})", shiftId, adminId);
    }

    public async Task<List<StaffEarning>> GetShiftEarningsAsync(int shiftId)
    {
        await using var db = new AppDbContext();
        return await db.StaffEarnings
            .Include(se => se.User).ThenInclude(u => u.Role)
            .Where(se => se.ShiftId == shiftId)
            .OrderByDescending(se => se.EarnedAmount)
            .ToListAsync();
    }

    public async Task<Shift?> GetShiftByIdAsync(int shiftId)
    {
        await using var db = new AppDbContext();
        return await db.Shifts
            .Include(s => s.Admin)
            .FirstOrDefaultAsync(s => s.Id == shiftId);
    }

    public async Task<List<Shift>> GetAllShiftsAsync()
    {
        await using var db = new AppDbContext();
        return await db.Shifts
            .Include(s => s.Admin)
            .OrderByDescending(s => s.OpenedAt)
            .ToListAsync();
    }
}
