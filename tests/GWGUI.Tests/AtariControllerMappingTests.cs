using GWGUI.App.Controls;
using GWGUI.Emulation;
using GWGUI.Emulation.Atari;

namespace GWGUI.Tests;

public sealed class AtariControllerMappingTests
{
    [Fact]
    public void CustomControllerMappingIsAppliedBeforeLibretroInput()
    {
        var physical = new EmulationControllerState(1u << 8, 0, 0, 0, 0, 0, 0);
        var input = new AtariInputConfiguration(Controllers:
        [
            new AtariControllerBinding(0, AtariPeripheralKind.Joystick,
                Mappings: new Dictionary<string, string> { ["Fire1"] = "Controller:ButtonA" })
        ]);

        var mapped = AtariMachineInputFunctions.ApplyControllerMappings(
            new HashSet<EmulationKey>(), [physical], input);

        Assert.Equal(1u, mapped[0].Buttons);
    }

    [Fact]
    public void LynxSystemButtonsRemainInTheControllerMapping()
    {
        var physical = new EmulationControllerState((1u << 10) | (1u << 3), 0, 0, 0, 0, 0, 0);
        var input = new AtariInputConfiguration(Controllers:
        [
            new AtariControllerBinding(0, AtariPeripheralKind.EnhancedController,
                Mappings: new Dictionary<string, string>
                {
                    ["Option1"] = "Controller:LeftShoulder",
                    ["Pause"] = "Controller:Menu"
                })
        ]);

        var mapped = AtariMachineInputFunctions.ApplyControllerMappings(
            new HashSet<EmulationKey>(), [physical], input, AtariMachineModel.Lynx);

        Assert.NotEqual(0u, mapped[0].Buttons & (1u << 10));
        Assert.NotEqual(0u, mapped[0].Buttons & (1u << 3));
    }
}
