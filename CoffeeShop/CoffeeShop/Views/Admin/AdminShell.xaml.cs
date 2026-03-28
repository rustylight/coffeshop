using System.Windows;
using System.Windows.Controls;
using CoffeeShop.Helpers;
using CoffeeShop.Services;

namespace CoffeeShop.Views.Admin;

public partial class AdminShell : Window
{
    private Button? _activeButton;
    private readonly ShiftService _shiftService = new();

    public AdminShell()
    {
        InitializeComponent();
        UserName.Text = SessionManager.GetShortName();
        UserPhone.Text = SessionManager.CurrentUser?.Phone ?? "";
        AvatarInitials.Text = SessionManager.GetInitials();
        _activeButton = BtnDashboard;
        UpdateShiftInfo();
        Loaded += (_, _) => ContentFrame.Navigate(new DashboardPage(this));
    }

    public void UpdateShiftInfo()
    {
        if (SessionManager.CurrentShift != null && !SessionManager.CurrentShift.IsClosed)
            ShiftInfo.Text = $"≈ Смена активна · {SessionManager.CurrentShift.OpenedAt:HH:mm}";
        else
            ShiftInfo.Text = "Нет активной смены";
    }

    public async Task RefreshShift()
    {
        SessionManager.CurrentShift = await _shiftService.GetCurrentShiftAsync();
        UpdateShiftInfo();
    }

    private void SetActive(Button btn)
    {
        if (_activeButton != null) _activeButton.Style = (Style)FindResource("SidebarButton");
        btn.Style = (Style)FindResource("SidebarActiveButton");
        _activeButton = btn;
    }

    private void NavDashboard(object sender, RoutedEventArgs e)
    {
        SetActive(BtnDashboard);
        ContentFrame.Navigate(new DashboardPage(this));
    }

    private void NavMenu(object sender, RoutedEventArgs e)
    {
        SetActive(BtnMenu);
        ContentFrame.Navigate(new MenuManagementPage());
    }

    private void NavStaff(object sender, RoutedEventArgs e)
    {
        SetActive(BtnStaff);
        ContentFrame.Navigate(new StaffManagementPage());
    }

    private void NavReport(object sender, RoutedEventArgs e)
    {
        SetActive(BtnReport);
        ContentFrame.Navigate(new ShiftReportPage());
    }

    private void LogoutClick(object sender, RoutedEventArgs e)
    {
        SessionManager.Clear();
        new AuthWindow().Show();
        Close();
    }
}
