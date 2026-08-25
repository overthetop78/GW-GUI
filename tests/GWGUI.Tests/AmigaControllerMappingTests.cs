using GWGUI.Emulation;
using GWGUI.Emulation.Amiga;

namespace GWGUI.Tests;

public sealed class AmigaControllerMappingTests
{
    [Fact]
    public void FirstEmulatedPortUsesFirstConfiguredControllerBinding()
    {
        var first = new EmulationControllerState(1u << 8, 0, 0, 0, 0, 0, 0)
        {
            DeviceId = "first-controller"
        };
        var second = EmulationControllerState.Empty with
        {
            DeviceId = "second-controller"
        };
        var input = new AmigaInputConfiguration(ControllerBindings:
        [
            new AmigaControllerBinding(0, AmigaControllerType.Cd32Pad, first.DeviceId,
                new Dictionary<string, string> { ["B"] = "Controller:first-controller:ButtonA" }),
            new AmigaControllerBinding(1, AmigaControllerType.Joystick, second.DeviceId,
                new Dictionary<string, string> { ["B"] = "Controller:second-controller:ButtonA" })
        ]);

        var mapped = AmigaInputSnapshotFunctions.Apply(
            new EmulationInputSnapshot(new HashSet<EmulationKey>(),
                EmulationInputSnapshot.Empty.Pointer, [first, second]),
            input, false).Controllers;

        Assert.Equal(1u, mapped[0].Buttons);
        Assert.Equal(0u, mapped[1].Buttons);
    }

    [Fact]
    public void ControllerPointerSwitchUsesKeyboardHotkeyForJoystickDevices()
    {
        var input = new AmigaInputConfiguration(ControllerBindings:
        [
            new AmigaControllerBinding(0, AmigaControllerType.Joystick, null,
                new Dictionary<string, string>())
        ]);

        var mapped = AmigaInputSnapshotFunctions.Apply(EmulationInputSnapshot.Empty, input, true);

        Assert.Contains(EmulationKey.RightControl, mapped.Keys);
        Assert.Equal(0u, mapped.Controllers[0].Buttons & (1u << 2));
    }
}
