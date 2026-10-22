using System.Reflection;
using GWGUI.Emulation;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Example.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Example.Common.Machines.Common.Enums;
using GWGUI.Emulation.Example.Modules;

namespace GWGUI.Tests.Emulation;

public sealed class ExampleMouseInputTests
{
    [Fact]
    public void PortMouseMapsItsButtonsAndWheelsToThePointer()
    {
        var mappings = new Dictionary<string, string>
        {
            ["Left"] = "Keyboard:A", ["Right"] = "Keyboard:B", ["Middle"] = "Keyboard:C",
            ["XButton1"] = "Keyboard:D", ["XButton2"] = "Keyboard:E", ["WheelUp"] = "Keyboard:F",
            ["WheelDown"] = "Keyboard:G", ["WheelLeft"] = "Keyboard:H", ["WheelRight"] = "Keyboard:I"
        };
        var snapshot = EmulationInputSnapshot.Empty with
        {
            Keys = new HashSet<EmulationKey> { EmulationKey.A, EmulationKey.B, EmulationKey.C, EmulationKey.D, EmulationKey.E, EmulationKey.F, EmulationKey.H },
            Pointer = new EmulationPointerState(23, -17, 4, false, false, false, HorizontalWheel: -3)
        };
        var result = Apply(snapshot, new InputConfiguration(Controllers: [new ControllerBinding(0, PeripheralCategory.Mouse, Mappings: mappings)]));

        Assert.Equal(new EmulationPointerState(23, -17, 1, true, true, true, true, true, -1), result.Pointer);
        Assert.Equal(EmulationControllerState.Empty, result.Controllers[0]);
        Assert.Equal(1, EmulationSdk.InputProcessing.ReadNativeInput(result, 0, 2, 0, 2));
        Assert.Equal(1, EmulationSdk.InputProcessing.ReadNativeInput(result, 0, 2, 0, 4));
        Assert.Equal(23, EmulationSdk.InputProcessing.ReadNativeInput(result, 0, 2, 0, 0));
        Assert.False(snapshot.Pointer.Left);
        Assert.Equal(4, snapshot.Pointer.Wheel);
    }

    [Fact]
    public void MainMouseMappingsKeepTheirExistingProjection()
    {
        var snapshot = EmulationInputSnapshot.Empty with { Keys = new HashSet<EmulationKey> { EmulationKey.A }, Pointer = new EmulationPointerState(-8, 12, 7, false, true, false, HorizontalWheel: -4) };
        var result = Apply(snapshot, new InputConfiguration(MouseMappings: new Dictionary<string, string> { ["Left"] = "Keyboard:A" }));
        Assert.Equal(snapshot.Pointer with { Left = true }, result.Pointer);
    }

    [Fact]
    public void MainMappingsOverrideOnlyTheirExplicitCommandsAfterPortMappings()
    {
        var snapshot = EmulationInputSnapshot.Empty with { Keys = new HashSet<EmulationKey> { EmulationKey.A } };
        var configuration = new InputConfiguration(
            Controllers: [new ControllerBinding(0, PeripheralCategory.Mouse, Mappings: new Dictionary<string, string> { ["Left"] = "Keyboard:A", ["Right"] = "Keyboard:A" })],
            MouseMappings: new Dictionary<string, string> { ["Left"] = "Keyboard:B" });
        var result = Apply(snapshot, configuration);
        Assert.False(result.Pointer.Left);
        Assert.True(result.Pointer.Right);
    }

    private static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot, InputConfiguration configuration)
    {
        var functions = typeof(ExampleModuleFactory).Assembly.GetType("GWGUI.Emulation.Example.Common.Machines.Common.Functions.InputSnapshotFunctions", throwOnError: true)!;
        return (EmulationInputSnapshot)functions.GetMethod("Apply", BindingFlags.Static | BindingFlags.NonPublic, [typeof(EmulationInputSnapshot), typeof(InputConfiguration)])!.Invoke(null, [snapshot, configuration])!;
    }
}
