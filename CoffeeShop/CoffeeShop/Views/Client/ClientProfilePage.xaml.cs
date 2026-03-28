using System.Windows;
using System.Windows.Controls;
using CoffeeShop.Data;
using CoffeeShop.Helpers;
using CoffeeShop.Services;
using CoffeeShop.Views.Dialogs;

namespace CoffeeShop.Views.Client;

public partial class ClientProfilePage : Page
{
    private readonly LoyaltyService _loyaltyService = new();

    public ClientProfilePage()
    {
        InitializeComponent();
        Loaded += async (_, _) => await LoadProfile();
    }

    private async Task LoadProfile()
    {
        var user = SessionManager.CurrentUser;
        if (user == null) return;

        AvatarText.Text = SessionManager.GetInitials();
        NameText.Text = user.FullName;
        if (user.Phone?.Length >= 12)
            PhoneText.Text = $"+7 {user.Phone[2..5]} {user.Phone[5..8]}-{user.Phone[8..10]}-{user.Phone[10..]}";
        else
            PhoneText.Text = user.Phone ?? "—";
        EmailText.Text = user.Email ?? "—";
        BonusText.Text = $"{user.BonusBalance:N0} бонусов";
        RegDateText.Text = user.CreatedAt.ToString("dd MMMM yyyy");
    }

    private void EditProfileClick(object sender, RoutedEventArgs e)
    {
        var user = SessionManager.CurrentUser;
        if (user == null) return;

        var dialog = new EditProfileDialog(user.FullName, user.Email);
        dialog.Owner = Window.GetWindow(this);
        if (dialog.ShowDialog() == true)
        {
            try
            {
                using var db = new AppDbContext();
                var dbUser = db.Users.Find(user.Id);
                if (dbUser != null)
                {
                    dbUser.FullName = dialog.UserName;
                    dbUser.Email = string.IsNullOrWhiteSpace(dialog.UserEmail) ? null : dialog.UserEmail;
                    db.SaveChanges();

                    user.FullName = dialog.UserName;
                    user.Email = dbUser.Email;

                    // Обновить профиль
                    NameText.Text = dialog.UserName;
                    EmailText.Text = user.Email ?? "—";
                    AvatarText.Text = SessionManager.GetInitials();

                    // Обновить sidebar
                    if (Window.GetWindow(this) is ClientShell shell)
                    {
                        shell.UpdateUserInfo();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void LogoutClick(object sender, RoutedEventArgs e)
    {
        SessionManager.Clear();
        var authWindow = new AuthWindow();
        authWindow.Show();
        Window.GetWindow(this)?.Close();
    }
}
