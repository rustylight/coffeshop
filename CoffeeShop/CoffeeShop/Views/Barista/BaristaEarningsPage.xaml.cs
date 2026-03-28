using System.Globalization;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Helpers;
using CoffeeShop.Services;

namespace CoffeeShop.Views.Barista;

public partial class BaristaEarningsPage : Page
{
    private readonly StaffService _staffService = new();

    public BaristaEarningsPage()
    {
        InitializeComponent();
        Loaded += async (_, _) => await LoadEarnings();
    }

    private async Task LoadEarnings()
    {
        if (SessionManager.CurrentUser == null) return;

        var culture = new CultureInfo("ru-RU");
        PeriodText.Text = DateTime.Now.ToString("MMMM yyyy", culture);
        PeriodText.Text = char.ToUpper(PeriodText.Text[0]) + PeriodText.Text[1..];

        await using var db = new AppDbContext();

        var monthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var monthEarnings = await db.StaffEarnings
            .Where(se => se.UserId == SessionManager.CurrentUser.Id && se.CreatedAt >= monthStart)
            .ToListAsync();

        var totalOrders = monthEarnings.Sum(e => e.OrdersCount);
        var totalSum = monthEarnings.Sum(e => e.OrdersTotal);

        EarnedText.Text = $"{totalSum:N0} ₽";
        OrdersCountText.Text = totalOrders.ToString();
        if (totalOrders > 0)
            AvgCheckText.Text = $"{totalSum / totalOrders:N0} ₽";

        var earnings = await db.StaffEarnings
            .Include(se => se.Shift)
            .Where(se => se.UserId == SessionManager.CurrentUser.Id)
            .OrderByDescending(se => se.ShiftId)
            .ToListAsync();
        EarningsGrid.ItemsSource = earnings;
    }
}
