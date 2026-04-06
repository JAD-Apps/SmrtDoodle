using Microsoft.UI.Xaml;
using SmrtDoodle.Services;
using System;

namespace SmrtDoodle
{
    public partial class App : Application
    {
        private Window? _window;

        public App()
        {
            InitializeComponent();
            UnhandledException += OnUnhandledException;
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();
            _window.Activate();
            LoggingService.Instance.Info("Application launched.");
        }

        private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            LoggingService.Instance.Fatal("Unhandled exception", e.Exception);
            e.Handled = true; // Prevent crash — let user save work
        }
    }
}
