using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CoffeeShop.Helpers;
using CoffeeShop.Models;
using CoffeeShop.Services;
using CoffeeShop.Views.Dialogs;

namespace CoffeeShop.Views.Barista;

public class OrderItemInfo
{
    public string Name { get; set; } = "";
    public int Quantity { get; set; }
    public decimal Subtotal { get; set; }
}

public class OrderDisplay
{
    public int Id { get; set; }
    public string ClientName { get; set; } = "";
    public List<string> Items { get; set; } = new();
    public List<OrderItemInfo> ItemDetails { get; set; } = new();
    public decimal FinalAmount { get; set; }
    public string Status { get; set; } = "";
    public string TimeText { get; set; } = "";

    public string StatusText => Status switch
    {
        "pending" => "Новый",
        "in_progress" => "В процессе",
        "ready" => "Готов",
        _ => Status
    };

    public Brush StatusBg => Status switch
    {
        "pending" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEF3C7")),
        "in_progress" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DBEAFE")),
        "ready" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D1FAE5")),
        _ => Brushes.LightGray
    };

    public Brush StatusFg => Status switch
    {
        "pending" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#92400E")),
        "in_progress" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E40AF")),
        "ready" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#065F46")),
        _ => Brushes.Gray
    };

    public string ActionText => Status switch
    {
        "pending" => "Принять в работу",
        "in_progress" => "Заказ готов ✓",
        "ready" => "Выдан клиенту",
        _ => ""
    };

    public Brush ActionBg => Status switch
    {
        "pending" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C8956C")),
        "in_progress" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6F4E37")),
        "ready" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2C1810")),
        _ => Brushes.Gray
    };

    public bool IsPending => Status == "pending";
    public bool IsInProgress => Status == "in_progress";
    public bool IsReady => Status == "ready";
}

public partial class BaristaActivePage : Page
{
    private readonly OrderService _orderService = new();

    public BaristaActivePage()
    {
        InitializeComponent();
        Loaded += async (_, _) => await LoadOrders();
    }

    private async Task LoadOrders()
    {
        var orders = await _orderService.GetActiveOrdersAsync();
        var displays = orders.Select(o => new OrderDisplay
        {
            Id = o.Id,
            ClientName = o.Client?.FullName ?? "Клиент",
            Items = o.OrderItems.Select(oi =>
                oi.Quantity > 1 ? $"{oi.MenuItem?.Name} × {oi.Quantity}" : oi.MenuItem?.Name ?? "?").ToList(),
            ItemDetails = o.OrderItems.Select(oi => new OrderItemInfo
            {
                Name = oi.MenuItem?.Name ?? "?",
                Quantity = oi.Quantity,
                Subtotal = oi.Quantity * (oi.MenuItem?.Price ?? 0)
            }).ToList(),
            FinalAmount = o.FinalAmount,
            Status = o.Status,
            TimeText = o.CreatedAt.ToString("HH:mm")
        }).ToList();

        OrdersList.ItemsSource = displays;
        QueueCountText.Text = $"{displays.Count} в очереди";
    }

    private void DetailsClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: OrderDisplay order })
        {
            var dialog = new Dialogs.OrderDetailsDialog(order);
            dialog.Owner = Window.GetWindow(this);
            dialog.ShowDialog();
        }
    }

    private async void ActionClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: OrderDisplay order })
        {
            switch (order.Status)
            {
                case "pending":
                    await _orderService.UpdateStatusAsync(order.Id, "in_progress", SessionManager.CurrentUser?.Id);
                    break;
                case "in_progress":
                    await _orderService.UpdateStatusAsync(order.Id, "ready", SessionManager.CurrentUser?.Id);
                    break;
                case "ready":
                    await _orderService.CompleteOrderAsync(order.Id, SessionManager.CurrentUser!.Id);
                    break;
            }
            await LoadOrders();
        }
    }
}
