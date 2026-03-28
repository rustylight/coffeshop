using System.Windows;

namespace CoffeeShop.Views.Dialogs;

public partial class EditProfileDialog : Window
{
    public string UserName { get; private set; } = "";
    public string UserEmail { get; private set; } = "";

    public EditProfileDialog(string currentName, string? currentEmail)
    {
        InitializeComponent();
        NameBox.Text = currentName;
        EmailBox.Text = currentEmail ?? "";
        UserName = currentName;
        UserEmail = currentEmail ?? "";
    }

    private void SaveClick(object sender, RoutedEventArgs e)
    {
        var name = NameBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Введите ФИО", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        UserName = name;
        UserEmail = EmailBox.Text.Trim();
        DialogResult = true;
    }

    private void CancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
