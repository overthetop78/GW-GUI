using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Snk.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Snk.Common.Machines.Common.Enums;
using GWGUI.Emulation.Snk.Common.Machines.Common.Functions;

namespace GWGUI.Tests.Emulation.Snk;

public sealed class SnkInputSettingsTests
{
    [Theory]
    [InlineData(MachineModel.NeoGeoAes)]
    [InlineData(MachineModel.NeoGeoMvs)]
    [InlineData(MachineModel.NeoGeoPocket)]
    [InlineData(MachineModel.NeoGeoPocketColor)]
    [InlineData(MachineModel.NeoGeoCd)]
    [InlineData(MachineModel.NeoGeoCdz)]
    public void ControllerAssociationsRemainEmptyUntilExplicitlyAssigned(MachineModel model)
    {
        var configuration = new MachineConfiguration(model, core: Emulator.None);
        var physical = EmulationInputSnapshot.Empty with { Keys = new HashSet<EmulationKey> { EmulationKey.Z,
            EmulationKey.Up,
            EmulationKey.Space },
            Pointer = EmulationInputSnapshot.Empty.Pointer with { Left = true },
            Controllers = [EmulationControllerState.Empty with { Buttons = uint.MaxValue,
            LeftX = short.MaxValue,
            LeftY = short.MaxValue }] };
        Assert.All(InputSnapshotFunctions.Apply(physical, configuration).Controllers, controller => Assert.Equal(EmulationControllerState.Empty, controller));
        var settings = InputSettingsFunctions.Describe(configuration);
        Assert.Null(settings.Keyboard);
        Assert.Null(settings.Mouse);
        Assert.NotEmpty(settings.ControllerPorts);
        foreach (var port in settings.ControllerPorts)
        {
            Assert.Empty(port.Bindings.Values);
            Assert.All(port.Bindings.Definitions, definition => Assert.True(string.IsNullOrEmpty(definition.DefaultBinding)));
        }
        var firstPort = settings.ControllerPorts[0];
        var target = firstPort.Bindings.Definitions[0].Id;
        var assignedPort = firstPort with { Bindings = firstPort.Bindings with { Values = new Dictionary<string, string> { [target] = "Keyboard:Z" } } };
        var assignedSettings = settings with { ControllerPorts = settings.ControllerPorts.Select(port => port.Number == firstPort.Number ? assignedPort : port).ToArray() };
        var assigned = InputSettingsFunctions.Apply(configuration, assignedSettings);
        var assignedSnapshot = InputSnapshotFunctions.Apply(physical, assigned);
        Assert.NotEqual(0u, assignedSnapshot.Controllers[0].Buttons);
        Assert.All(assignedSnapshot.Controllers.Skip(1), controller => Assert.Equal(EmulationControllerState.Empty, controller));
        Assert.Empty(assignedSnapshot.Keys);
        Assert.Equal(EmulationInputSnapshot.Empty.Pointer, assignedSnapshot.Pointer);
        var described = InputSettingsFunctions.Describe(assigned);
        Assert.Equal("Keyboard:Z", described.ControllerPorts[0].Bindings.Values[target]);
        var erasedPort = described.ControllerPorts[0] with { Bindings = described.ControllerPorts[0].Bindings with { Values = new Dictionary<string, string>() } };
        var erasedSettings = described with { ControllerPorts = described.ControllerPorts.Select(port => port.Number == firstPort.Number ? erasedPort : port).ToArray() };
        var erased = InputSettingsFunctions.Apply(assigned, erasedSettings);
        Assert.All(InputSettingsFunctions.Describe(erased).ControllerPorts, port => Assert.Empty(port.Bindings.Values));
        Assert.All(InputSnapshotFunctions.Apply(physical, erased).Controllers, controller => Assert.Equal(EmulationControllerState.Empty, controller));
    }
}
