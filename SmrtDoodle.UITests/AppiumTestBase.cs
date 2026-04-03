using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;

namespace SmrtDoodle.UITests;

/// <summary>
/// Base class for all Appium UI tests. Manages the WindowsDriver session
/// targeting a remote WinAppDriver instance at 192.168.0.100:4723.
/// </summary>
public abstract class AppiumTestBase
{
    private const string DefaultAppiumHost = "192.168.0.100";
    private const int DefaultAppiumPort = 4723;
    private const string DefaultAppPath = @"C:\SmrtDoodle-Test\SmrtDoodle.exe";

    protected static WindowsDriver? Driver { get; private set; }
    protected static TestContext? TestCtx { get; private set; }

    /// <summary>
    /// Initializes the Appium session once for the entire test class.
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
            : DefaultAppPath;

        var appiumUrl = new Uri($"http://{appiumHost}:{appiumPort}");

        var appCandidates = new[]
        {
            configuredAppPath,
            @"C:\SmrtDoodle\SmrtDoodle.exe",
            @"C:\SmrtDoodle-Test\publish\x64\SmrtDoodle.exe",
            @"C:\SmrtDoodle\publish\x64\SmrtDoodle.exe"
        }.Distinct(StringComparer.OrdinalIgnoreCase);

        Exception? lastException = null;
        foreach (var appPath in appCandidates)
        {
            for (var attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    var options = new AppiumOptions
                    {
                        App = appPath,
                        AutomationName = "Windows",
                        DeviceName = "WindowsPC",
                        PlatformName = "Windows"
                    };
                    options.AddAdditionalAppiumOption("ms:waitForAppLaunch", 25);
                    options.AddAdditionalAppiumOption("ms:experimental-webdriver", true);

                    Driver = new WindowsDriver(appiumUrl, options, TimeSpan.FromSeconds(60));
                    break;
                }
                catch (WebDriverException ex) when (ex.Message.Contains("cannot find the file specified", StringComparison.OrdinalIgnoreCase))
                {
                    lastException = ex;
                    break;
                }
                catch (WebDriverException ex)
                {
                    lastException = ex;
                    Thread.Sleep(1000);
                }
            }

            if (Driver is not null)
            {
                break;
            }
        }

        if (Driver is null)
        {
            throw new InvalidOperationException("Could not launch SmrtDoodle on remote host from any known path.", lastException);
        }
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

        // Maximize can be unsupported in some remote contexts; continue if unavailable.
        try
        {
            Driver.Manage().Window.Maximize();
        }
        catch (WebDriverException)
        {
        }

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
            Driver.Quit();
            Driver = null;
        }
    }

    #region Element Finding Helpers

    /// <summary>
    /// Finds an element by its AutomationId (x:Name in XAML).
    /// </summary>
    protected static AppiumElement FindByAutomationId(string automationId)
    {
        try
        {
            return (AppiumElement)Driver!.FindElement(MobileBy.AccessibilityId(automationId));
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
    protected static AppiumElement? TryFindByAutomationId(string automationId)
    {
        try
        {
            return (AppiumElement)Driver!.FindElement(MobileBy.AccessibilityId(automationId));
        }
        catch (NoSuchElementException)
        {
            if (automationId == "DrawingCanvas")
            {
                try
                {
                    return (AppiumElement)Driver!.FindElement(MobileBy.AccessibilityId("CanvasContainer"));
                }
                catch (NoSuchElementException)
                {
                    try
                    {
                        return (AppiumElement)Driver!.FindElement(MobileBy.AccessibilityId("CanvasScrollViewer"));
                    }
                    catch (NoSuchElementException)
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
    protected static AppiumElement FindByName(string name)
    {
        return (AppiumElement)Driver!.FindElement(MobileBy.Name(name));
    }

    /// <summary>
    /// Tries to find an element by its display name/text, returns null if not found.
    /// </summary>
    protected static AppiumElement? TryFindByName(string name)
    {
        try
        {
            return (AppiumElement)Driver!.FindElement(MobileBy.Name(name));
        }
        catch (NoSuchElementException)
        {
            return null;
        }
    }

    /// <summary>
    /// Finds an element using XPath.
    /// </summary>
    protected static AppiumElement FindByXPath(string xpath)
    {
        return (AppiumElement)Driver!.FindElement(By.XPath(xpath));
    }

    /// <summary>
    /// Finds multiple elements by class name.
    /// </summary>
    protected static IReadOnlyCollection<AppiumElement> FindAllByClassName(string className)
    {
        return Driver!.FindElements(By.ClassName(className));
    }

    /// <summary>
    /// Finds multiple elements by name.
    /// </summary>
    protected static IReadOnlyCollection<AppiumElement> FindAllByName(string name)
    {
        return Driver!.FindElements(MobileBy.Name(name));
    }

    #endregion

    #region Interaction Helpers

    /// <summary>
    /// Clicks a menu bar item (File, Edit, etc.) and then clicks a menu flyout item.
    /// </summary>
    protected static void ClickMenuItem(string menuTitle, string itemText)
    {
        var menu = FindByName(menuTitle);
        menu.Click();
        Thread.Sleep(300);

        var item = FindByName(itemText);
        item.Click();
        Thread.Sleep(300);
    }

    /// <summary>
    /// Opens a menu and verifies an item exists without clicking it, then dismisses the menu.
    /// </summary>
    protected static AppiumElement OpenMenuAndFindItem(string menuTitle, string itemText)
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
        Driver!.FindElement(By.XPath("/*")).SendKeys(Keys.Escape);
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
                Driver!.FindElement(By.XPath("/*")).SendKeys(keys);
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
    protected static void RightClick(AppiumElement element)
    {
        var actions = new Actions(Driver!);
        actions.ContextClick(element).Perform();
        Thread.Sleep(300);
    }

    /// <summary>
    /// Drags from one point to another on an element.
    /// </summary>
    protected static void DragOnElement(AppiumElement element, int startX, int startY, int endX, int endY)
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
    /// Waits for a dialog to appear and returns it. Useful for Save/Open/Resize dialogs.
    /// </summary>
    protected static AppiumElement? WaitForDialog(int timeoutMs = 3000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var dialog = Driver!.FindElement(By.XPath("//Window[@IsDialog='True']"));
                if (dialog is AppiumElement ae)
                    return ae;
            }
            catch (NoSuchElementException)
            {
                // Dialog not yet open
            }

            Thread.Sleep(100);
        }

        return null;
    }

    /// <summary>
    /// Dismisses any open dialog by pressing Escape.
    /// </summary>
    protected static void DismissDialog()
    {
        SendShortcut(Keys.Escape);
        Thread.Sleep(300);
    }

    /// <summary>
    /// Gets the toggle state of a ToggleButton (true = checked/pressed).
    /// </summary>
    protected static bool IsToggled(AppiumElement element)
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

        // If there's a "Save changes?" dialog, click "Don't Save"
        var dontSave = TryFindByName("Don't Save");
        if (dontSave is not null)
        {
            dontSave.Click();
            Thread.Sleep(500);
        }
    }

    #endregion
}
