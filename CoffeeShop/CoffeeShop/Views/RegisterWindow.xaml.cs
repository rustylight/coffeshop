using System.Text.RegularExpressions;
using System.Windows;
using CoffeeShop.Helpers;
using CoffeeShop.Services;
using CoffeeShop.Views.Client;

namespace CoffeeShop.Views;

public partial class RegisterWindow : Window
{
    private readonly AuthService _authService = new();

    public RegisterWindow()
    {
        InitializeComponent();
    }

    private async void RegisterClick(object sender, RoutedEventArgs e)
    {
        ErrorText.Visibility = Visibility.Collapsed;

        var name = NameBox.Text.Trim();
        var phone = PhoneBox.Text.Trim();
        var password = PasswordBox.Password;
        var confirm = PasswordConfirmBox.Password;

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone) ||
            string.IsNullOrWhiteSpace(password))
        {
            ShowError("Заполните все поля");
            return;
        }

        if (!Regex.IsMatch(phone, @"^\+7\d{10}$"))
        {
            ShowError("Телефон должен быть в формате +7XXXXXXXXXX");
            return;
        }

        if (password.Length < 6)
        {
            ShowError("Пароль должен содержать не менее 6 символов");
            return;
        }

        if (password != confirm)
        {
            ShowError("Пароли не совпадают");
            return;
        }

        try
        {
            var user = await _authService.RegisterClientAsync(name, phone, password);
            SessionManager.CurrentUser = user;

            var shiftService = new ShiftService();
            SessionManager.CurrentShift = await shiftService.GetCurrentShiftAsync();

            new ClientShell().Show();
            Close();
        }
        catch (InvalidOperationException ex)
        {
            ShowError(ex.Message);
        }
        catch (Exception ex)
        {
            ShowError($"Ошибка: {ex.Message}");
        }
    }

    private void LoginClick(object sender, RoutedEventArgs e)
    {
        new AuthWindow().Show();
        Close();
    }

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorText.Visibility = Visibility.Visible;
    }
}
