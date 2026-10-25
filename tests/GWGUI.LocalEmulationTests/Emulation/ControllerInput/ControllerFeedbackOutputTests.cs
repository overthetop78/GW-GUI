using GWGUI.App.Views.Controls.Emulation.Input;
using GWGUI.Emulation.Constants;
using GWGUI.Tests.Application.TestInfrastructure;
using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Controllers.Emulation.Machine;
using GWGUI.App.Services.Input.GameInput;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Interfaces;
using GWGUI.Tests.Interface.DeviceSettingsViews;

namespace GWGUI.Tests.Emulation.ControllerInput;

[Collection("WPF")]
public sealed class ControllerFeedbackOutputTests(StaExecutionScenarios sta)
{
    [Fact]
    public Task OutputEditorOffersOnlyAvailableMotorsAndPreservesAnExplicitDisconnectedTarget() => sta.Run(() =>
    {
        var source = new Source { Devices = [DeviceListScenarios.Device("one") with { RumbleMotors = GameInputRumbleMotors.LowFrequency }] };
        var editor = new EmulationControllerOutputEditor(source);
        EmulationControllerOutputDefinition[] definitions = [new(EmulationControllerFeedbackCommands.Strong, EmulationControllerFeedbackCommands.StrongResourceKey)];
        try
        {
            editor.SetDefinitions(definitions, null);
            var selector = Assert.Single(Assert.Single(editor.Children.OfType<Grid>()).Children.OfType<ComboBox>());
            Assert.Equal(2, selector.Items.Count);
            Assert.Equal(0, selector.SelectedIndex);
            Assert.Empty(editor.Values);
            selector.SelectedIndex = 1;
            var binding = Assert.Single(editor.Values).Value;
            Assert.Equal(new EmulationControllerOutputBinding("one", EmulationControllerMotor.LowFrequency), binding);
            source.Devices = [];
            editor.SetDefinitions(definitions, null);
            var disconnected = Assert.Single(Assert.Single(editor.Children.OfType<Grid>()).Children.OfType<ComboBox>());
            Assert.Equal(2, disconnected.Items.Count);
            Assert.Equal(binding, Assert.Single(editor.Values).Value);
            disconnected.SelectedIndex = 0;
            Assert.Empty(editor.Values);
            editor.SetDefinitions([], null);
            Assert.Equal(Visibility.Collapsed, editor.Visibility);
        }
        finally { editor.SetDefinitions([], null); editor.SetBindings(new Dictionary<string, EmulationControllerOutputBinding>()); source.Dispose(); }
    });

    [Fact]
    public void NoAssociationNeverTouchesAPhysicalDevice()
    {
        var source = new Source();
        using var controller = new MachineControllerFeedbackController(source, _ => { });
        controller.Update(new Feedback(), active: true);
        Assert.Empty(source.Calls);
        Assert.Equal(0, source.Enumerations);
    }

    [Fact]
    public void CompositionPreservesIndependentMotorsUsesTheMaximumAndSkipsUnchangedValues()
    {
        var source = new Source();
        var feedback = new Feedback
        {
            ControllerOutputs = [new("one", EmulationControllerMotor.LowFrequency, .25f), new("one", EmulationControllerMotor.LowFrequency, .75f),
                new("one", EmulationControllerMotor.HighFrequency, .5f), new("one", EmulationControllerMotor.LeftTrigger, .1f), new("one", EmulationControllerMotor.RightTrigger, .2f)]
        };
        using var controller = new MachineControllerFeedbackController(source, _ => { });
        controller.Update(feedback, active: true);
        Assert.Equal(("one", .75f, .5f, .1f, .2f), Assert.Single(source.Calls));
        controller.Update(feedback, active: true);
        Assert.Single(source.Calls);
        controller.Update(feedback, active: false);
        Assert.Equal(("one", 0f, 0f, 0f, 0f), source.Calls[^1]);
    }

    [Fact]
    public void UnsupportedDisconnectedAndInvalidOutputsRemainNeutral()
    {
        var source = new Source { Devices = [DeviceListScenarios.Device("one") with { RumbleMotors = GameInputRumbleMotors.LowFrequency }] };
        using var controller = new MachineControllerFeedbackController(source, _ => { });
        controller.Update(new Feedback { ControllerOutputs = [new("one", EmulationControllerMotor.HighFrequency, 1f), new("gone", EmulationControllerMotor.LowFrequency, 1f),
            new("one", EmulationControllerMotor.LowFrequency, float.NaN), new("one", EmulationControllerMotor.LowFrequency, 2f)] }, active: true);
        Assert.Empty(source.Calls);
    }

    [Fact]
    public void SnapshotFailureStopsEveryPreviouslyUsedDevice()
    {
        var source = new Source();
        using var controller = new MachineControllerFeedbackController(source, _ => { });
        controller.Update(TwoDevices(), active: true);
        controller.Update(new BrokenFeedback(), active: true);
        Assert.Contains(("one", 0f, 0f, 0f, 0f), source.Calls);
        Assert.Contains(("two", 0f, 0f, 0f, 0f), source.Calls);
    }

    [Fact]
    public void DisposalAttemptsEveryMotorEvenWhenOneDeviceThrows()
    {
        var source = new Source();
        var controller = new MachineControllerFeedbackController(source, _ => { });
        try
        {
            controller.Update(TwoDevices(), active: true);
            source.ThrowWhenStoppingOne = true;
        }
        finally { controller.Dispose(); }
        Assert.Contains(("one", 0f, 0f, 0f, 0f), source.Calls);
        Assert.Contains(("two", 0f, 0f, 0f, 0f), source.Calls);
        Assert.False(source.WasDisposed);
    }

    private static Feedback TwoDevices() => new() { ControllerOutputs = [new("one", EmulationControllerMotor.LowFrequency, 1f), new("two", EmulationControllerMotor.HighFrequency, .5f)] };
    private sealed class Feedback : IEmulationControllerFeedbackSource { public IReadOnlyList<EmulationControllerOutputState> ControllerOutputs { get; init; } = []; }
    private sealed class BrokenFeedback : IEmulationControllerFeedbackSource { public IReadOnlyList<EmulationControllerOutputState> ControllerOutputs => throw new InvalidOperationException("Snapshot failed."); }

    private sealed class Source : IGameInputControllerSource, IDisposable
    {
        internal IReadOnlyList<GameInputDeviceDescriptor> Devices = [DeviceListScenarios.Device("one"), DeviceListScenarios.Device("two")];
        internal readonly List<(string, float, float, float, float)> Calls = [];
        internal bool ThrowWhenStoppingOne;
        internal bool WasDisposed;
        internal int Enumerations;
        public IReadOnlyList<GameInputDeviceDescriptor> GetConnectedDevices() { Enumerations++; return Devices; }
        public GameInputLiveState ReadState(string deviceId) => throw new NotSupportedException();
        public void Refresh() { }
        public bool SetRumble(string id, float low, float high, float left, float right)
        {
            Calls.Add((id, low, high, left, right));
            if (ThrowWhenStoppingOne && id == "one" && low == 0) throw new InvalidOperationException("Disconnected while stopping.");
            return true;
        }
        public void Dispose() => WasDisposed = true;
    }
}
