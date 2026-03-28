using System.Windows;

namespace CoffeeShop.Views.Dialogs;

public partial class EditBaristaDialog : Window
{
    public string BaristaName { get; private set; } = "";
    public string BaristaPhone { get; private set; } = "";
    public int BaristaId { get; private set; }

    public EditBaristaDialog(int id, string name, string phone)
    {
        InitializeComponent();
        BaristaId = id;
        NameBox.Text = name;
        PhoneBox.Text = phone;
    }

    private void SaveClick(object sender, RoutedEventArgs e)
    {
        BaristaName = NameBox.Text.Trim();
        BaristaPhone = PhoneBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(BaristaName) || string.IsNullOrWhiteSpace(BaristaPhone))
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
