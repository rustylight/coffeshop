using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using CoffeeShop.Helpers;
using CoffeeShop.Models;
using CoffeeShop.Services;

namespace CoffeeShop.Views.Admin;

public class ShiftDisplay
{
    public int Id { get; set; }
    public Shift Shift { get; set; } = null!;
    public string DisplayText => $"Смена #{Id} — {Shift.OpenedAt:dd.MM.yyyy}" +
                                  (Shift.IsClosed ? " (закрыта)" : " (активна)");
}

public partial class ShiftReportPage : Page
{
    private readonly ShiftService _shiftService = new();
    private readonly OrderService _orderService = new();
    private readonly ExcelExportService _excelService = new();
    private readonly LoyaltyService _loyaltyService = new();
    private List<StaffEarning> _currentEarnings = new();
    private Shift? _selectedShift;

    public ShiftReportPage()
    {
        InitializeComponent();
        Loaded += async (_, _) => await LoadShifts();
    }

    private AdminShell? Shell => Window.GetWindow(this) as AdminShell;

    private async Task LoadShifts()
    {
        var shifts = await _shiftService.GetAllShiftsAsync();
        var display = shifts.Select(s => new ShiftDisplay { Id = s.Id, Shift = s }).ToList();
        ShiftCombo.ItemsSource = display;
        if (display.Count > 0) ShiftCombo.SelectedIndex = 0;
    }

    private async void ShiftChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ShiftCombo.SelectedItem is ShiftDisplay sd)
        {
            _selectedShift = sd.Shift;
            await LoadReport(sd.Id);
        }
    }

    private async Task LoadReport(int shiftId)
    {
        var shift = await _shiftService.GetShiftByIdAsync(shiftId);
        if (shift == null) return;

        _selectedShift = shift;

        ShiftDateText.Text = shift.OpenedAt.ToString("dd MMMM yyyy");
        ShiftTimeText.Text = $"{shift.OpenedAt:HH:mm} — " +
            (shift.ClosedAt.HasValue ? shift.ClosedAt.Value.ToString("HH:mm") : "...");

        if (shift.IsClosed)
        {
            StatusText.Text = "Закрыта";
            StatusBorder.Background = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString("#FEE2E2"));
            StatusText.Foreground = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString("#DC2626"));
            BtnCloseShift.Visibility = Visibility.Collapsed;
        }
        else
        {
            StatusText.Text = "Активна";
            StatusBorder.Background = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString("#D1FAE5"));
            StatusText.Foreground = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString("#065F46"));
            BtnCloseShift.Visibility = Visibility.Visible;
        }

        RevenueText.Text = $"{shift.TotalRevenue:N0} ₽";

        var ordersCount = await _orderService.GetShiftOrdersCountAsync(shiftId);
        OrdersCountText.Text = ordersCount.ToString();

        _currentEarnings = await _shiftService.GetShiftEarningsAsync(shiftId);

        var bonusAccrued = _currentEarnings.Sum(e => e.EarnedAmount);
        BonusText.Text = bonusAccrued.ToString("N0");
        EarningsGrid.ItemsSource = _currentEarnings;
        EarningsPanel.Visibility = _currentEarnings.Count > 0
            ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ExportClick(object sender, RoutedEventArgs e)
    {
        if (_selectedShift == null) return;

        var dialog = new SaveFileDialog
        {
            Filter = "Excel файлы (*.xlsx)|*.xlsx",
            FileName = $"Отчёт_смены_{_selectedShift.Id}_{DateTime.Now:yyyyMMdd}.xlsx"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                _excelService.ExportShiftReport(_selectedShift, _currentEarnings, dialog.FileName);
                MessageBox.Show("Отчёт успешно экспортирован!", "Экспорт",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async void CloseShiftClick(object sender, RoutedEventArgs e)
    {
        if (_selectedShift == null) return;

        var revenue = _selectedShift.TotalRevenue;
        var ordersCount = await _orderService.GetShiftOrdersCountAsync(_selectedShift.Id);
        var bonusAccrued = _currentEarnings.Sum(e2 => e2.EarnedAmount);

        var dialog = new Dialogs.CloseShiftDialog(revenue, ordersCount, bonusAccrued);
        dialog.Owner = Window.GetWindow(this);
        if (dialog.ShowDialog() != true) return;

        try
        {
            await _shiftService.CloseShiftAsync(
                _selectedShift.Id, SessionManager.CurrentUser!.Id);
            if (Shell != null) await Shell.RefreshShift();
            else SessionManager.CurrentShift = null;
            await LoadShifts();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
