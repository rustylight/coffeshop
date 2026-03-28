using System.Windows;
using CoffeeShop.Models;
using CoffeeShop.Services;

namespace CoffeeShop.Views.Dialogs;

public partial class MenuItemDialog : Window
{
    public string ItemName { get; private set; } = "";
    public string? ItemDescription { get; private set; }
    public decimal ItemPrice { get; private set; }
    public int CategoryId { get; private set; }

    public MenuItemDialog(MenuItem? existing = null)
    {
        InitializeComponent();

        Loaded += async (_, _) =>
        {
            var service = new MenuService();
            var categories = await service.GetCategoriesAsync();
            CategoryCombo.ItemsSource = categories;

            if (existing != null)
            {
                TitleText.Text = "Редактировать позицию";
                NameBox.Text = existing.Name;
                DescBox.Text = existing.Description ?? "";
                PriceBox.Text = existing.Price.ToString("F0");
                CategoryCombo.SelectedValue = existing.CategoryId;
            }
            else if (categories.Count > 0)
            {
                CategoryCombo.SelectedIndex = 0;
            }
        };
    }

    private void SaveClick(object sender, RoutedEventArgs e)
    {
        ItemName = NameBox.Text.Trim();
        ItemDescription = string.IsNullOrWhiteSpace(DescBox.Text) ? null : DescBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(ItemName))
        {
            MessageBox.Show("Введите название", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!decimal.TryParse(PriceBox.Text, out var price) || price <= 0)
        {
            MessageBox.Show("Введите корректную цену", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (CategoryCombo.SelectedValue is not int catId)
        {
            MessageBox.Show("Выберите категорию", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        ItemPrice = price;
        CategoryId = catId;
        DialogResult = true;
    }

    private void CancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
