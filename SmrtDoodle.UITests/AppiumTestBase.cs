using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;
using System.Diagnostics;

namespace SmrtDoodle.UITests;

/// <summary>
/// Base class for all Appium UI tests. Manages the WindowsDriver session
/// targeting a local or remote WinAppDriver instance.
/// Defaults to local WinAppDriver at 127.0.0.1:4723.
/// Override via test run settings: AppiumHost, AppiumPort, AppPath.
/// </summary>
public abstract class AppiumTestBase
{
    private const string DefaultAppiumHost = "127.0.0.1";
    private const int DefaultAppiumPort = 4723;

    protected static WindowsDriver<WindowsElement>? Driver { get; private set; }
    protected static TestContext? TestCtx { get; private set; }
    private static Process? _winAppDriverProcess;
    private static Process? _appProcess;

    private static string GetDefaultAppPath()
    {
        // Walk up from test assembly to find the repo root
        var dir = Path.GetDirectoryName(typeof(AppiumTestBase).Assembly.Location)!;
        while (dir != null)
        {
            var candidate = Path.Combine(dir, @"SmrtDoodle\bin\x64\Debug\net8.0-windows10.0.19041.0\SmrtDoodle.exe");
            if (File.Exists(candidate))
                return candidate;
            if (File.Exists(Path.Combine(dir, "SmrtDoodle.slnx")))
                return candidate;
            dir = Path.GetDirectoryName(dir);
        }
        return @"B:\Source\repos\SmrtDoodle\SmrtDoodle\bin\x64\Debug\net8.0-windows10.0.19041.0\SmrtDoodle.exe";
    }

    /// <summary>
    /// Ensures WinAppDriver is running locally. Starts it if not already running.
    /// </summary>
    private static void EnsureWinAppDriverRunning()
    {
        var existing = Process.GetProcessesByName("WinAppDriver");
        if (existing.Length > 0) return;

        var wadPath = @"C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe";
        if (!File.Exists(wadPath))
        {
            Assert.Inconclusive(
                "WinAppDriver is not installed. Install from: https://github.com/microsoft/WinAppDriver/releases");
            return;
        }

        _winAppDriverProcess = Process.Start(new ProcessStartInfo(wadPath)
        {
            UseShellExecute = true
        });

        // Give WinAppDriver time to start listening
        Thread.Sleep(2000);
    }

    /// <summary>
    /// Initializes the Appium session once for the entire test class.
    /// Launches the app manually, then attaches WinAppDriver to the window handle.
    /// </summary>
    protected static void InitializeSession(TestContext context)
    {
        TestCtx = context;

        if (Driver is not null)
        {
            TeardownSession();
        }

        var appiumHost = context.Properties.Contains("AppiumHost")
            ? (string)context.Properties["AppiumHost"]!
            : DefaultAppiumHost;

        var appiumPort = context.Properties.Contains("AppiumPort")
            ? int.Parse((string)context.Properties["AppiumPort"]!)
            : DefaultAppiumPort;

        var configuredAppPath = context.Properties.Contains("AppPath")
            ? (string)context.Properties["AppPath"]!
            : GetDefaultAppPath();

        // Start WinAppDriver locally if targeting localhost
        if (appiumHost is "127.0.0.1" or "localhost")
        {
            EnsureWinAppDriverRunning();
        }

        // Kill any existing SmrtDoodle instances to start fresh
        foreach (var proc in Process.GetProcessesByName("SmrtDoodle"))
        {
            try { proc.Kill(); proc.WaitForExit(3000); } catch { }
        }

        var appiumUrl = new Uri($"http://{appiumHost}:{appiumPort}");

        var appCandidates = new[]
        {
            configuredAppPath,
            GetDefaultAppPath(),
            @"C:\SmrtDoodle-Test\SmrtDoodle.exe",
            @"C:\SmrtDoodle\SmrtDoodle.exe",
        }.Distinct(StringComparer.OrdinalIgnoreCase).Where(File.Exists).ToList();

        if (appCandidates.Count == 0)
        {
            throw new FileNotFoundException(
                "SmrtDoodle.exe not found at any expected path. Build the project first.");
        }

        // Launch the app manually
        var exePath = appCandidates[0];
        _appProcess = Process.Start(new ProcessStartInfo(exePath) { UseShellExecute = false });
        if (_appProcess is null || _appProcess.HasExited)
            throw new InvalidOperationException($"Failed to start SmrtDoodle from {exePath}");

        // Wait for the main window to appear
        var deadline = DateTime.UtcNow.AddSeconds(30);
        IntPtr hwnd = IntPtr.Zero;
        while (DateTime.UtcNow < deadline)
        {
            _appProcess.Refresh();
            if (_appProcess.MainWindowHandle != IntPtr.Zero)
            {
                hwnd = _appProcess.MainWindowHandle;
                break;
            }
            Thread.Sleep(500);
        }

        if (hwnd == IntPtr.Zero)
            throw new TimeoutException("SmrtDoodle window did not appear within 30 seconds.");

        // Attach WinAppDriver to the existing window handle via JSONWP capabilities
        var hwndHex = "0x" + hwnd.ToString("X");
        var options = new AppiumOptions();
        options.AddAdditionalCapability("appTopLevelWindow", hwndHex);
        options.AddAdditionalCapability("deviceName", "WindowsPC");
        options.AddAdditionalCapability("ms:experimental-webdriver", true);

        Driver = new WindowsDriver<WindowsElement>(appiumUrl, options, TimeSpan.FromSeconds(30));
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

        // Maximize
        try { Driver.Manage().Window.Maximize(); } catch (WebDriverException) { }

        // Brief pause for app initialization
        Thread.Sleep(2000);
    }

    /// <summary>
    /// Tears down the Appium session after all tests in the class.
    /// </summary>
    protected static void TeardownSession()
    {
        if (Driver is not null)
        {
            try { Driver.Quit(); } catch { }
            Driver = null;
        }

        // Kill the app process
        if (_appProcess is not null && !_appProcess.HasExited)
        {
            try { _appProcess.Kill(); _appProcess.WaitForExit(3000); } catch { }
        }
        _appProcess = null;

        // Kill any remaining SmrtDoodle instances
        foreach (var proc in Process.GetProcessesByName("SmrtDoodle"))
        {
            try { proc.Kill(); } catch { }
        }

        // Stop WinAppDriver if we started it
        if (_winAppDriverProcess is not null && !_winAppDriverProcess.HasExited)
        {
            try { _winAppDriverProcess.Kill(); } catch { }
            _winAppDriverProcess = null;
        }
    }

    /// <summary>
    /// Ensures clean state before each test: dismiss open menus/dialogs.
    /// </summary>
    [TestInitialize]
    public void EnsureCleanState()
    {
        if (Driver is null) return;

        // Switch back to the main window if a dialog shifted focus
        try
        {
            var handles = Driver.WindowHandles;
            if (handles.Count > 0)
            {
                Driver.SwitchTo().Window(handles[0]);
            }
        }
        catch { }

        // Press Escape a few times to dismiss any open flyout/menu/dialog
        for (var i = 0; i < 5; i++)
        {
            try
            {
                Driver.FindElementByXPath("/*").SendKeys(Keys.Escape);
            }
            catch { }
            Thread.Sleep(200);
        }
    }

    #region Element Finding Helpers

    /// <summary>
    /// Finds an element by its AutomationId (x:Name in XAML).
    /// </summary>
    protected static WindowsElement FindByAutomationId(string automationId)
    {
        try
        {
            return Driver!.FindElementByAccessibilityId(automationId);
        }
        catch (NoSuchElementException) when (automationId == "DrawingCanvas")
        {
            var fallback = TryFindByAutomationId("CanvasContainer") ?? TryFindByAutomationId("CanvasScrollViewer");
            if (fallback is not null)
            {
                return fallback;
            }

            throw;
        }
    }

    /// <summary>
    /// Tries to find an element by its AutomationId, returns null if not found.
    /// </summary>
    protected static WindowsElement? TryFindByAutomationId(string automationId)
    {
        try
        {
            return Driver!.FindElementByAccessibilityId(automationId);
        }
        catch (Exception ex) when (ex is NoSuchElementException or WebDriverException)
        {
            if (automationId == "DrawingCanvas")
            {
                try
                {
                    return Driver!.FindElementByAccessibilityId("CanvasContainer");
                }
                catch (Exception ex2) when (ex2 is NoSuchElementException or WebDriverException)
                {
                    try
                    {
                        return Driver!.FindElementByAccessibilityId("CanvasScrollViewer");
                    }
                    catch (Exception ex3) when (ex3 is NoSuchElementException or WebDriverException)
                    {
                    }
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Finds an element by its display name/text.
    /// </summary>
    protected static WindowsElement FindByName(string name)
    {
        return Driver!.FindElementByName(name);
    }

    /// <summary>
    /// Tries to find an element by its display name/text, returns null if not found.
    /// </summary>
    protected static WindowsElement? TryFindByName(string name)
    {
        try
        {
            return Driver!.FindElementByName(name);
        }
        catch (Exception ex) when (ex is NoSuchElementException or WebDriverException)
        {
            return null;
        }
    }

    /// <summary>
    /// Finds an element using XPath.
    /// </summary>
    protected static WindowsElement FindByXPath(string xpath)
    {
        return (WindowsElement)Driver!.FindElementByXPath(xpath);
    }

    /// <summary>
    /// Finds multiple elements by class name.
    /// </summary>
    protected static IReadOnlyCollection<WindowsElement> FindAllByClassName(string className)
    {
        return Driver!.FindElementsByClassName(className);
    }

    /// <summary>
    /// Finds multiple elements by name.
    /// </summary>
    protected static IReadOnlyCollection<WindowsElement> FindAllByName(string name)
    {
        return Driver!.FindElementsByName(name);
    }

    #endregion

    #region Interaction Helpers

    /// <summary>
    /// Clicks a menu bar item (File, Edit, etc.) and then clicks a menu flyout item.
    /// </summary>
    protected static void ClickMenuItem(string menuTitle, string itemText)
    {
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            var menu = FindByName(menuTitle);
            menu.Click();
            Thread.Sleep(500);

            var item = TryFindByName(itemText);
            if (item is not null)
            {
                item.Click();
                Thread.Sleep(300);
                return;
            }

            // Menu flyout may not have opened; dismiss and retry
            Driver!.FindElementByXPath("/*").SendKeys(Keys.Escape);
            Thread.Sleep(300);
        }

        // Final attempt — let it throw
        FindByName(menuTitle).Click();
        Thread.Sleep(500);
        FindByName(itemText).Click();
        Thread.Sleep(300);
    }

    /// <summary>
    /// Clicks View > 100% using XPath to avoid matching the LayerOpacityText TextBlock.
    /// </summary>
    protected static void ClickViewMenu100Percent()
    {
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            FindByName("View").Click();
            Thread.Sleep(500);

            try
            {
                var menuItem = Driver!.FindElementByXPath("//MenuItem[@Name='100%']");
                menuItem.Click();
                Thread.Sleep(300);
                return;
            }
            catch (WebDriverException) when (attempt < 3)
            {
                Driver!.FindElementByXPath("/*").SendKeys(Keys.Escape);
                Thread.Sleep(300);
            }
        }

        // Final attempt — let it throw
        FindByName("View").Click();
        Thread.Sleep(500);
        Driver!.FindElementByXPath("//MenuItem[@Name='100%']").Click();
        Thread.Sleep(300);
    }

    /// <summary>
    /// Opens a menu and verifies an item exists without clicking it, then dismisses the menu.
    /// </summary>
    protected static WindowsElement OpenMenuAndFindItem(string menuTitle, string itemText)
    {
        var menu = FindByName(menuTitle);
        menu.Click();
        Thread.Sleep(300);

        var item = FindByName(itemText);
        Assert.IsNotNull(item, $"Menu item '{itemText}' not found under '{menuTitle}'");
        return item;
    }

    /// <summary>
    /// Dismisses any open menu by pressing Escape.
    /// </summary>
    protected static void DismissMenu()
    {
        Driver!.FindElementByXPath("/*").SendKeys(Keys.Escape);
        Thread.Sleep(200);
    }

    /// <summary>
    /// Clicks a tool toggle button by its AutomationId (e.g., "BtnPencil").
    /// </summary>
    protected static void SelectTool(string automationId)
    {
        var btn = FindByAutomationId(automationId);
        btn.Click();
        Thread.Sleep(200);
    }

    /// <summary>
    /// Sends a keyboard shortcut (e.g., Ctrl+N, Ctrl+Shift+S).
    /// </summary>
    protected static void SendShortcut(string keys)
    {
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                Driver!.FindElementByXPath("/*").SendKeys(keys);
                Thread.Sleep(300);
                return;
            }
            catch (StaleElementReferenceException) when (attempt < 3)
            {
                Thread.Sleep(150);
            }
        }

        Thread.Sleep(300);
    }

    /// <summary>
    /// Right-clicks an element for context menu.
    /// </summary>
    protected static void RightClick(WindowsElement element)
    {
        var actions = new Actions(Driver!);
        actions.ContextClick(element).Perform();
        Thread.Sleep(300);
    }

    /// <summary>
    /// Drags from one point to another on an element.
    /// </summary>
    protected static void DragOnElement(WindowsElement element, int startX, int startY, int endX, int endY)
    {
        try
        {
            var actions = new Actions(Driver!);
            actions.MoveToElement(element, startX, startY)
                   .ClickAndHold()
                   .MoveByOffset(endX - startX, endY - startY)
                   .Release()
                   .Perform();
        }
        catch (WebDriverException ex) when (ex.Message.Contains("pen and touch pointer", StringComparison.OrdinalIgnoreCase))
        {
            var touch = new PointerInputDevice(PointerKind.Touch);
            var sequence = new ActionSequence(touch, 0);

            sequence.AddAction(touch.CreatePointerMove(element, startX, startY, TimeSpan.Zero));
            sequence.AddAction(touch.CreatePointerDown(MouseButton.Left));
            sequence.AddAction(touch.CreatePointerMove(element, endX, endY, TimeSpan.FromMilliseconds(250)));
            sequence.AddAction(touch.CreatePointerUp(MouseButton.Left));

            Driver!.PerformActions(new List<ActionSequence> { sequence });
        }

        Thread.Sleep(200);
    }

    /// <summary>
    /// Waits for a dialog to appear and returns it.
    /// </summary>
    protected static WindowsElement? WaitForDialog(int timeoutMs = 3000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var dialog = Driver!.FindElementByXPath("//Window[@IsDialog='True']");
                if (dialog is not null)
                    return dialog;
            }
            catch (NoSuchElementException)
            {
            }

            Thread.Sleep(100);
        }

        return null;
    }

    /// <summary>
    /// Dismisses any open dialog by pressing Escape. Handles native file dialogs too.
    /// </summary>
    protected static void DismissDialog()
    {
        // Try multiple escape approaches for native dialogs
        for (var i = 0; i < 3; i++)
        {
            try
            {
                // Try to find Cancel button in native dialog
                var cancel = Driver!.FindElementByName("Cancel");
                if (cancel != null)
                {
                    cancel.Click();
                    Thread.Sleep(500);
                    return;
                }
            }
            catch { }

            try
            {
                Driver!.FindElementByXPath("/*").SendKeys(Keys.Escape);
            }
            catch { }
            Thread.Sleep(300);
        }
    }

    /// <summary>
    /// Gets the toggle state of a ToggleButton (true = checked/pressed).
    /// </summary>
    protected static bool IsToggled(WindowsElement element)
    {
        var toggleState = element.GetAttribute("Toggle.ToggleState");
        return toggleState == "1" || toggleState == "On";
    }

    /// <summary>
    /// Verifies an element is displayed (visible) on screen.
    /// </summary>
    protected static void AssertElementDisplayed(string automationId, string description)
    {
        var element = FindByAutomationId(automationId);
        Assert.IsTrue(element.Displayed, $"{description} (AutomationId='{automationId}') should be displayed");
    }

    /// <summary>
    /// Verifies the text content of a status bar element.
    /// </summary>
    protected static void AssertStatusText(string automationId, string expectedText, string description)
    {
        var element = FindByAutomationId(automationId);
        Assert.AreEqual(expectedText, element.Text, $"{description} status text mismatch");
    }

    /// <summary>
    /// Creates a fresh canvas by invoking File > New and dismissing any save prompt.
    /// </summary>
    protected static void ResetCanvas()
    {
        SendShortcut(Keys.Control + "n");
        Thread.Sleep(500);

        // Temporarily reduce implicit wait so we don't wait 5s for a dialog that may not appear
        Driver!.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(500);
        try
        {
            // If there's a "Save changes?" dialog, click "Don't Save"
            var dontSave = TryFindByName("Don't Save");
            if (dontSave is not null)
            {
                dontSave.Click();
                Thread.Sleep(500);
            }
        }
        finally
        {
            Driver!.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        }
    }

    #endregion
}
