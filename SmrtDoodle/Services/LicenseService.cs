using System;
using System.Threading.Tasks;
using Windows.Services.Store;

namespace SmrtDoodle.Services;

/// <summary>
/// Manages in-app purchase licensing for Pro/AI features.
/// Free tier: all drawing tools, file formats, layers.
/// Pro tier: AI features (background removal, upscaling, content-fill, colorize, style transfer, smart select, denoise).
/// </summary>
public class LicenseService
{
    private const string ProProductId = "SmrtDoodlePro";

    private StoreContext? _storeContext;
    private bool _isProLicensed;
    private bool _checkedLicense;

    public bool IsProLicensed => _isProLicensed;

    /// <summary>
    /// Check if the user owns the Pro add-on. Caches the result.
    /// </summary>
    public async Task<bool> CheckProLicenseAsync()
    {
        if (_checkedLicense) return _isProLicensed;

        try
        {
            _storeContext ??= StoreContext.GetDefault();
            var result = await _storeContext.GetAppLicenseAsync();
            if (result?.AddOnLicenses != null)
            {
                foreach (var license in result.AddOnLicenses)
                {
                    if (license.Value.SkuStoreId.StartsWith(ProProductId, StringComparison.OrdinalIgnoreCase)
                        && license.Value.IsActive)
                    {
                        _isProLicensed = true;
                        break;
                    }
                }
            }
        }
        catch (Exception)
        {
            // Store API not available (dev/sideloaded) — default to unlicensed
            _isProLicensed = false;
        }

        _checkedLicense = true;
        return _isProLicensed;
    }

    /// <summary>
    /// Launch the Store purchase flow for the Pro add-on.
    /// Returns true if purchase succeeded.
    /// </summary>
    public async Task<bool> PurchaseProAsync()
    {
        try
        {
            _storeContext ??= StoreContext.GetDefault();
            var result = await _storeContext.RequestPurchaseAsync(ProProductId);

            if (result.Status == StorePurchaseStatus.Succeeded
                || result.Status == StorePurchaseStatus.AlreadyPurchased)
            {
                _isProLicensed = true;
                _checkedLicense = true;
                return true;
            }
        }
        catch (Exception)
        {
            // Store not available
        }

        return false;
    }

    /// <summary>
    /// Reset cached license state (e.g., after restore purchases).
    /// </summary>
    public void ResetCache()
    {
        _checkedLicense = false;
        _isProLicensed = false;
    }
}
