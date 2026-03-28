using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Models;
using CoffeeShop.Helpers;

namespace CoffeeShop.Services;

public class AuthService
{
    public async Task<User?> LoginAsync(string phone, string password)
    {
        await using var db = new AppDbContext();
        var user = await db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Phone == phone && u.IsActive);

        if (user == null) return null;
        return PasswordHasher.Verify(password, user.PasswordHash) ? user : null;
    }

    public async Task<User> RegisterClientAsync(string fullName, string phone, string password)
    {
        await using var db = new AppDbContext();

        var exists = await db.Users.AnyAsync(u => u.Phone == phone);
        if (exists) throw new InvalidOperationException("Пользователь с таким телефоном уже существует");

        var clientRole = await db.Roles.FirstAsync(r => r.Name == "Клиент");

        var user = new User
        {
            FullName = fullName,
            Phone = phone,
            PasswordHash = PasswordHasher.Hash(password),
            RoleId = clientRole.Id,
            IsActive = true,
            BonusBalance = 0
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        user.Role = clientRole;
        return user;
    }

    public async Task<User> RegisterBaristaAsync(string fullName, string phone, string password)
    {
        await using var db = new AppDbContext();

        var exists = await db.Users.AnyAsync(u => u.Phone == phone);
        if (exists) throw new InvalidOperationException("Пользователь с таким телефоном уже существует");

        var baristaRole = await db.Roles.FirstAsync(r => r.Name == "Бариста");

        var user = new User
        {
            FullName = fullName,
            Phone = phone,
            PasswordHash = PasswordHasher.Hash(password),
            RoleId = baristaRole.Id,
            IsActive = true
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        user.Role = baristaRole;
        return user;
    }
}
