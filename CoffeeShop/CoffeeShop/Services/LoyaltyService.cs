using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Models;

namespace CoffeeShop.Services;

public class LoyaltyService
{
    public async Task<decimal> GetBalanceAsync(int userId)
    {
        await using var db = new AppDbContext();
        var user = await db.Users.FindAsync(userId);
        return user?.BonusBalance ?? 0;
    }

    public async Task<List<LoyaltyTransaction>> GetTransactionsAsync(int userId)
    {
        await using var db = new AppDbContext();
        return await db.LoyaltyTransactions
            .Include(lt => lt.Order)
            .Where(lt => lt.UserId == userId)
            .OrderByDescending(lt => lt.CreatedAt)
            .ToListAsync();
    }
}
