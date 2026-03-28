using System.Windows;
using CoffeeShop.Views.Barista;

namespace CoffeeShop.Views.Dialogs;

public class OrderItemDisplay
{
    public string NameText { get; set; } = "";
    public string PriceText { get; set; } = "";
}

public partial class OrderDetailsDialog : Window
{
    public OrderDetailsDialog(OrderDisplay order)
    {
        InitializeComponent();

        TitleText.Text = $"Детали заказа #{order.Id}";
        ClientText.Text = order.ClientName;
        TimeText.Text = order.TimeText;
        StatusText.Text = order.StatusText;
        StatusText.Foreground = order.StatusFg;
        StatusBorder.Background = order.StatusBg;

        var items = order.ItemDetails.Select(d => new OrderItemDisplay
        {
            NameText = d.Quantity > 1 ? $"{d.Name} × {d.Quantity}" : d.Name,
            PriceText = $"{d.Subtotal:N0} ₽"
        }).ToList();
        ItemsList.ItemsSource = items;

        TotalText.Text = $"{order.FinalAmount:N0} ₽";
    }

    private void CloseClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
