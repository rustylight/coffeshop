using System.Windows;
using System.Windows.Controls;
using CoffeeShop.Helpers;

namespace CoffeeShop.Views.Barista;

public partial class BaristaShell : Window
{
    private Button? _activeButton;

    public BaristaShell()
    {
        InitializeComponent();
        UserName.Text = SessionManager.GetShortName();
        UserPhone.Text = SessionManager.CurrentUser?.Phone ?? "";
        AvatarInitials.Text = SessionManager.GetInitials();
        _activeButton = BtnActive;

        if (SessionManager.CurrentShift != null && !SessionManager.CurrentShift.IsClosed)
            ShiftInfo.Text = $"≈ Смена активна · {SessionManager.CurrentShift.OpenedAt:HH:mm}";
        else
            ShiftInfo.Text = "Нет активной смены";

        Loaded += (_, _) => ContentFrame.Navigate(new BaristaActivePage());
    }

    private void SetActive(Button btn)
    {
        if (_activeButton != null) _activeButton.Style = (Style)FindResource("SidebarButton");
        btn.Style = (Style)FindResource("SidebarActiveButton");
        _activeButton = btn;
    }

    private void NavActive(object sender, RoutedEventArgs e)
    {
        SetActive(BtnActive);
        ContentFrame.Navigate(new BaristaActivePage());
    }

    private void NavCompleted(object sender, RoutedEventArgs e)
    {
        SetActive(BtnCompleted);
        ContentFrame.Navigate(new BaristaCompletedPage());
    }

    private void NavEarnings(object sender, RoutedEventArgs e)
    {
        SetActive(BtnEarnings);
        ContentFrame.Navigate(new BaristaEarningsPage());
    }

    private void LogoutClick(object sender, RoutedEventArgs e)
    {
        SessionManager.Clear();
        new AuthWindow().Show();
        Close();
    }
}
