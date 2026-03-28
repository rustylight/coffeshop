using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Services;
using CoffeeShop.Views.Dialogs;

namespace CoffeeShop.Views.Admin;

public class StaffDisplay
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Phone { get; set; } = "";
    public bool IsActive { get; set; }
    public int OrdersCount { get; set; }
    public string EarningsText { get; set; } = "0 ₽";
    public string Initials
    {
        get
        {
            var parts = FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length >= 2 ? $"{parts[0][0]}{parts[1][0]}" : parts.Length > 0 ? $"{parts[0][0]}" : "?";
        }
    }
}

public partial class StaffManagementPage : Page
{
    private readonly StaffService _staffService = new();
    private readonly AuthService _authService = new();

    public StaffManagementPage()
    {
        InitializeComponent();
        Loaded += async (_, _) => await LoadStaff();
    }

    private async Task LoadStaff()
    {
        var baristas = await _staffService.GetBaristasAsync();

        await using var db = new AppDbContext();
        var displays = new List<StaffDisplay>();

        foreach (var b in baristas)
        {
            var totalOrders = await db.Orders.CountAsync(o => o.BaristaId == b.Id);
            var totalEarnings = await db.StaffEarnings
                .Where(se => se.UserId == b.Id)
                .SumAsync(se => (decimal?)se.EarnedAmount) ?? 0;

            displays.Add(new StaffDisplay
            {
                Id = b.Id,
                FullName = b.FullName,
                Phone = b.Phone ?? "",
                IsActive = b.IsActive,
                OrdersCount = totalOrders,
                EarningsText = $"{totalEarnings:N0} ₽"
            });
        }

        StaffList.ItemsSource = displays;
    }

    private async void AddBaristaClick(object sender, RoutedEventArgs e)
    {
        var dialog = new AddBaristaDialog();
        dialog.Owner = Window.GetWindow(this);
        if (dialog.ShowDialog() == true)
        {
            try
            {
                await _authService.RegisterBaristaAsync(
                    dialog.BaristaName, dialog.BaristaPhone, dialog.BaristaPassword);
                await LoadStaff();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async void EditClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is StaffDisplay staff)
        {
            var dialog = new EditBaristaDialog(staff.Id, staff.FullName, staff.Phone);
            dialog.Owner = Window.GetWindow(this);
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    await _staffService.UpdateBaristaAsync(dialog.BaristaId, dialog.BaristaName, dialog.BaristaPhone);
                    await LoadStaff();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    private async void DeactivateClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is StaffDisplay staff)
        {
            var dialog = new Dialogs.ConfirmDeleteDialog(staff.FullName);
            dialog.Owner = Window.GetWindow(this);
            if (dialog.ShowDialog() == true)
            {
                await _staffService.DeactivateUserAsync(staff.Id);
                await LoadStaff();
            }
        }
    }
}
