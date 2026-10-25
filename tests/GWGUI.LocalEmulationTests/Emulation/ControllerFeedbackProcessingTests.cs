using GWGUI.Emulation;
using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;

namespace GWGUI.Tests.Emulation;

public sealed class ControllerFeedbackProcessingTests
{
    private enum Peripheral { None, Keyboard, Gamepad, Joystick }

    private static EmulationControllerPort Port(int number, IReadOnlyDictionary<string, EmulationControllerOutputBinding> outputs)
        => new(number, [], "Gamepad", null, new([], new Dictionary<string, string>(), EmulationInputSource.Controller)) { OutputBindings = outputs };

    [Fact]
    public void StandardDefinitionsAreSharedAndExcludeInvalidDisabledAndKeyboardCategories()
    {
        var service = EmulationSdk.ControllerFeedback;
        Assert.False(service.GetType().IsVisible);
        Assert.Empty(service.DescribeOutputs(Peripheral.None));
        Assert.Empty(service.DescribeOutputs(Peripheral.Keyboard));
        Assert.Empty(service.DescribeOutputs((Peripheral)(-1)));
        var definitions = service.DescribeOutputs(Peripheral.Gamepad);
        Assert.Same(definitions, service.DescribeOutputs(Peripheral.Joystick));
        Assert.Equal(new[] { EmulationControllerFeedbackCommands.Strong, EmulationControllerFeedbackCommands.Weak }, definitions.Select(item => item.Id));
        Assert.Throws<NotSupportedException>(() => ((IList<EmulationControllerOutputDefinition>)definitions).Clear());
    }

    [Fact]
    public void BindingsUseTechnicalPortIndexesAndOnlyDeclaredCommandsAndValidTargets()
    {
        var service = EmulationSdk.ControllerFeedback;
        var outputs = new Dictionary<string, EmulationControllerOutputBinding>
        {
            [EmulationControllerFeedbackCommands.Strong] = new("device-a", EmulationControllerMotor.LowFrequency),
            [EmulationControllerFeedbackCommands.Weak] = new("device-b", EmulationControllerMotor.HighFrequency),
            ["Unknown"] = new("device-c", EmulationControllerMotor.LowFrequency)
        };
        var calls = 0;
        var bindings = service.CreateBindings([Port(0, outputs), Port(1, outputs), Port(10, outputs), Port(11, outputs)], port =>
        {
            calls++;
            return service.DescribeOutputs(Peripheral.Gamepad).Where(item => item.Id == EmulationControllerFeedbackCommands.Strong).ToArray();
        });
        Assert.Equal(2, calls);
        Assert.Equal(new[] { 0, 9 }, bindings.Keys.Select(key => key.Port).Order());
        Assert.All(bindings, item => Assert.Equal("device-a", item.Value.DeviceId));
        outputs.Clear();
        Assert.Equal(2, bindings.Count);
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("  ", 0)]
    [InlineData("device", -1)]
    [InlineData("device", 99)]
    public void InvalidPhysicalTargetsAreIgnored(string? device, int motor)
    {
        var outputs = new Dictionary<string, EmulationControllerOutputBinding> { [EmulationControllerFeedbackCommands.Strong] = new(device!, (EmulationControllerMotor)motor) };
        Assert.Empty(EmulationSdk.ControllerFeedback.CreateBindings([Port(1, outputs)], _ => EmulationSdk.ControllerFeedback.DescribeOutputs(Peripheral.Gamepad)));
    }

    [Fact]
    public void ProjectionKeepsExplicitStopsAndIgnoresUnknownPortsCommandsAndInvalidStrengths()
    {
        var service = EmulationSdk.ControllerFeedback;
        var outputs = new Dictionary<string, EmulationControllerOutputBinding>
        {
            [EmulationControllerFeedbackCommands.Strong] = new("device-a", EmulationControllerMotor.LowFrequency),
            [EmulationControllerFeedbackCommands.Weak] = new("device-b", EmulationControllerMotor.HighFrequency)
        };
        var bindings = service.CreateBindings([Port(1, outputs)], _ => service.DescribeOutputs(Peripheral.Gamepad));
        ControllerFeedbackState[] states =
        [
            new(0, EmulationControllerFeedbackCommands.Strong, .25f), new(0, EmulationControllerFeedbackCommands.Weak, 0f),
            new(1, EmulationControllerFeedbackCommands.Strong, 1f), new(0, "Unknown", 1f),
            new(0, EmulationControllerFeedbackCommands.Strong, float.NaN), new(0, EmulationControllerFeedbackCommands.Strong, float.PositiveInfinity),
            new(0, EmulationControllerFeedbackCommands.Strong, -1f), new(0, EmulationControllerFeedbackCommands.Strong, 2f)
        ];
        Assert.Equal(new[] { new EmulationControllerOutputState("device-a", EmulationControllerMotor.LowFrequency, .25f), new EmulationControllerOutputState("device-b", EmulationControllerMotor.HighFrequency, 0f) }, service.ResolveStates(states, bindings));
        Assert.Empty(service.ResolveStates([], bindings));
        Assert.Empty(service.ResolveStates(states, new Dictionary<(int, string), EmulationControllerOutputBinding>()));
        Assert.Equal(8, states.Length);
    }
}
