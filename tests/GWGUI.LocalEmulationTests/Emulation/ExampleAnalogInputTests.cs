using System.Reflection;
using System.Net.Http;
using System.Text.Json;
using GWGUI.Emulation;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Example.Common.Enums;
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
        var functions = typeof(ExampleModuleFactory).Assembly.GetType("GWGUI.Emulation.Example.Common.Functions.ControllerFunctions", true)!;
        var fromCommon = functions.GetMethod("FromCommon", BindingFlags.Static | BindingFlags.NonPublic)!.CreateDelegate<Func<EmulationPeripheralCategory, MachineConfiguration, int, PeripheralCategory>>();
        foreach (var model in GWGUI.Emulation.Example.Common.Machines.Common.Dictionaries.HardwareModelCatalog.All)
        foreach (var emulator in Enum.GetValues<Emulator>())
        {
            var configuration = new MachineConfiguration(model.Model, core: emulator);
            if (emulator != Emulator.None)
            {
                var adapter = ExampleFirmwareSamplesTests.Adapter(emulator);
                var advertised = (IReadOnlyList<PeripheralCategory>)adapter.GetType().GetMethod("GetPeripherals", [typeof(MachineConfiguration)])!.Invoke(adapter, [configuration])!;
                var settingsFunctions = typeof(ExampleModuleFactory).Assembly.GetType("GWGUI.Emulation.Example.Common.Machines.Common.Functions.InputSettingsFunctions", true)!;
                var settings = (EmulationInputSettings)settingsFunctions.GetMethod("Describe", BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, [configuration])!;
                var offered = settings.ControllerPorts.SelectMany(port => port.ControllerChoices).Select(choice => Enum.Parse<PeripheralCategory>(choice.Id)).Distinct().ToArray();
                Assert.Equal(offered, advertised);
                Assert.Equal(expected.Keys.Where(category => category is not (PeripheralCategory.Automatic or PeripheralCategory.Joystick)).Order(), advertised.Order());
                Assert.DoesNotContain(PeripheralCategory.Automatic, advertised);
                Assert.DoesNotContain(PeripheralCategory.Joystick, advertised);
                var usbPorts = (IReadOnlyList<int>)adapter.GetType().GetMethod("GetUsbControllerPorts")!.Invoke(adapter, [configuration])!;
                Assert.Equal(0, Assert.Single(usbPorts));
                Assert.True(Assert.Single(settings.ControllerPorts, port => port.Number == 1).IsUsb);
                Assert.Contains(settings.ControllerPorts, port => !port.IsUsb);
                foreach (var port in settings.ControllerPorts)
                {
                    var joystick = Assert.Single(port.ControllerChoices, choice => choice.Category == EmulationControllerCategory.Joystick && choice.Id != nameof(PeripheralCategory.Paddle));
                    Assert.Equal(nameof(PeripheralCategory.AnalogJoystick), joystick.Id);
                    var commands = typeof(EmulationControllerCommandIds).GetFields(BindingFlags.Static | BindingFlags.Public).Where(field => field.IsLiteral && field.FieldType == typeof(string)).Select(field => (string)field.GetRawConstantValue()!).Distinct().Order().ToArray();
                    Assert.Equal(commands, joystick.BindingDefinitions!.Select(definition => definition.Id).Order());
                }
            }
            foreach (var category in expected.Keys)
            {
                Assert.True(Enum.TryParse<EmulationPeripheralCategory>(category.ToString(), out var publicCategory), category.ToString());
                Assert.Equal(category, fromCommon(publicCategory, configuration, 0));
            }
            Assert.Throws<ArgumentOutOfRangeException>(() => fromCommon((EmulationPeripheralCategory)int.MaxValue, configuration, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => fromCommon(EmulationPeripheralCategory.Pointer, configuration, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => fromCommon(EmulationPeripheralCategory.DanceMat, configuration, 2));
        }
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

    [Fact]
    public void ControllerExtensionKeepsItsSettingsAndHiddenBindingsForEveryProfileAndAdapter()
    {
        using var http = new HttpClient();
        var module = new ExampleModuleFactory().Create(new EmulationModuleContext(AppContext.BaseDirectory, AppContext.BaseDirectory, http));
        var inputs = Assert.IsAssignableFrom<IEmulationInputSettingsManager>(module);
        const string key = "configuration.parallelJoystickAdapter";
        foreach (var model in GWGUI.Emulation.Example.Common.Machines.Common.Dictionaries.HardwareModelCatalog.All)
        foreach (var emulator in Enum.GetValues<Emulator>())
        {
            var initial = new MachineConfiguration(model.Model, core: emulator);
            var field = Assert.Single(module.Describe(initial.MachineId, initial).Blocks.SelectMany(block => block.Fields), field => field.Id == key);
            Assert.Equal(bool.FalseString, field.Value);
            Assert.True(field.RefreshSettingsOnChange);
            Assert.True(field.RequiresRestart);
            Assert.True(field.IsGlobalControllerOption);
            Assert.Equal(2, inputs.DescribeInputSettings(initial).ControllerPorts.Count);
            var enabled = Assert.IsType<MachineConfiguration>(module.ApplySettings(initial, new Dictionary<string, string?> { [key] = bool.TrueString }));
            Assert.False(initial.Input.ParallelJoystickAdapterEnabled);
            Assert.True(enabled.Input.ParallelJoystickAdapterEnabled);
            Assert.DoesNotContain(key, enabled.Options.Keys);
            var settings = inputs.DescribeInputSettings(enabled);
            Assert.Equal(4, settings.ControllerPorts.Count);
            var ports = settings.ControllerPorts.Select(port => port.Number == 4 ? port with { SelectedControllerId = "Pointer", PhysicalDeviceId = "extension-device", DeadZonePercent = 17 } : port).ToArray();
            var configured = Assert.IsType<MachineConfiguration>(inputs.ApplyInputSettings(enabled, settings with { ControllerPorts = ports }));
            var restored = JsonSerializer.Deserialize<InputConfiguration>(JsonSerializer.Serialize(configured.Input))!;
            Assert.True(restored.ParallelJoystickAdapterEnabled);
            Assert.Equal(PeripheralCategory.Pointer, Assert.Single(restored.Controllers!, binding => binding.Port == 3).Peripheral);
            var disabled = Assert.IsType<MachineConfiguration>(module.ApplySettings(configured, new Dictionary<string, string?> { [key] = bool.FalseString }));
            Assert.Equal(2, inputs.DescribeInputSettings(disabled).ControllerPorts.Count);
            Assert.Equal(configured.Input.Controllers, disabled.Input.Controllers);
            var reenabled = module.ApplySettings(disabled, new Dictionary<string, string?> { [key] = bool.TrueString });
            var last = inputs.DescribeInputSettings(reenabled).ControllerPorts.Last();
            Assert.Equal("Pointer", last.SelectedControllerId);
            Assert.Equal("extension-device", last.PhysicalDeviceId);
            Assert.Equal(17, last.DeadZonePercent);
            Assert.Throws<ArgumentException>(() => module.ApplySettings(initial, new Dictionary<string, string?> { [key] = "invalid" }));
            var invalid = initial with { Input = initial.Input with { Controllers = [new ControllerBinding(4, PeripheralCategory.RetroPad)] } };
            Assert.Throws<ArgumentException>(() => module.ApplySettings(invalid, new Dictionary<string, string?>()));
        }
    }

    [Fact]
    public void BothProfilesAndAllThreeMockModesPublishAndReleaseControllerFeedback()
    {
        var type = typeof(ExampleModuleFactory).Assembly.GetType("GWGUI.Emulation.Example.Emulators.Common.Interop.Services.ExternalCore", throwOnError: true)!;
        foreach (var model in GWGUI.Emulation.Example.Common.Machines.Common.Dictionaries.HardwareModelCatalog.All)
        foreach (var emulator in Enum.GetValues<Emulator>().Where(value => value != Emulator.None))
        foreach (var extension in new[] { false, true })
        {
            var configuration = new MachineConfiguration(model.Model, core: emulator, input: new InputConfiguration(ParallelJoystickAdapterEnabled: extension));
            var portCount = extension ? 4 : 2;
            var instance = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, [typeof(string), typeof(Emulator)])!.Invoke(["mock-library-identity", emulator]);
            IEmulationControllerFeedbackAccumulator? feedback = null;
            var feedbackTransferred = false;
            try
            {
                var core = Assert.IsAssignableFrom<IEmulationCoreRuntime<MachineConfiguration, EmulationOption>>(instance);
                // Injecter uniquement l’état géré évite le parcours d’initialisation qui consulte les chemins de firmware.
                // Le cœur reçoit la propriété du stockage et exécute ensuite ses vraies méthodes de frames et de commandes.
                type.GetField("_configuration", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(instance, configuration);
                type.GetField("_memory", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(instance, new byte[1]);
                feedback = EmulationSdk.EnvironmentProcessing.CreateControllerFeedbackAccumulator(portCount);
                type.GetField("_feedback", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(instance, feedback);
                feedbackTransferred = true;
                type.GetField("_initialized", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(instance, true);
                var commands = Assert.IsAssignableFrom<IEmulationCoreCommands<MediaConfiguration, PeripheralCategory>>(instance);
                var lastPort = portCount - 1;
                commands.SetControllerPortDevice(lastPort, PeripheralCategory.Pointer);
                commands.SetControllerPortDevice(lastPort, PeripheralCategory.RetroPad);
                Assert.Throws<ArgumentOutOfRangeException>(() => commands.SetControllerPortDevice(portCount, PeripheralCategory.RetroPad));
                core.SetInput(EmulationInputSnapshot.Empty with { Controllers = Enumerable.Range(0, portCount).Select(port => port == lastPort ? EmulationControllerState.Empty with { LeftTrigger = short.MaxValue } : EmulationControllerState.Empty).ToArray() });
                core.RunFrame();
                Assert.Equal(lastPort, Assert.Single(core.ControllerFeedback).Port);
                commands.SetControllerPortDevice(lastPort, PeripheralCategory.None);
                Assert.Empty(core.ControllerFeedback);
                commands.SetControllerPortDevice(lastPort, PeripheralCategory.RetroPad);
                var input = EmulationInputSnapshot.Empty with
                {
                    Controllers = [EmulationControllerState.Empty with { LeftTrigger = short.MaxValue, RightTrigger = short.MaxValue / 2 },
                        EmulationControllerState.Empty with { LeftTrigger = short.MinValue, RightTrigger = short.MaxValue }]
                };
                core.SetInput(input);
                core.RunFrame();
                var captured = core.ControllerFeedback;
                Assert.Equal(3, captured.Count);
                Assert.Equal(1f, Assert.Single(captured, item => item.Port == 0 && item.CommandId == EmulationControllerFeedbackCommands.Strong).Strength);
                Assert.InRange(Assert.Single(captured, item => item.Port == 0 && item.CommandId == EmulationControllerFeedbackCommands.Weak).Strength, .4999f, .5001f);
                Assert.Equal(1f, Assert.Single(captured, item => item.Port == 1).Strength);

                commands.SetControllerPortDevice(0, PeripheralCategory.None);
                Assert.DoesNotContain(core.ControllerFeedback, item => item.Port == 0);
                core.RunFrame();
                Assert.DoesNotContain(core.ControllerFeedback, item => item.Port == 0);
                commands.SetControllerPortDevice(1, PeripheralCategory.Keyboard);
                core.RunFrame();
                Assert.Empty(core.ControllerFeedback);
                commands.SetControllerPortDevice(0, PeripheralCategory.RetroPad);
                commands.SetControllerPortDevice(1, PeripheralCategory.RetroPad);
                core.RunFrame();
                Assert.Equal(3, core.ControllerFeedback.Count);
                core.SetInput(EmulationInputSnapshot.Empty);
                core.RunFrame();
                Assert.Empty(core.ControllerFeedback);
                Assert.Equal(3, captured.Count);
                core.SetInput(input);
                core.RunFrame();
                commands.SoftReset();
                Assert.Empty(core.ControllerFeedback);
                core.RunFrame();
                Assert.Equal(3, core.ControllerFeedback.Count);
                commands.Stop();
                Assert.Empty(core.ControllerFeedback);
            }
            finally
            {
                // Les tableaux vidéo et le stockage de feedback appartiennent au cœur, même après un échec du test.
                try { (instance as IDisposable)?.Dispose(); }
                finally { if (!feedbackTransferred) feedback?.Dispose(); }
            }
            Assert.NotNull(feedback);
            Assert.Empty(feedback.Snapshot());
            Assert.False(feedback.SetState(0, 0, ushort.MaxValue));
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
