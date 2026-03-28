using System.Windows.Controls;
using CoffeeShop.Helpers;
using CoffeeShop.Services;

namespace CoffeeShop.Views.Client;

public partial class OrderHistoryPage : Page
{
    private readonly OrderService _orderService = new();

    public OrderHistoryPage()
    {
        InitializeComponent();
        Loaded += async (_, _) => await LoadOrders();
    }

    private async Task LoadOrders()
    {
        if (SessionManager.CurrentUser == null) return;
        var orders = await _orderService.GetClientOrdersAsync(SessionManager.CurrentUser.Id);
        OrdersList.ItemsSource = orders;
        OrderCountText.Text = $"{orders.Count} заказов";
    }
}
