using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmrtDoodle.Services;

namespace SmrtDoodle.Tests;

[TestClass]
public class AIServiceTests
{
    [TestMethod]
    public void Default_NotProLicensed()
    {
        var svc = new AIService();
        Assert.IsFalse(svc.IsProLicensed);
    }

    [TestMethod]
    public void SetProLicense_True()
    {
        var svc = new AIService();
        svc.SetProLicense(true);
        Assert.IsTrue(svc.IsProLicensed);
    }

    [TestMethod]
    public void SetProLicense_FalseThenTrue()
    {
        var svc = new AIService();
        svc.SetProLicense(false);
        Assert.IsFalse(svc.IsProLicensed);
        svc.SetProLicense(true);
        Assert.IsTrue(svc.IsProLicensed);
    }

    [TestMethod]
    public async Task IsModelAvailableAsync_ReturnsFalse()
    {
        var svc = new AIService();
        foreach (AIOperation op in Enum.GetValues(typeof(AIOperation)))
        {
            var result = await svc.IsModelAvailableAsync(op);
            Assert.IsFalse(result, $"Expected false for {op}");
        }
    }

    [TestMethod]
    public void AllAIOperations_Defined()
    {
        var values = Enum.GetValues(typeof(AIOperation));
        Assert.AreEqual(7, values.Length);
    }

    [TestMethod]
    public void EnsureProLicense_ThrowsWhenNotLicensed()
    {
        var svc = new AIService();
        // All async methods should throw when not licensed
        Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => svc.RemoveBackgroundAsync(null!, null!));
    }
}
