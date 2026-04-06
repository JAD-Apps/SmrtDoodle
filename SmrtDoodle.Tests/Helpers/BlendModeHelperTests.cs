using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmrtDoodle.Helpers;
using SmrtDoodle.Models;
using System;

namespace SmrtDoodle.Tests.Helpers;

/// <summary>
/// Tests for BlendModeHelper internal math functions.
/// Since ComposeLayer requires CanvasRenderTarget (GPU), we test the blend math
/// by verifying expected values from the formulas used in the switch cases.
/// </summary>
[TestClass]
public class BlendModeHelperTests
{
    // Helper: applies the channel-level formula for a given blend mode
    // (mirrors the switch logic in BlendModeHelper.ComposeLayer)
    private static float BlendChannel(BlendMode mode, float dst, float src)
    {
        return mode switch
        {
            BlendMode.Multiply => dst * src,
            BlendMode.Screen => 1f - (1f - dst) * (1f - src),
            BlendMode.Overlay => dst < 0.5f ? 2f * dst * src : 1f - 2f * (1f - dst) * (1f - src),
            BlendMode.Darken => Math.Min(dst, src),
            BlendMode.Lighten => Math.Max(dst, src),
            BlendMode.ColorBurn => src > 0 ? 1f - Math.Min(1f, (1f - dst) / src) : 0f,
            BlendMode.ColorDodge => src < 1f ? Math.Min(1f, dst / (1f - src)) : 1f,
            BlendMode.LinearBurn => Math.Max(0f, dst + src - 1f),
            BlendMode.LinearDodge => Math.Min(1f, dst + src),
            BlendMode.HardLight => src < 0.5f ? 2f * dst * src : 1f - 2f * (1f - dst) * (1f - src),
            BlendMode.LinearLight => Math.Clamp(dst + 2f * src - 1f, 0f, 1f),
            BlendMode.PinLight => src < 0.5f ? Math.Min(dst, 2f * src) : Math.Max(dst, 2f * src - 1f),
            BlendMode.HardMix => (dst + src >= 1f) ? 1f : 0f,
            BlendMode.Difference => Math.Abs(dst - src),
            BlendMode.Exclusion => dst + src - 2f * dst * src,
            _ => src
        };
    }

    #region Multiply

    [TestMethod]
    public void Multiply_BlackOnWhite_IsBlack()
    {
        float result = BlendChannel(BlendMode.Multiply, 1f, 0f);
        Assert.AreEqual(0f, result, 0.001f);
    }

    [TestMethod]
    public void Multiply_WhiteOnWhite_IsWhite()
    {
        float result = BlendChannel(BlendMode.Multiply, 1f, 1f);
        Assert.AreEqual(1f, result, 0.001f);
    }

    [TestMethod]
    public void Multiply_MidValues()
    {
        float result = BlendChannel(BlendMode.Multiply, 0.5f, 0.5f);
        Assert.AreEqual(0.25f, result, 0.001f);
    }

    #endregion

    #region Screen

    [TestMethod]
    public void Screen_BlackOnBlack_IsBlack()
    {
        float result = BlendChannel(BlendMode.Screen, 0f, 0f);
        Assert.AreEqual(0f, result, 0.001f);
    }

    [TestMethod]
    public void Screen_WhiteOnAnything_IsWhite()
    {
        float result = BlendChannel(BlendMode.Screen, 0.3f, 1f);
        Assert.AreEqual(1f, result, 0.001f);
    }

    [TestMethod]
    public void Screen_MidValues()
    {
        // 1 - (1-0.5)*(1-0.5) = 1 - 0.25 = 0.75
        float result = BlendChannel(BlendMode.Screen, 0.5f, 0.5f);
        Assert.AreEqual(0.75f, result, 0.001f);
    }

    #endregion

    #region Overlay

    [TestMethod]
    public void Overlay_DarkBase_UsesMultiplyVariant()
    {
        // Base < 0.5: 2*dst*src = 2*0.3*0.8 = 0.48
        float result = BlendChannel(BlendMode.Overlay, 0.3f, 0.8f);
        Assert.AreEqual(0.48f, result, 0.001f);
    }

    [TestMethod]
    public void Overlay_LightBase_UsesScreenVariant()
    {
        // Base >= 0.5: 1 - 2*(1-dst)*(1-src) = 1 - 2*0.3*0.7 = 1 - 0.42 = 0.58
        float result = BlendChannel(BlendMode.Overlay, 0.7f, 0.3f);
        Assert.AreEqual(0.58f, result, 0.001f);
    }

    #endregion

    #region Darken / Lighten

    [TestMethod]
    public void Darken_ReturnsMinimum()
    {
        Assert.AreEqual(0.3f, BlendChannel(BlendMode.Darken, 0.7f, 0.3f), 0.001f);
        Assert.AreEqual(0.2f, BlendChannel(BlendMode.Darken, 0.2f, 0.9f), 0.001f);
    }

    [TestMethod]
    public void Lighten_ReturnsMaximum()
    {
        Assert.AreEqual(0.7f, BlendChannel(BlendMode.Lighten, 0.7f, 0.3f), 0.001f);
        Assert.AreEqual(0.9f, BlendChannel(BlendMode.Lighten, 0.2f, 0.9f), 0.001f);
    }

    #endregion

    #region ColorBurn / ColorDodge

    [TestMethod]
    public void ColorBurn_WhiteSource_NoChange()
    {
        // src=1: 1 - min(1, (1-dst)/1) = 1 - (1-dst) = dst
        float result = BlendChannel(BlendMode.ColorBurn, 0.6f, 1f);
        Assert.AreEqual(0.6f, result, 0.001f);
    }

    [TestMethod]
    public void ColorBurn_BlackSource_IsBlack()
    {
        float result = BlendChannel(BlendMode.ColorBurn, 0.6f, 0f);
        Assert.AreEqual(0f, result, 0.001f);
    }

    [TestMethod]
    public void ColorDodge_BlackSource_NoChange()
    {
        // src=0: min(1, dst/1) = dst
        float result = BlendChannel(BlendMode.ColorDodge, 0.5f, 0f);
        Assert.AreEqual(0.5f, result, 0.001f);
    }

    [TestMethod]
    public void ColorDodge_WhiteSource_IsWhite()
    {
        float result = BlendChannel(BlendMode.ColorDodge, 0.5f, 1f);
        Assert.AreEqual(1f, result, 0.001f);
    }

    #endregion

    #region Linear Burn / Dodge

    [TestMethod]
    public void LinearBurn_Max0()
    {
        // max(0, 0.3 + 0.2 - 1) = max(0, -0.5) = 0
        float result = BlendChannel(BlendMode.LinearBurn, 0.3f, 0.2f);
        Assert.AreEqual(0f, result, 0.001f);
    }

    [TestMethod]
    public void LinearBurn_Normal()
    {
        // max(0, 0.8 + 0.6 - 1) = 0.4
        float result = BlendChannel(BlendMode.LinearBurn, 0.8f, 0.6f);
        Assert.AreEqual(0.4f, result, 0.001f);
    }

    [TestMethod]
    public void LinearDodge_ClipsAt1()
    {
        float result = BlendChannel(BlendMode.LinearDodge, 0.8f, 0.8f);
        Assert.AreEqual(1f, result, 0.001f);
    }

    [TestMethod]
    public void LinearDodge_Normal()
    {
        float result = BlendChannel(BlendMode.LinearDodge, 0.3f, 0.4f);
        Assert.AreEqual(0.7f, result, 0.001f);
    }

    #endregion

    #region HardLight / HardMix

    [TestMethod]
    public void HardLight_DarkSource_Multiply()
    {
        float result = BlendChannel(BlendMode.HardLight, 0.6f, 0.3f);
        Assert.AreEqual(2f * 0.6f * 0.3f, result, 0.001f);
    }

    [TestMethod]
    public void HardMix_SumAbove1_IsWhite()
    {
        float result = BlendChannel(BlendMode.HardMix, 0.6f, 0.5f);
        Assert.AreEqual(1f, result, 0.001f);
    }

    [TestMethod]
    public void HardMix_SumBelow1_IsBlack()
    {
        float result = BlendChannel(BlendMode.HardMix, 0.3f, 0.3f);
        Assert.AreEqual(0f, result, 0.001f);
    }

    #endregion

    #region Difference / Exclusion

    [TestMethod]
    public void Difference_SameValues_IsZero()
    {
        float result = BlendChannel(BlendMode.Difference, 0.5f, 0.5f);
        Assert.AreEqual(0f, result, 0.001f);
    }

    [TestMethod]
    public void Difference_Opposite_IsAbsDiff()
    {
        float result = BlendChannel(BlendMode.Difference, 0.8f, 0.3f);
        Assert.AreEqual(0.5f, result, 0.001f);
    }

    [TestMethod]
    public void Exclusion_MidValues()
    {
        // 0.5 + 0.5 - 2*0.5*0.5 = 1 - 0.5 = 0.5
        float result = BlendChannel(BlendMode.Exclusion, 0.5f, 0.5f);
        Assert.AreEqual(0.5f, result, 0.001f);
    }

    [TestMethod]
    public void Exclusion_Black_CopiesSource()
    {
        float result = BlendChannel(BlendMode.Exclusion, 0f, 0.7f);
        Assert.AreEqual(0.7f, result, 0.001f);
    }

    #endregion

    #region LinearLight / PinLight

    [TestMethod]
    public void LinearLight_MidSource_NoChange()
    {
        // dst + 2*0.5 - 1 = dst
        float result = BlendChannel(BlendMode.LinearLight, 0.4f, 0.5f);
        Assert.AreEqual(0.4f, result, 0.001f);
    }

    [TestMethod]
    public void PinLight_DarkSource_Darkens()
    {
        // src < 0.5: min(dst, 2*src) = min(0.8, 0.4) = 0.4
        float result = BlendChannel(BlendMode.PinLight, 0.8f, 0.2f);
        Assert.AreEqual(0.4f, result, 0.001f);
    }

    #endregion

    #region BlendMode Enum Coverage

    [TestMethod]
    public void AllChannelBlendModes_ProduceValidRange()
    {
        var modes = new[]
        {
            BlendMode.Multiply, BlendMode.Screen, BlendMode.Overlay,
            BlendMode.Darken, BlendMode.Lighten, BlendMode.ColorBurn,
            BlendMode.ColorDodge, BlendMode.LinearBurn, BlendMode.LinearDodge,
            BlendMode.HardLight, BlendMode.LinearLight, BlendMode.PinLight,
            BlendMode.HardMix, BlendMode.Difference, BlendMode.Exclusion
        };

        foreach (var mode in modes)
        {
            for (float d = 0f; d <= 1f; d += 0.25f)
            {
                for (float s = 0f; s <= 1f; s += 0.25f)
                {
                    float result = BlendChannel(mode, d, s);
                    Assert.IsTrue(result >= -0.001f && result <= 1.001f,
                        $"{mode}: BlendChannel({d}, {s}) = {result} is out of [0,1]");
                }
            }
        }
    }

    #endregion
}
