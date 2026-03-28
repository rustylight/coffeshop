using System.Windows;

namespace CoffeeShop.Views.Dialogs;

public partial class AddBaristaDialog : Window
{
    public string BaristaName { get; private set; } = "";
    public string BaristaPhone { get; private set; } = "";
    public string BaristaPassword { get; private set; } = "";

    public AddBaristaDialog()
    {
        InitializeComponent();
    }

    private void SaveClick(object sender, RoutedEventArgs e)
    {
        BaristaName = NameBox.Text.Trim();
        BaristaPhone = PhoneBox.Text.Trim();
        BaristaPassword = PasswordBox.Password;

        if (string.IsNullOrWhiteSpace(BaristaName) ||
            string.IsNullOrWhiteSpace(BaristaPhone) ||
            string.IsNullOrWhiteSpace(BaristaPassword))
        {
            MessageBox.Show("Заполните все поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        DialogResult = true;
    }

    private void CancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
