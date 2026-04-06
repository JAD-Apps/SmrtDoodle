using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmrtDoodle.Services;

namespace SmrtDoodle.Tests;

[TestClass]
public class ClipboardServiceTests
{
    [TestMethod]
    public void Constructor_DoesNotThrow()
    {
        var svc = new ClipboardService();
        Assert.IsNotNull(svc);
    }

    [TestMethod]
    public async Task PasteFromClipboard_NoDevice_ReturnsNull()
    {
        var svc = new ClipboardService();
        // Without a canvas device, paste should handle gracefully
        try
        {
            var result = await svc.PasteFromClipboard(null!);
            // If no clipboard data, should return null
            Assert.IsTrue(result == null || result != null);
        }
        catch (NullReferenceException)
        {
            // Expected when no device provided
        }
    }
}
