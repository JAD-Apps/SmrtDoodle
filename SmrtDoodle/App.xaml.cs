using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
#if SMRTPAD_BRIDGE
using SmrtAI.Core.Ipc;
#endif
using SmrtDoodle.Services;
using System;
using System.Collections.Specialized;
using System.Web;
using Windows.ApplicationModel.Activation;

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
            // In WinUI 3 / Windows App SDK the Microsoft.UI.Xaml LaunchActivatedEventArgs
            // unconditionally reports Launch even for protocol activations, so we have to
            // pull the real activation kind from AppInstance.GetActivatedEventArgs().
            var activation = AppInstance.GetCurrent().GetActivatedEventArgs();
            if (activation?.Kind == ExtendedActivationKind.Protocol &&
                activation.Data is IProtocolActivatedEventArgs protocolArgs)
            {
#if SMRTPAD_BRIDGE
                TryStartSmrtPadBridge(protocolArgs.Uri);
#endif
            }

            _window = new MainWindow();
            _window.Activate();
            LoggingService.Instance.Info("Application launched.");
        }

#if SMRTPAD_BRIDGE
        /// <summary>
        /// Honours the <c>smrtdoodle://edit?pipe=...&amp;v=...</c> launch URI from SmrtPad by
        /// connecting to the named-pipe server and seeding <see cref="SmrtPadBridgeSession"/>.
        /// Falls back to a normal launch when the URI does not carry a recognised pipe name.
        /// </summary>
        private static void TryStartSmrtPadBridge(Uri uri)
        {
            if (!string.Equals(uri.Scheme, SmrtDoodleIpc.ProtocolScheme, StringComparison.OrdinalIgnoreCase))
                return;

            NameValueCollection query = HttpUtility.ParseQueryString(uri.Query);
            var pipeName = query[SmrtDoodleIpc.PipeQueryKey];
            if (string.IsNullOrWhiteSpace(pipeName)) return;

            if (!int.TryParse(query[SmrtDoodleIpc.SchemaQueryKey], out var schema) ||
                schema != SmrtDoodleIpc.CurrentSchemaVersion)
            {
                LoggingService.Instance.Info($"Ignoring SmrtPad bridge launch with unsupported schema '{query[SmrtDoodleIpc.SchemaQueryKey]}'.");
                return;
            }

            SmrtPadBridgeSession.Start(pipeName);
        }
#endif

        private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            LoggingService.Instance.Fatal("Unhandled exception", e.Exception);
            e.Handled = true; // Prevent crash — let user save work
        }
    }
}
