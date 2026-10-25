using System.Reflection;
using GWGUI.Emulation;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Example.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Example.Common.Machines.Common.Enums;
using GWGUI.Emulation.Example.Modules;

namespace GWGUI.Tests.Emulation;

public sealed class ExampleKeyboardTests
{
    [Fact]
    public void PortKeyboardDescribesTheSameKeysAsTheMainKeyboard()
    {
        var functions = Functions("InputSettingsFunctions");
        var main = (IReadOnlyList<InputBindingDefinition>)functions.GetMethod("KeyboardDefinitions", BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, [null])!;
        var port = (IReadOnlyList<InputBindingDefinition>)functions.GetMethod("ControllerDefinitions", BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, [PeripheralCategory.Keyboard])!;
        Assert.Same(main, port);
        Assert.Equal(EmulationSdk.InputProcessing.CreateKeyMap().Keys.OrderBy(key => key).Select(key => key.ToString()), port.Select(key => key.Id));
        Assert.DoesNotContain(port, key => key.Id == nameof(EmulationKey.Unknown));
    }

    [Fact]
    public void PortKeyboardMapsPhysicalSourcesToKeysWithoutLeakingControllerButtons()
    {
        var source = EmulationControllerState.Empty with { DeviceId = "pad", Controls = new EmulationControllerControls(new Dictionary<string, float> { ["Button0"] = 1f }) };
        var snapshot = EmulationInputSnapshot.Empty with { Keys = new HashSet<EmulationKey> { EmulationKey.C }, Controllers = [source] };
        var mappings = new Dictionary<string, string> { ["A"] = "Keyboard:C", ["B"] = "Controller:pad:Button0", ["Unknown"] = "Keyboard:C" };
        var configuration = new InputConfiguration(Controllers: [new ControllerBinding(0, PeripheralCategory.Keyboard, "pad", mappings)]);

        var result = Apply(snapshot, configuration);

        Assert.Equal(new HashSet<EmulationKey> { EmulationKey.A, EmulationKey.B, EmulationKey.C }, result.Keys);
        Assert.Equal(EmulationControllerState.Empty, Assert.Single(result.Controllers));
        Assert.Equal(new HashSet<EmulationKey> { EmulationKey.C }, snapshot.Keys);
        Assert.Same(source, Assert.Single(snapshot.Controllers));
    }

    [Theory]
    [InlineData(PeripheralCategory.None)]
    [InlineData(PeripheralCategory.Keyboard)]
    public void EmptyOrDisabledPortDoesNotAddKeys(PeripheralCategory peripheral)
    {
        var snapshot = EmulationInputSnapshot.Empty with { Keys = new HashSet<EmulationKey> { EmulationKey.C } };
        var configuration = new InputConfiguration(Controllers: [new ControllerBinding(0, peripheral)]);
        var result = Apply(snapshot, configuration);
        Assert.Equal(snapshot.Keys, result.Keys);
        Assert.Equal(EmulationControllerState.Empty, result.Controllers[0]);
    }

    private static Type Functions(string name) => typeof(ExampleModuleFactory).Assembly.GetType("GWGUI.Emulation.Example.Common.Machines.Common.Functions." + name, throwOnError: true)!;

    private static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot, InputConfiguration configuration) => (EmulationInputSnapshot)Functions("InputSnapshotFunctions")
        .GetMethod("Apply", BindingFlags.Static | BindingFlags.NonPublic, [typeof(EmulationInputSnapshot), typeof(InputConfiguration)])!.Invoke(null, [snapshot, configuration])!;
}
