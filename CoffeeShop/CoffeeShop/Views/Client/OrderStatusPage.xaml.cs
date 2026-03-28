using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CoffeeShop.Helpers;
using CoffeeShop.Models;
using CoffeeShop.Services;

namespace CoffeeShop.Views.Client;

public partial class OrderStatusPage : Page
{
    private readonly OrderService _orderService = new();
    private readonly ClientShell _shell;
    private Order? _currentOrder;

    public OrderStatusPage(ClientShell shell)
    {
        InitializeComponent();
        _shell = shell;
        Loaded += async (_, _) => await LoadOrder();
    }

    private async Task LoadOrder()
    {
        if (SessionManager.CurrentUser == null) return;
        _currentOrder = await _orderService.GetClientActiveOrderAsync(SessionManager.CurrentUser.Id);

        if (_currentOrder == null)
        {
            NoOrderPanel.Visibility = Visibility.Visible;
            OrderPanel.Visibility = Visibility.Collapsed;
            return;
        }

        NoOrderPanel.Visibility = Visibility.Collapsed;
        OrderPanel.Visibility = Visibility.Visible;

        OrderSubtitle.Text = $"Заказ A{_currentOrder.Id}";
        ItemsList.ItemsSource = _currentOrder.OrderItems;
        TotalText.Text = $"{_currentOrder.FinalAmount:N0} ₽";

        UpdateSteps(_currentOrder.Status);

        if (_currentOrder.Status == "completed")
        {
            await _shell.RefreshBonus();
        }
    }

    private void UpdateSteps(string status)
    {
        var active = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6F4E37"));
        var done = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2C1810"));
        var doneLight = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C8956C"));
        var inactive = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5E7EB"));

        Step1.Background = inactive; Step1Text.Text = "1";
        Step2.Background = inactive; Step2Text.Text = "2";
        Step3.Background = inactive; Step3Text.Text = "3";
        Step4.Background = inactive; Step4Text.Text = "4";

        switch (status)
        {
            case "pending":
                Step1.Background = done; Step1Text.Text = "✓";
                break;
            case "in_progress":
                Step1.Background = done; Step1Text.Text = "✓";
                Step2.Background = active; Step2Text.Text = "2";
                break;
            case "ready":
                Step1.Background = done; Step1Text.Text = "✓";
                Step2.Background = done; Step2Text.Text = "✓";
                Step3.Background = active; Step3Text.Text = "3";
                break;
            case "completed":
                Step1.Background = done; Step1Text.Text = "✓";
                Step2.Background = done; Step2Text.Text = "✓";
                Step3.Background = done; Step3Text.Text = "✓";
                Step4.Background = done; Step4Text.Text = "✓";
                break;
        }
    }

    private void GoToMenuClick(object sender, RoutedEventArgs e)
    {
        _shell.NavigateToMenu();
    }
}
