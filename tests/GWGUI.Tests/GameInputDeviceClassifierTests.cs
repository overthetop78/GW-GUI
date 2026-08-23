using GWGUI.App.Services.Input.GameInput;

namespace GWGUI.Tests;

public sealed class GameInputDeviceClassifierTests
{
    [Theory]
    [InlineData(0x00010000u)]
    [InlineData(0x00020000u)]
    [InlineData(0x00040000u)]
    [InlineData(0x00080000u)]
    [InlineData(0x00000002u)]
    [InlineData(0x00000004u)]
    [InlineData(0x00000008u)]
    public void EveryStandardGamingKindIsIncluded(uint kind)
    {
        Assert.True(GameInputDeviceClassifier.IsGamingController(
            Descriptor((GameInputKind)kind, 0, 0)));
    }

    [Theory]
    [InlineData(0x01, 0x04)]
    [InlineData(0x01, 0x05)]
    [InlineData(0x01, 0x08)]
    public void RawHidJoystickGamepadAndMultiAxisDevicesAreIncluded(ushort page, ushort usage)
    {
        Assert.True(GameInputDeviceClassifier.IsGamingController(
            Descriptor(GameInputKind.RawDeviceReport, page, usage)));
    }

    [Theory]
    [InlineData(0x00000010u, 0x01, 0x06)]
    [InlineData(0x00000020u, 0x01, 0x02)]
    [InlineData(0x00000001u, 0x01, 0x02)]
    [InlineData(0x00000001u, 0x0C, 0x01)]
    [InlineData(0x00000040u, 0x20, 0x01)]
    public void NonGamingGameInputDevicesAreExcluded(uint kind, ushort page, ushort usage)
    {
        Assert.False(GameInputDeviceClassifier.IsGamingController(
            Descriptor((GameInputKind)kind, page, usage)));
    }

    private static GameInputDeviceDescriptor Descriptor(
        GameInputKind kind, ushort page, ushort usage) => new(
        "gameinput:classifier", "Device", "Device", string.Empty,
        1, 1, 1, new(), new(), "root", Guid.Empty,
        GameInputDeviceFamily.Hid, new GameInputUsage { Page = page, Id = usage },
        kind, GameInputRumbleMotors.None, GameInputSystemButtons.None,
        string.Empty, [], [],
        new(GameInputGamepadButtons.None, 0, 0, false, false, false, false,
            false, false, false, 0,
            new Dictionary<GameInputKind, IReadOnlyList<byte>>(),
            new Dictionary<GameInputKind, IReadOnlyList<byte>>()),
        [], [], [], false, string.Empty, [],
        ControllerVisualModel.GenericGamepad, false);
}
