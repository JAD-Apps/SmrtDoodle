using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmrtDoodle.Services;

namespace SmrtDoodle.Tests;

[TestClass]
public class LicenseServiceTests
{
    [TestMethod]
    public void Default_NotProLicensed()
    {
        var svc = new LicenseService();
        Assert.IsFalse(svc.IsProLicensed);
    }

    [TestMethod]
    public void ResetCache_ClearsState()
    {
        var svc = new LicenseService();
        svc.ResetCache();
        Assert.IsFalse(svc.IsProLicensed);
    }

    [TestMethod]
    public async Task CheckProLicenseAsync_DefaultFalse()
    {
        // In test environment, Store APIs not available, should default to false
        var svc = new LicenseService();
        var result = await svc.CheckProLicenseAsync();
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task CheckProLicenseAsync_CachesResult()
    {
        var svc = new LicenseService();
        var first = await svc.CheckProLicenseAsync();
        var second = await svc.CheckProLicenseAsync();
        Assert.AreEqual(first, second);
    }

    [TestMethod]
    public async Task PurchaseProAsync_FailsGracefully()
    {
        // In test environment, Store APIs not available
        var svc = new LicenseService();
        var result = await svc.PurchaseProAsync();
        Assert.IsFalse(result);
    }
}
