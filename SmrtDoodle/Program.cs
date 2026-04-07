using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace SmrtDoodle;

internal static class Program
{
    [global::System.STAThreadAttribute]
    private static void Main(string[] args)
    {
        global::WinRT.ComWrappersSupport.InitializeComWrappers();

        Application.Start(p =>
        {
            var context = new DispatcherQueueSynchronizationContext(
                DispatcherQueue.GetForCurrentThread());
            global::System.Threading.SynchronizationContext
                .SetSynchronizationContext(context);
            new App();
        });
    }
}
