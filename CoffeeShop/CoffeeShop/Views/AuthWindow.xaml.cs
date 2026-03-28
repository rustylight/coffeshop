using System.Windows;
using CoffeeShop.Helpers;
using CoffeeShop.Services;
using CoffeeShop.Views.Client;
using CoffeeShop.Views.Barista;
using CoffeeShop.Views.Admin;

namespace CoffeeShop.Views;

public partial class AuthWindow : Window
{
    private readonly AuthService _authService = new();
    private readonly ShiftService _shiftService = new();

    public AuthWindow()
    {
        InitializeComponent();
    }

    private async void LoginClick(object sender, RoutedEventArgs e)
    {
        ErrorText.Visibility = Visibility.Collapsed;

        var phone = PhoneBox.Text.Trim();
        var password = PasswordBox.Password;

        if (string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(password))
        {
            ShowError("Заполните все поля");
            return;
        }

        try
        {
            var user = await _authService.LoginAsync(phone, password);
            if (user == null)
            {
                ShowError("Неверный телефон или пароль");
                return;
            }

            SessionManager.CurrentUser = user;
            var shift = await _shiftService.GetCurrentShiftAsync();
            SessionManager.CurrentShift = shift;

            Window nextWindow = user.Role.Name switch
            {
                "Клиент" => new ClientShell(),
                "Бариста" => new BaristaShell(),
                "Администратор" => new AdminShell(),
                _ => throw new InvalidOperationException("Неизвестная роль")
            };

            nextWindow.Show();
            Close();
        }
        catch (Exception ex)
        {
            ShowError($"Ошибка подключения: {ex.Message}");
        }
    }

    private void RegisterClick(object sender, RoutedEventArgs e)
    {
        new RegisterWindow().Show();
        Close();
    }

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorText.Visibility = Visibility.Visible;
    }
}
