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
        Assert.Equal(short.MaxValue, EmulationSdk.InputProcessing.ReadNativeInput(result, 0, 5, 2, 12));
        Assert.Equal(short.MaxValue, EmulationSdk.InputProcessing.ReadNativeInput(result, 0, 5, 2, 13));
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

    [Fact]
    public void EveryGenericCategoryUsesItsNativeDeviceClass()
    {
        var expected = new Dictionary<PeripheralCategory, uint>
        {
            [PeripheralCategory.None] = 0, [PeripheralCategory.Automatic] = 1, [PeripheralCategory.Keyboard] = 3,
            [PeripheralCategory.Mouse] = 2, [PeripheralCategory.RetroPad] = 1, [PeripheralCategory.Joystick] = 1,
            [PeripheralCategory.AnalogJoystick] = 5, [PeripheralCategory.DrivingController] = 5, [PeripheralCategory.LightGun] = 4,
            [PeripheralCategory.Paddle] = 5, [PeripheralCategory.NumericKeypad] = 1, [PeripheralCategory.DanceMat] = 1, [PeripheralCategory.Pointer] = 6
        };
        Assert.Equal(Enum.GetValues<PeripheralCategory>().Order(), expected.Keys.Order());
        foreach (var (category, device) in expected) Assert.Equal(device, ResolveControllerDevice(category));
        Assert.IsType<ArgumentOutOfRangeException>(Assert.Throws<TargetInvocationException>(() => ResolveControllerDevice((PeripheralCategory)int.MaxValue)).InnerException);
    }

    [Fact]
    public void PointingAssociationsReachNativeReadsForEachBackendAndProfile()
    {
        var assembly = typeof(ExampleModuleFactory).Assembly;
        var mapperContract = assembly.GetType("GWGUI.Emulation.Example.Common.Interfaces.IEmulatorInputMapper", throwOnError: true)!;
        var map = mapperContract.GetMethod("MapInput")!;
        var backends = assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && mapperContract.IsAssignableFrom(type)).ToArray();
        Assert.Equal(3, backends.Length);
        var models = GWGUI.Emulation.Example.Common.Machines.Common.Dictionaries.HardwareModelCatalog.All;
        Assert.Equal(2, models.Count);
        var source = EmulationControllerState.Empty with { DeviceId = "pad", Controls = new EmulationControllerControls(new Dictionary<string, float> { ["Axis0"] = .875f, ["Axis1"] = .125f }) };
        var snapshot = EmulationInputSnapshot.Empty with { Keys = new HashSet<EmulationKey> { EmulationKey.A }, Controllers = [source] };
        var mappings = new Dictionary<string, string> { ["LeftStickRight"] = "Controller:pad:Axis0Positive", ["LeftStickUp"] = "Controller:pad:Axis1Negative", ["A"] = "Keyboard:A", ["B"] = "Keyboard:A", ["Reload"] = "Keyboard:A", ["L2"] = "Controller:pad:Axis0Positive" };
        foreach (var backend in backends)
        foreach (var model in models)
        foreach (var category in new[] { PeripheralCategory.LightGun, PeripheralCategory.Pointer, PeripheralCategory.DrivingController })
        {
            var adapter = Activator.CreateInstance(backend, nonPublic: true)!;
            var configuration = new MachineConfiguration(model.Model, input: new InputConfiguration(Controllers: [new ControllerBinding(0, category, "pad", mappings)]));
            var result = Assert.IsType<EmulationInputSnapshot>(map.Invoke(adapter, [snapshot, configuration]));
            var device = ResolveControllerDevice(category);
            var xId = category == PeripheralCategory.LightGun ? 13u : 0u;
            var yId = category == PeripheralCategory.LightGun ? 14u : 1u;
            Assert.Equal((short)(.75f * short.MaxValue), ReadNativeInput(result, device, 0, xId));
            Assert.Equal((short)(-.75f * short.MaxValue), ReadNativeInput(result, device, 0, yId));
            if (category == PeripheralCategory.DrivingController)
                Assert.Equal((short)24575, ReadNativeInput(result, device, 2, 12));
            else
                Assert.Equal((short)1, ReadNativeInput(result, device, 0, 2));
            var released = Assert.IsType<EmulationInputSnapshot>(map.Invoke(adapter, [EmulationInputSnapshot.Empty, configuration]));
            Assert.Equal((short)0, ReadNativeInput(released, device, category == PeripheralCategory.DrivingController ? 2u : 0u, category == PeripheralCategory.DrivingController ? 12u : 2u));
        }
    }

    private static readonly Type CommonFunctions = typeof(ExampleModuleFactory).Assembly.GetType("GWGUI.Emulation.Example.Common.Functions.ControllerFunctions", throwOnError: true)!;
    private static uint ResolveControllerDevice(PeripheralCategory category) => (uint)CommonFunctions.GetMethod("ResolveControllerDevice", BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, [category])!;
    private static short ReadNativeInput(EmulationInputSnapshot snapshot, uint device, uint index, uint id) => (short)CommonFunctions.GetMethod("ReadNativeInput", BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, [snapshot, 0u, device, index, id])!;

    private static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot, IReadOnlyDictionary<string, string> mappings)
    {
        var configuration = new InputConfiguration(Controllers: [new ControllerBinding(0, PeripheralCategory.RetroPad, Mappings: mappings)]);
        var functions = typeof(ExampleModuleFactory).Assembly.GetType("GWGUI.Emulation.Example.Common.Machines.Common.Functions.InputSnapshotFunctions", throwOnError: true)!;
        return (EmulationInputSnapshot)functions.GetMethod("Apply", BindingFlags.Static | BindingFlags.NonPublic, [typeof(EmulationInputSnapshot), typeof(InputConfiguration)])!.Invoke(null, [snapshot, configuration])!;
    }
}
