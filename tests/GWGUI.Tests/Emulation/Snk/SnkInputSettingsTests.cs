using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Snk.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Snk.Common.Machines.Common.Dictionaries;
using GWGUI.Emulation.Snk.Common.Machines.Common.Enums;
using GWGUI.Emulation.Snk.Common.Machines.Common.Functions;
using GWGUI.Emulation.Snk.Common.Interfaces;
using GWGUI.Emulation.Snk.Common.Services;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Factories;

namespace GWGUI.Tests.Emulation.Snk;

public sealed class SnkInputSettingsTests
{
    [Theory]
    [InlineData(Emulator.NeoCd, 10)]
    [InlineData(Emulator.Mame2015, 10)]
    [InlineData(Emulator.FbAlpha2012NeoGeo, 10)]
    [InlineData(Emulator.Mame2003Plus, 10)]
    [InlineData(Emulator.FbAlpha2012, 10)]
    [InlineData(Emulator.FbAlpha, 10)]
    [InlineData(Emulator.Mame2000, 10)]
    [InlineData(Emulator.Geolith, 10)]
    [InlineData(Emulator.HbMame, 10)]
    [InlineData(Emulator.Race, 7)]
    [InlineData(Emulator.MednafenNgp, 7)]
    public void NativeControllerMapsKeepEachCallIsolated(Emulator emulator, int expectedCount)
    {
        var factory = Assert.IsAssignableFrom<LibretroMachineFactory>(new Engine().Adapter(emulator));
        var configuration = new MachineConfiguration(ModelCatalog.Parse(factory.Definition.MachineIds.First()), core: emulator);
        var first = Assert.IsType<Dictionary<ControllerCommand, uint>>(factory.ControllerButtonIds(configuration));
        var expected = first.OrderBy(pair => pair.Key).ToArray();
        var second = factory.ControllerButtonIds(configuration);
        Assert.Equal(expectedCount, expected.Length);
        Assert.NotSame(first, second);
        first.Clear();
        Assert.Equal(expected, second.OrderBy(pair => pair.Key));
        var third = factory.ControllerButtonIds(configuration);
        Assert.NotSame(second, third);
        Assert.Equal(expected, third.OrderBy(pair => pair.Key));
    }

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
        var mapper = new Engine().Adapters(configuration.MachineId).OfType<IEmulatorInputMapper>().First();
        var physical = EmulationInputSnapshot.Empty with { Keys = new HashSet<EmulationKey> { EmulationKey.Z,
            EmulationKey.Up,
            EmulationKey.Space },
            Pointer = EmulationInputSnapshot.Empty.Pointer with { Left = true },
            Controllers = [EmulationControllerState.Empty with { Buttons = uint.MaxValue,
            LeftX = short.MaxValue,
            LeftY = short.MaxValue }] };
        Assert.All(InputSnapshotFunctions.Apply(physical, configuration, mapper).Controllers, controller => Assert.Equal(EmulationControllerState.Empty, controller));
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
        foreach (var source in new[] { "Keyboard:Z", "Keyboard:Z+Mouse:Left+Controller:A" })
        {
            var assignedPort = firstPort with { Bindings = firstPort.Bindings with { Values = new Dictionary<string, string> { [target] = source } } };
            var assignedSettings = settings with { ControllerPorts = settings.ControllerPorts.Select(port => port.Number == firstPort.Number ? assignedPort : port).ToArray() };
            var assigned = InputSettingsFunctions.Apply(configuration, assignedSettings);
            var assignedSnapshot = InputSnapshotFunctions.Apply(physical, assigned, mapper);
            Assert.NotEqual(0u, assignedSnapshot.Controllers[0].Buttons);
            Assert.All(assignedSnapshot.Controllers.Skip(1), controller => Assert.Equal(EmulationControllerState.Empty, controller));
            Assert.Empty(assignedSnapshot.Keys);
            Assert.Equal(EmulationInputSnapshot.Empty.Pointer, assignedSnapshot.Pointer);
            var described = InputSettingsFunctions.Describe(assigned);
            Assert.Equal(source, described.ControllerPorts[0].Bindings.Values[target]);
            if (source.Contains('+'))
                foreach (var released in new[]
                {
                    physical with { Keys = EmulationInputSnapshot.Empty.Keys },
                    physical with { Pointer = EmulationInputSnapshot.Empty.Pointer },
                    physical with { Controllers = [EmulationControllerState.Empty] }
                }) Assert.Equal(0u, InputSnapshotFunctions.Apply(released, assigned, mapper).Controllers[0].Buttons);
            var erasedPort = described.ControllerPorts[0] with { Bindings = described.ControllerPorts[0].Bindings with { Values = new Dictionary<string, string>() } };
            var erasedSettings = described with { ControllerPorts = described.ControllerPorts.Select(port => port.Number == firstPort.Number ? erasedPort : port).ToArray() };
            var erased = InputSettingsFunctions.Apply(assigned, erasedSettings);
            Assert.All(InputSettingsFunctions.Describe(erased).ControllerPorts, port => Assert.Empty(port.Bindings.Values));
            Assert.All(InputSnapshotFunctions.Apply(physical, erased, mapper).Controllers, controller => Assert.Equal(EmulationControllerState.Empty, controller));
        }
    }
}
