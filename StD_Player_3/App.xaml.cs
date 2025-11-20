using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace StD_Player_3
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Перехват исключений в UI-потоке
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;

            // Перехват исключений в других потоках
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // Перехват исключений тасков
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ShowErrorWindow(e.Exception);
            e.Handled = true; // предотвращаем падение
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ShowErrorWindow(e.ExceptionObject as Exception);
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            ShowErrorWindow(e.Exception);
            e.SetObserved();
        }

        private void ShowErrorWindow(Exception ex)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var wnd = new ErrorWindow(ex);
                wnd.ShowDialog();
            });
        }
    }
}
