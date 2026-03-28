using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CoffeeShop.Services;
using CoffeeShop.Views.Dialogs;
using MenuItem = CoffeeShop.Models.MenuItem;

namespace CoffeeShop.Views.Admin;

public partial class MenuManagementPage : Page
{
    private readonly MenuService _menuService = new();
    private List<MenuItem> _allItems = new();
    private string _currentFilter = "Все";

    public MenuManagementPage()
    {
        InitializeComponent();
        Loaded += async (_, _) => await LoadMenu();
    }

    private async Task LoadMenu()
    {
        _allItems = await _menuService.GetAllItemsAsync();
        BuildFilterTabs();
        ApplyFilter();
    }

    private void BuildFilterTabs()
    {
        FilterPanel.Children.Clear();
        var categories = new List<string> { "Все" };
        categories.AddRange(_allItems.Select(m => m.Category.Name).Distinct());

        foreach (var cat in categories)
        {
            var btn = new Button { Cursor = System.Windows.Input.Cursors.Hand, Margin = new Thickness(0, 0, 6, 0) };
            btn.Tag = cat;
            btn.Click += FilterClick;
            ApplyFilterStyle(btn, cat == _currentFilter);
            FilterPanel.Children.Add(btn);
        }
    }

    private void ApplyFilterStyle(Button btn, bool active)
    {
        var tag = btn.Tag as string ?? "";
        var template = new ControlTemplate(typeof(Button));
        var border = new FrameworkElementFactory(typeof(Border));
        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(16));
        border.SetValue(Border.PaddingProperty, new Thickness(14, 6, 14, 6));
        border.SetValue(Border.BackgroundProperty, active
            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6F4E37"))
            : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8E0D4")));

        var text = new FrameworkElementFactory(typeof(TextBlock));
        text.SetValue(TextBlock.TextProperty, tag);
        text.SetValue(TextBlock.FontSizeProperty, 12.0);
        text.SetValue(TextBlock.FontWeightProperty, FontWeights.SemiBold);
        text.SetValue(TextBlock.ForegroundProperty, active
            ? Brushes.White
            : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6F4E37")));

        border.AppendChild(text);
        template.VisualTree = border;
        btn.Template = template;
    }

    private void FilterClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string tag)
        {
            _currentFilter = tag;
            foreach (Button child in FilterPanel.Children)
                ApplyFilterStyle(child, (child.Tag as string) == _currentFilter);
            ApplyFilter();
        }
    }

    private void ApplyFilter()
    {
        MenuList.ItemsSource = _currentFilter == "Все"
            ? _allItems
            : _allItems.Where(m => m.Category.Name == _currentFilter).ToList();
    }

    private async void AddItemClick(object sender, RoutedEventArgs e)
    {
        var dialog = new MenuItemDialog();
        dialog.Owner = Window.GetWindow(this);
        if (dialog.ShowDialog() == true)
        {
            await _menuService.AddMenuItemAsync(
                dialog.ItemName, dialog.ItemDescription,
                dialog.ItemPrice, dialog.CategoryId);
            await LoadMenu();
        }
    }

    private async void EditItemClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: MenuItem item })
        {
            var dialog = new MenuItemDialog(item);
            dialog.Owner = Window.GetWindow(this);
            if (dialog.ShowDialog() == true)
            {
                await _menuService.UpdateMenuItemAsync(
                    item.Id, dialog.ItemName, dialog.ItemDescription,
                    dialog.ItemPrice, dialog.CategoryId);
                await LoadMenu();
            }
        }
    }

    private async void DeleteItemClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: MenuItem item })
        {
            var result = MessageBox.Show(
                $"Удалить позицию \"{item.Name}\" из меню?",
                "Удаление",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await _menuService.DeleteMenuItemAsync(item.Id);
                await LoadMenu();
            }
        }
    }
}
