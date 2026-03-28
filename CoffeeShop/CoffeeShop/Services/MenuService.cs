using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Models;

namespace CoffeeShop.Services;

public class MenuService
{
    public async Task<List<MenuCategory>> GetCategoriesAsync()
    {
        await using var db = new AppDbContext();
        return await db.MenuCategories
            .OrderBy(c => c.SortOrder)
            .ToListAsync();
    }

    public async Task<List<MenuItem>> GetAvailableItemsAsync()
    {
        await using var db = new AppDbContext();
        return await db.MenuItems
            .Include(m => m.Category)
            .Where(m => m.IsAvailable)
            .OrderBy(m => m.Category.SortOrder)
            .ThenBy(m => m.Name)
            .ToListAsync();
    }

    public async Task<List<MenuItem>> GetAllItemsAsync()
    {
        await using var db = new AppDbContext();
        return await db.MenuItems
            .Include(m => m.Category)
            .OrderBy(m => m.Category.SortOrder)
            .ThenBy(m => m.Name)
            .ToListAsync();
    }

    public async Task ToggleAvailabilityAsync(int menuItemId)
    {
        await using var db = new AppDbContext();
        var item = await db.MenuItems.FindAsync(menuItemId);
        if (item != null)
        {
            item.IsAvailable = !item.IsAvailable;
            await db.SaveChangesAsync();
        }
    }

    public async Task AddMenuItemAsync(string name, string? description, decimal price, int categoryId)
    {
        await using var db = new AppDbContext();
        db.MenuItems.Add(new MenuItem
        {
            Name = name,
            Description = description,
            Price = price,
            CategoryId = categoryId,
            IsAvailable = true
        });
        await db.SaveChangesAsync();
    }

    public async Task UpdateMenuItemAsync(int id, string name, string? description, decimal price, int categoryId)
    {
        await using var db = new AppDbContext();
        var item = await db.MenuItems.FindAsync(id);
        if (item != null)
        {
            item.Name = name;
            item.Description = description;
            item.Price = price;
            item.CategoryId = categoryId;
            await db.SaveChangesAsync();
        }
    }

    public async Task DeleteMenuItemAsync(int id)
    {
        await using var db = new AppDbContext();
        var item = await db.MenuItems.FindAsync(id);
        if (item != null)
        {
            db.MenuItems.Remove(item);
            await db.SaveChangesAsync();
        }
    }
}
