using System.Reflection;
using GWGUI.Emulation;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Example.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Example.Common.Machines.Common.Enums;
using GWGUI.Emulation.Example.Modules;

namespace GWGUI.Tests.Emulation;

public sealed class ExampleAnalogInputTests
{
    [Fact]
    public void MappedSticksAndTriggersReachTheNativeFields()
    {
        var mappings = new Dictionary<string, string>
        {
            ["LeftStickRight"] = "Keyboard:A", ["LeftStickUp"] = "Keyboard:B",
            ["RightStickLeft"] = "Keyboard:C", ["RightStickDown"] = "Keyboard:D",
            ["L2"] = "Keyboard:E", ["R2"] = "Keyboard:F", ["A"] = "Keyboard:G"
        };
        var snapshot = EmulationInputSnapshot.Empty with { Keys = new HashSet<EmulationKey> { EmulationKey.A, EmulationKey.B, EmulationKey.C, EmulationKey.D, EmulationKey.E, EmulationKey.F, EmulationKey.G } };
        var result = Apply(snapshot, mappings);
        var controller = result.Controllers[0];

        Assert.Equal(short.MaxValue, controller.LeftX);
        Assert.Equal(-short.MaxValue, controller.LeftY);
        Assert.Equal(-short.MaxValue, controller.RightX);
        Assert.Equal(short.MaxValue, controller.RightY);
        Assert.Equal(short.MaxValue, controller.LeftTrigger);
        Assert.Equal(short.MaxValue, controller.RightTrigger);
        Assert.Equal(1f, controller.Controls["L2"]);
        Assert.Equal(controller.LeftX, EmulationSdk.InputProcessing.ReadNativeInput(result, 0, 5, 0, 0));
        Assert.Equal(controller.LeftY, EmulationSdk.InputProcessing.ReadNativeInput(result, 0, 5, 0, 1));
        Assert.Equal(controller.RightX, EmulationSdk.InputProcessing.ReadNativeInput(result, 0, 5, 1, 0));
        Assert.Equal(controller.RightY, EmulationSdk.InputProcessing.ReadNativeInput(result, 0, 5, 1, 1));
        Assert.Equal(1, EmulationSdk.InputProcessing.ReadNativeInput(result, 0, 1, 0, (uint)EmulationSdk.InputProcessing.ControllerButtonIndex("A")));
        Assert.Equal(EmulationControllerState.Empty, snapshot.Controllers[0]);
    }

    [Fact]
    public void OppositeMappedDirectionsCancelOut()
    {
        var snapshot = EmulationInputSnapshot.Empty with { Keys = new HashSet<EmulationKey> { EmulationKey.A } };
        var result = Apply(snapshot, new Dictionary<string, string> { ["LeftStickRight"] = "Keyboard:A", ["LeftStickLeft"] = "Keyboard:A" });
        Assert.Equal(0, result.Controllers[0].LeftX);
        Assert.Equal(1f, result.Controllers[0].Controls["LeftStickRight"]);
        Assert.Equal(1f, result.Controllers[0].Controls["LeftStickLeft"]);
    }

    [Fact]
    public void DeadZoneIsAppliedBeforeConversionWithoutLeakingUnmappedAxes()
    {
        // Les axes physiques génériques sont centrés à 0,5 : ces positions donnent 0,1 et 0,75 de course positive.
        var source = EmulationControllerState.Empty with { DeviceId = "pad", RightY = 1234, Controls = new EmulationControllerControls(new Dictionary<string, float> { ["Axis0"] = .55f, ["Axis1"] = .875f }) };
        var snapshot = EmulationInputSnapshot.Empty with { Controllers = [source] };
        var result = Apply(snapshot, new Dictionary<string, string> { ["LeftStickRight"] = "Controller:pad:Axis0Positive", ["RightStickRight"] = "Controller:pad:Axis1Positive", ["L2"] = "Controller:pad:Axis1Positive" });
        var controller = Assert.Single(result.Controllers);

        Assert.Equal(0, controller.LeftX);
        Assert.Equal((short)(.75f * short.MaxValue), controller.RightX);
        Assert.Equal((short)(.75f * short.MaxValue), controller.LeftTrigger);
        Assert.Equal(0, controller.RightY);
        Assert.Equal(0, controller.RightTrigger);
        Assert.Same(source, Assert.Single(snapshot.Controllers));
    }

    [Fact]
    public void AllThreeBackendsAndBothModelsUseTheSameInputProjection()
    {
        var assembly = typeof(ExampleModuleFactory).Assembly;
        var contract = assembly.GetType("GWGUI.Emulation.Example.Common.Interfaces.IEmulatorInputMapper", throwOnError: true)!;
        var map = contract.GetMethod("MapInput")!;
        var backends = assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && contract.IsAssignableFrom(type)).ToArray();
        var models = GWGUI.Emulation.Example.Common.Machines.Common.Dictionaries.HardwareModelCatalog.All;
        Assert.Equal(3, backends.Length);
        Assert.Equal(2, models.Count);
        var input = new InputConfiguration(Controllers:
        [
            new ControllerBinding(0, PeripheralCategory.RetroPad, Mappings: new Dictionary<string, string> { ["LeftStickRight"] = "Keyboard:A" }),
            new ControllerBinding(1, PeripheralCategory.Keyboard, Mappings: new Dictionary<string, string> { ["B"] = "Keyboard:A" })
        ], MouseMappings: new Dictionary<string, string> { ["Left"] = "Keyboard:A" });
        var snapshot = EmulationInputSnapshot.Empty with { Keys = new HashSet<EmulationKey> { EmulationKey.A } };
        foreach (var backend in backends)
        foreach (var model in models)
        {
            var adapter = Activator.CreateInstance(backend, nonPublic: true)!;
            var configuration = new MachineConfiguration(model.Model, input: input);
            var result = Assert.IsType<EmulationInputSnapshot>(map.Invoke(adapter, [snapshot, configuration]));
            Assert.Equal(short.MaxValue, result.Controllers[0].LeftX);
            Assert.Contains(EmulationKey.B, result.Keys);
            Assert.True(result.Pointer.Left);
            Assert.Equal(EmulationControllerState.Empty, result.Controllers[1]);
        }
    }

    private static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot, IReadOnlyDictionary<string, string> mappings)
    {
        var configuration = new InputConfiguration(Controllers: [new ControllerBinding(0, PeripheralCategory.RetroPad, Mappings: mappings)]);
        var functions = typeof(ExampleModuleFactory).Assembly.GetType("GWGUI.Emulation.Example.Common.Machines.Common.Functions.InputSnapshotFunctions", throwOnError: true)!;
        return (EmulationInputSnapshot)functions.GetMethod("Apply", BindingFlags.Static | BindingFlags.NonPublic, [typeof(EmulationInputSnapshot), typeof(InputConfiguration)])!.Invoke(null, [snapshot, configuration])!;
    }
}
