using GWGUI.App.Services.Input.GameInput;

namespace GWGUI.Tests;

public sealed class GameInputDeviceModelCatalogTests
{
    [Theory]
    [InlineData(0x045E, 0x0B12, "Xbox Series X Controller", "XboxSeries", false)]
    [InlineData(0x10F5, 0x7122, "Xbox Rematch Core Wired Controller- Black", "XboxRematchCore", true)]
    [InlineData(0x0810, 0xE501, "SEGA Mega Drive 6 boutons", "MegaDrive6", true)]
    [InlineData(0x0079, 0x0006, "Manette Nintendo 64", "Nintendo64", true)]
    [InlineData(0x081F, 0xE401, "Manette rétro USB (081F:E401)", "GenericGamepad", false)]
    [InlineData(0x054C, 0x05C4, "DUALSHOCK 4 Wireless Controller", "PlayStation4", true)]
    [InlineData(0x054C, 0x09CC, "DUALSHOCK 4 Wireless Controller", "PlayStation4", true)]
    [InlineData(0x054C, 0x0CE6, "DualSense Wireless Controller", "PlayStation5", true)]
    public void EveryKnownVidPidResolvesANameSuggestedVisualAndCertainty(
        ushort vendorId,
        ushort productId,
        string expectedName,
        string expectedModel,
        bool expectedExact)
    {
        Assert.Equal(expectedName, GameInputDeviceModelCatalog.ResolveProductName(
            vendorId, productId, "transport", "database", "GameInput"));
        var visual = GameInputDeviceModelCatalog.ResolveVisualModel(
            vendorId, productId, expectedName, GameInputKind.Gamepad);
        Assert.Equal(expectedModel, visual.Model.ToString());
        Assert.Equal(expectedExact, visual.Exact);
    }

    [Theory]
    [InlineData("Unknown Xbox compatible controller", 0x00040000u, "XboxOne")]
    [InlineData("Unknown wheel", 0x00080000u, "RacingWheel")]
    [InlineData("Unknown flight device", 0x00020000u, "FlightStick")]
    [InlineData("Unknown arcade device", 0x00010000u, "ArcadeStick")]
    [InlineData("Unknown HID controller", 0x0000000Eu, "GenericGamepad")]
    public void NonExactDevicesKeepTheManualVisualSelectorAvailable(
        string productName,
        uint kind,
        string expectedModel)
    {
        var visual = GameInputDeviceModelCatalog.ResolveVisualModel(
            0xFFFF, 0xFFFF, productName, (GameInputKind)kind);
        Assert.Equal(expectedModel, visual.Model.ToString());
        Assert.False(visual.Exact);
    }
}
