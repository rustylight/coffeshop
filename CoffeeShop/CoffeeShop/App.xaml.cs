using System.Windows;
using System.Windows.Threading;

namespace CoffeeShop;

public partial class App : Application
{
    static App()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += OnUnhandledException;
    }

    private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show($"Произошла ошибка: {e.Exception.Message}", "Ошибка",
            MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }
}
