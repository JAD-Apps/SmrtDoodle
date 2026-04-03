using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.ApplicationModel.DynamicDependency;

namespace SmrtDoodle;

internal static class Program
{
    [global::System.STAThreadAttribute]
    private static void Main(string[] args)
    {
        global::WinRT.ComWrappersSupport.InitializeComWrappers();
        Bootstrap.Initialize(0x00010008);

        try
        {
            Application.Start(p =>
            {
                var context = new DispatcherQueueSynchronizationContext(
                    DispatcherQueue.GetForCurrentThread());
                global::System.Threading.SynchronizationContext
                    .SetSynchronizationContext(context);
                new App();
            });
        }
        finally
        {
            Bootstrap.Shutdown();
        }
    }
}
