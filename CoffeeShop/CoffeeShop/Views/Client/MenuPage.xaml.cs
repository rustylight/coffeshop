using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CoffeeShop.Helpers;
using CoffeeShop.Models;
using CoffeeShop.Services;

namespace CoffeeShop.Views.Client;

public class CartItem
{
    public int MenuItemId { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal Subtotal => Price * Quantity;
}

public partial class MenuPage : Page
{
    private readonly MenuService _menuService = new();
    private readonly OrderService _orderService = new();
    private readonly ClientShell _shell;
    private List<Models.MenuItem> _allItems = new();
    private List<MenuCategory> _categories = new();
    private readonly ObservableCollection<CartItem> _cart = new();
    private bool _bonusApplied;

    private static readonly Dictionary<string, string> CategoryIcons = new()
    {
        ["Кофе"] = "☕",
        ["Чай"] = "🍵",
        ["Десерты"] = "🍰",
        ["Снэки"] = "🥪"
    };

    private static readonly Dictionary<string, string> CategoryDescriptions = new()
    {
        ["Кофе"] = "Бодрящие зёрна, приготовленные с любовью",
        ["Чай"] = "Ароматные чаи со всего мира",
        ["Десерты"] = "Сладкие угощения к вашему напитку",
        ["Снэки"] = "Лёгкие перекусы на каждый день"
    };

    public MenuPage(ClientShell shell)
    {
        InitializeComponent();
        _shell = shell;
        CartList.ItemsSource = _cart;
        Loaded += async (_, _) => await LoadMenu();
    }

    private async Task LoadMenu()
    {
        try
        {
            _allItems = await _menuService.GetAvailableItemsAsync();
            _categories = _allItems
                .Where(m => m.Category != null)
                .Select(m => m.Category)
                .DistinctBy(c => c.Id)
                .ToList();
            CategoryList.ItemsSource = _categories;

            if (_categories.Count > 0)
            {
                FilterByCategory(_categories[0].Name);
                Dispatcher.InvokeAsync(UpdateCategoryIcons,
                    System.Windows.Threading.DispatcherPriority.Loaded);
            }
            else
                MenuList.ItemsSource = _allItems;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки меню: {ex.Message}", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void FilterByCategory(string categoryName)
    {
        CategoryTitle.Text = categoryName;
        CategoryDesc.Text = CategoryDescriptions.GetValueOrDefault(categoryName, "");

        var items = _allItems.Where(m => m.Category?.Name == categoryName).ToList();
        MenuList.ItemsSource = items;

        // Update icons on rendered cards
        Dispatcher.InvokeAsync(() => UpdateMenuIcons(categoryName),
            System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private void UpdateMenuIcons(string categoryName)
    {
        var icon = CategoryIcons.GetValueOrDefault(categoryName, "☕");
        var containers = MenuList.Items.Cast<object>()
            .Select((_, i) => MenuList.ItemContainerGenerator.ContainerFromIndex(i))
            .Where(c => c != null);

        foreach (var container in containers)
        {
            var iconBlock = FindVisualChild<TextBlock>(container, "ItemIcon");
            if (iconBlock != null)
                iconBlock.Text = icon;
        }
    }

    private static T? FindVisualChild<T>(System.Windows.DependencyObject parent, string name) where T : FrameworkElement
    {
        for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
            if (child is T fe && fe.Name == name) return fe;
            var result = FindVisualChild<T>(child, name);
            if (result != null) return result;
        }
        return null;
    }

    private void CategoryClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string tag)
        {
            FilterByCategory(tag);

            // Update sidebar icons
            Dispatcher.InvokeAsync(UpdateCategoryIcons,
                System.Windows.Threading.DispatcherPriority.Loaded);
        }
    }

    private void UpdateCategoryIcons()
    {
        for (int i = 0; i < CategoryList.Items.Count; i++)
        {
            var container = CategoryList.ItemContainerGenerator.ContainerFromIndex(i);
            if (container == null) continue;
            var cat = CategoryList.Items[i] as MenuCategory;
            if (cat == null) continue;
            var iconBlock = FindVisualChild<TextBlock>(container, "Icon");
            if (iconBlock != null)
                iconBlock.Text = CategoryIcons.GetValueOrDefault(cat.Name, "☕");
        }
    }

    private void AddToCartClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Models.MenuItem item)
        {
            var existing = _cart.FirstOrDefault(c => c.MenuItemId == item.Id);
            if (existing != null)
            {
                existing.Quantity++;
                RefreshCart();
            }
            else
            {
                _cart.Add(new CartItem
                {
                    MenuItemId = item.Id,
                    Name = item.Name,
                    Price = item.Price
                });
            }
            UpdateTotals();
        }
    }

    private void IncreaseQtyClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is CartItem item)
        {
            item.Quantity++;
            RefreshCart();
            UpdateTotals();
        }
    }

    private void DecreaseQtyClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is CartItem item)
        {
            item.Quantity--;
            if (item.Quantity <= 0) _cart.Remove(item);
            else RefreshCart();
            UpdateTotals();
        }
    }

    private void RefreshCart()
    {
        var items = _cart.ToList();
        _cart.Clear();
        foreach (var i in items) _cart.Add(i);
    }

    private void ApplyBonusClick(object sender, RoutedEventArgs e)
    {
        if (_bonusApplied)
        {
            // Already applied — toggle off
            CancelBonusClick(sender, e);
            return;
        }

        var maxBonus = SessionManager.CurrentUser?.BonusBalance ?? 0;
        if (maxBonus <= 0)
        {
            MessageBox.Show("У вас нет доступных бонусов.", "Бонусы",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        AvailableBonusText.Text = $"Доступно: {maxBonus:N0} бонусов";
        var total = _cart.Sum(c => c.Subtotal);
        var suggested = Math.Min(maxBonus, total);
        BonusInput.Text = suggested > 0 ? $"{suggested:N0}" : "0";
        BonusPanel.Visibility = Visibility.Visible;
        ApplyBonusBtn.Visibility = Visibility.Collapsed;
        _bonusApplied = true;
        UpdateTotals();
    }

    private void CancelBonusClick(object sender, RoutedEventArgs e)
    {
        _bonusApplied = false;
        BonusInput.Text = "0";
        BonusPanel.Visibility = Visibility.Collapsed;
        BonusAppliedPanel.Visibility = Visibility.Collapsed;
        ApplyBonusBtn.Visibility = Visibility.Visible;
        UpdateTotals();
    }

    private void BonusPreviewInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !Regex.IsMatch(e.Text, @"^\d+$");
    }

    private void BonusPasting(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(typeof(string)))
        {
            var text = (string)e.DataObject.GetData(typeof(string))!;
            if (!Regex.IsMatch(text, @"^\d+$"))
                e.CancelCommand();
        }
        else
            e.CancelCommand();
    }

    private void BonusChanged(object sender, TextChangedEventArgs e)
    {
        if (!IsLoaded) return;
        UpdateTotals();
    }

    private void UpdateTotals()
    {
        var total = _cart.Sum(c => c.Subtotal);
        decimal bonus = 0;

        if (_bonusApplied && decimal.TryParse(BonusInput.Text, out var b))
        {
            var maxBonus = SessionManager.CurrentUser?.BonusBalance ?? 0;
            bonus = Math.Min(Math.Min(b, maxBonus), total);
            if (bonus < 0) bonus = 0;
        }

        if (bonus > 0)
        {
            BonusAppliedPanel.Visibility = Visibility.Visible;
            BonusDiscountText.Text = $"-{bonus:N0} ₽";
        }
        else
        {
            BonusAppliedPanel.Visibility = Visibility.Collapsed;
        }

        var final_ = total - bonus;
        FinalText.Text = $"{final_:N0} ₽";
        CartCountText.Text = _cart.Sum(c => c.Quantity).ToString();
        PlaceOrderBtn.IsEnabled = _cart.Count > 0;

        EmptyCartPanel.Visibility = _cart.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        CartScroll.Visibility = _cart.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void PlaceOrderClick(object sender, RoutedEventArgs e)
    {
        if (_cart.Count == 0) return;

        decimal bonus = 0;
        if (_bonusApplied)
        {
            decimal.TryParse(BonusInput.Text, out bonus);
            var maxBonus = SessionManager.CurrentUser?.BonusBalance ?? 0;
            var total = _cart.Sum(c => c.Subtotal);
            bonus = Math.Min(Math.Min(bonus, maxBonus), total);
            if (bonus < 0) bonus = 0;
        }

        var items = _cart.Select(c => (c.MenuItemId, c.Quantity)).ToList();

        try
        {
            PlaceOrderBtn.IsEnabled = false;
            await _orderService.CreateOrderAsync(SessionManager.CurrentUser!.Id, items, bonus);
            await _shell.RefreshBonus();
            _shell.NavigateToOrderStatus();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            PlaceOrderBtn.IsEnabled = true;
        }
    }
}
