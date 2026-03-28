using System.Windows;
using System.Windows.Controls;
using CoffeeShop.Helpers;
using CoffeeShop.Services;

namespace CoffeeShop.Views.Client;

public partial class ClientShell : Window
{
    private readonly LoyaltyService _loyaltyService = new();
    private Button? _activeButton;

    public ClientShell()
    {
        InitializeComponent();
        UserName.Text = SessionManager.GetShortName();
        AvatarInitials.Text = SessionManager.GetInitials();
        _activeButton = BtnMenu;
        Loaded += async (_, _) =>
        {
            try
            {
                await RefreshBonus();
                ContentFrame.Navigate(new MenuPage(this));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        };
    }

    public void UpdateUserInfo()
    {
        UserName.Text = SessionManager.GetShortName();
        AvatarInitials.Text = SessionManager.GetInitials();
    }

    public async Task RefreshBonus()
    {
        if (SessionManager.CurrentUser == null) return;
        var balance = await _loyaltyService.GetBalanceAsync(SessionManager.CurrentUser.Id);
        SessionManager.CurrentUser.BonusBalance = balance;
        BonusText.Text = $"⭐ {balance:N0} бонусов";
    }

    private void SetActive(Button btn)
    {
        if (_activeButton != null) _activeButton.Style = (Style)FindResource("SidebarButton");
        btn.Style = (Style)FindResource("SidebarActiveButton");
        _activeButton = btn;
    }

    private void NavMenu(object sender, RoutedEventArgs e)
    {
        SetActive(BtnMenu);
        ContentFrame.Navigate(new MenuPage(this));
    }

    private void NavOrder(object sender, RoutedEventArgs e)
    {
        SetActive(BtnOrder);
        ContentFrame.Navigate(new OrderStatusPage(this));
    }

    private void NavHistory(object sender, RoutedEventArgs e)
    {
        SetActive(BtnHistory);
        ContentFrame.Navigate(new OrderHistoryPage());
    }

    private void NavProfile(object sender, RoutedEventArgs e)
    {
        SetActive(BtnProfile);
        ContentFrame.Navigate(new ClientProfilePage());
    }

    public void NavigateToOrderStatus()
    {
        SetActive(BtnOrder);
        ContentFrame.Navigate(new OrderStatusPage(this));
    }

    public void NavigateToMenu()
    {
        SetActive(BtnMenu);
        ContentFrame.Navigate(new MenuPage(this));
    }

    private void LogoutClick(object sender, RoutedEventArgs e)
    {
        SessionManager.Clear();
        new AuthWindow().Show();
        Close();
    }
}
