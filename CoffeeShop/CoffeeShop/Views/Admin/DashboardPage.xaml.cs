using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Helpers;
using CoffeeShop.Services;

namespace CoffeeShop.Views.Admin;

public class TopDrinkItem
{
    public string Rank { get; set; } = "";
    public string Name { get; set; } = "";
    public string CountText { get; set; } = "";
}

public partial class DashboardPage : Page
{
    private readonly OrderService _orderService = new();
    private readonly StaffService _staffService = new();
    private readonly ShiftService _shiftService = new();
    private readonly AdminShell _shell;

    public DashboardPage(AdminShell shell)
    {
        InitializeComponent();
        _shell = shell;
        Loaded += async (_, _) => await LoadDashboard();
    }

    private async Task LoadDashboard()
    {
        var culture = new CultureInfo("ru-RU");
        DateText.Text = DateTime.Now.ToString("dd MMMM yyyy", culture);

        var shift = SessionManager.CurrentShift;

        var ordersCount = await _orderService.GetShiftOrdersCountAsync(shift?.Id);
        var revenue = await _orderService.GetShiftRevenueAsync(shift?.Id);
        var baristasOnShift = await _staffService.GetBaristasOnShiftCountAsync(shift?.Id);
        var totalBaristas = await _staffService.GetActiveBaristaCountAsync();

        RevenueText.Text = $"{revenue:N0} ₽";
        OrdersText.Text = ordersCount.ToString();
        BaristaCountText.Text = baristasOnShift.ToString();
        BaristaInfoText.Text = $"из {totalBaristas} активных";

        BtnOpenShift.Visibility = shift == null ? Visibility.Visible : Visibility.Collapsed;
        BtnCloseShift.Visibility = shift != null ? Visibility.Visible : Visibility.Collapsed;

        // Recent orders (all statuses)
        var orders = await _orderService.GetRecentOrdersAsync(10);
        RecentOrdersList.ItemsSource = orders;

        // Top drinks (all time)
        await using var db = new AppDbContext();
        var topDrinks = await db.OrderItems
            .Include(oi => oi.MenuItem)
            .GroupBy(oi => oi.MenuItem!.Name)
            .Select(g => new { Name = g.Key, Count = g.Sum(x => x.Quantity) })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync();

        TopDrinksList.ItemsSource = topDrinks.Select((d, i) => new TopDrinkItem
        {
            Rank = $"{i + 1}.",
            Name = d.Name,
            CountText = $"{d.Count} шт"
        }).ToList();
    }

    private async void OpenShiftClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var shift = await _shiftService.OpenShiftAsync(SessionManager.CurrentUser!.Id);
            SessionManager.CurrentShift = shift;
            await _shell.RefreshShift();
            await LoadDashboard();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void CloseShiftClick(object sender, RoutedEventArgs e)
    {
        if (SessionManager.CurrentShift == null) return;

        var shiftId = SessionManager.CurrentShift.Id;
        var revenue = await _orderService.GetShiftRevenueAsync(shiftId);
        var ordersCount = await _orderService.GetShiftOrdersCountAsync(shiftId);
        var earnings = await _shiftService.GetShiftEarningsAsync(shiftId);
        var bonusAccrued = earnings.Sum(e2 => e2.EarnedAmount);

        var dialog = new Dialogs.CloseShiftDialog(revenue, ordersCount, bonusAccrued);
        dialog.Owner = Window.GetWindow(this);
        if (dialog.ShowDialog() != true) return;

        try
        {
            await _shiftService.CloseShiftAsync(shiftId, SessionManager.CurrentUser!.Id);
            await _shell.RefreshShift();
            await LoadDashboard();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
