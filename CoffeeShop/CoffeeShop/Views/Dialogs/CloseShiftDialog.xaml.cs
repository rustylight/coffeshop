using System.Windows;

namespace CoffeeShop.Views.Dialogs;

public partial class CloseShiftDialog : Window
{
    public CloseShiftDialog(decimal revenue, int ordersCount, decimal bonusAccrued)
    {
        InitializeComponent();
        RevenueText.Text = $"{revenue:N0} ₽";
        OrdersText.Text = ordersCount.ToString();
        BonusText.Text = $"{bonusAccrued:N0}";
    }

    private void CancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void ConfirmClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }
}
