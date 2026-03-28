using System.Windows;

namespace CoffeeShop.Views.Dialogs;

public partial class ConfirmDeleteDialog : Window
{
    public ConfirmDeleteDialog(string fullName)
    {
        InitializeComponent();

        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var initials = parts.Length >= 2
            ? $"{parts[0][0]}.{parts[1][0]}."
            : parts.Length > 0 ? $"{parts[0][0]}." : "?";
        AvatarText.Text = initials;

        DescText.Text = $"Вы уверены, что хотите удалить\n{fullName} из системы?\nЭто действие необратимо.";
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
