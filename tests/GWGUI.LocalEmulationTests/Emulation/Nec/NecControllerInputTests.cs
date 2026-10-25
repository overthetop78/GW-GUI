using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Nec.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Nec.Common.Machines.Common.Enums;
using GWGUI.Emulation.Nec.Common.Machines.Common.Functions;

namespace GWGUI.Tests.Emulation.Nec;

public sealed class NecControllerInputTests
{
    [Theory]
    [InlineData(PeripheralCategory.PcFxPad)]
    [InlineData(PeripheralCategory.PcEnginePad)]
    public void ExplicitControllerMappingsUseTheirDeviceInsteadOfTheFirstConnectedController(PeripheralCategory controllerType)
    {
        var xbox360 = new EmulationControllerState(1u << 4, 0, 0, 0, 0, 0, 0)
        {
            DeviceId = "gameinput:xbox360"
        };
        var rematch = new EmulationControllerState(1u << 5, 0, 0, 0, 0, 0, 0)
        {
            DeviceId = "gameinput:rematch"
        };
        var input = EmulationInputSnapshot.Empty with { Controllers = [xbox360, rematch] };
        var configuration = new InputConfiguration(Controllers:
        [
            new ControllerBinding(0, controllerType, Mappings:
                new Dictionary<string, string>
                {
                    ["Up"] = "Controller:gameinput:rematch:DPadUp",
                    ["Down"] = "Controller:gameinput:rematch:DPadDown"
                })
        ]);

        var mapped = InputSnapshotFunctions.Apply(input, configuration, false);

        Assert.Equal(1u << 5, mapped.Controllers[0].Buttons);
    }

    [Theory]
    [InlineData(PeripheralCategory.PcFxPad)]
    [InlineData(PeripheralCategory.PcEnginePad)]
    public void DisconnectedSelectedControllerDoesNotFallBackToAnotherController(PeripheralCategory controllerType)
    {
        var xbox360 = new EmulationControllerState(1u << 4, 0, 0, 0, 0, 0, 0)
        {
            DeviceId = "gameinput:xbox360"
        };
        var input = EmulationInputSnapshot.Empty with { Controllers = [xbox360] };
        var configuration = new InputConfiguration(Controllers: [new ControllerBinding(0, controllerType, "gameinput:rematch")]);

        var mapped = InputSnapshotFunctions.Apply(input, configuration, false);

        Assert.Equal(0u, mapped.Controllers[0].Buttons);
    }
}
