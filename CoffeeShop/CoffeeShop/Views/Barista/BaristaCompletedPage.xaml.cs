using System.Windows.Controls;
using CoffeeShop.Helpers;
using CoffeeShop.Services;

namespace CoffeeShop.Views.Barista;

public partial class BaristaCompletedPage : Page
{
    private readonly OrderService _orderService = new();

    public BaristaCompletedPage()
    {
        InitializeComponent();
        Loaded += async (_, _) => await LoadOrders();
    }

    private async Task LoadOrders()
    {
        if (SessionManager.CurrentUser == null) return;
        var orders = await _orderService.GetCompletedOrdersByBaristaAsync(
            SessionManager.CurrentUser.Id, SessionManager.CurrentShift?.Id);
        OrdersList.ItemsSource = orders;
        TodayCountText.Text = $"Сегодня: {orders.Count} заказов";
    }
}
