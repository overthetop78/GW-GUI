using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Sony.Common.Machines.Common.Constants;
using GWGUI.Emulation.Sony.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Sony.Common.Machines.Common.Enums;
using GWGUI.Emulation.Sony.Common.Machines.Common.Functions;

namespace GWGUI.Tests.Emulation.Sony;

public sealed class PspControllerTests
{
    private const int IntegratedPortIndex = 0;
    private const uint ExpectedButtonMask = (1u << 0) | (1u << 1) | (1u << 2) | (1u << 3) | (1u << 4) | (1u << 8) | (1u << 9) | (1u << 10) | (1u << 11);
    private const string KeyboardA = "Keyboard:A";
    private const string KeyboardB = "Keyboard:B";
    private const string KeyboardC = "Keyboard:C";
    private const string KeyboardD = "Keyboard:D";
    private const string KeyboardE = "Keyboard:E";
    private const string KeyboardF = "Keyboard:F";
    private const string KeyboardG = "Keyboard:G";
    private const string KeyboardH = "Keyboard:H";
    private const string KeyboardI = "Keyboard:I";
    private const string KeyboardJ = "Keyboard:J";
    private const string KeyboardK = "Keyboard:K";

    [Fact]
    public void AssignedCommandDoesNotInheritOtherPhysicalNamedButtons()
    {
        var source = EmulationControllerState.Empty with
        {
            Controls = new EmulationControllerControls(new Dictionary<string, float>
            {
                [EmulationControllerCommandIds.X] = 1,
                [EmulationControllerCommandIds.L2] = 0.75f
            })
        };
        var snapshot = EmulationInputSnapshot.Empty with
        {
            Keys = new HashSet<EmulationKey> { EmulationKey.A },
            Controllers = [source]
        };
        var configuration = new InputConfiguration(ControllerBindings:
        [
            new ControllerBinding(IntegratedPortIndex, ControllerType.Joystick,
                ButtonMappings: new Dictionary<string, string> { [EmulationControllerCommandIds.B] = KeyboardA })
        ]);
        var controls = InputSnapshotFunctions.Apply(snapshot, configuration, false).Controllers[IntegratedPortIndex].Controls;
        Assert.Single(controls);
        Assert.Equal(1f, controls[EmulationControllerCommandIds.B]);
        Assert.False(controls.ContainsKey(EmulationControllerCommandIds.X));
        Assert.False(controls.ContainsKey(EmulationControllerCommandIds.L2));
    }

    [Theory]
    [InlineData(.625f, 8192)]
    [InlineData(.375f, -8192)]
    [InlineData(.5f, 0)]
    public void AssignedPhysicalAxisKeepsIntermediateVirtualStickValues(float position, int expected)
    {
        var source = EmulationControllerState.Empty with
        {
            DeviceId = "pad",
            Controls = new(new Dictionary<string, float> { ["Axis0"] = position })
        };
        var snapshot = EmulationInputSnapshot.Empty with { Controllers = [source] };
        var configuration = new InputConfiguration(ControllerBindings:
        [
            new(IntegratedPortIndex, ControllerType.Joystick, "pad", new Dictionary<string, string>
            {
                [InputSettingsFunctionsConstants.PspStickRight] = "Controller:pad:Axis0Positive",
                [InputSettingsFunctionsConstants.PspStickLeft] = "Controller:pad:Axis0Negative"
            })
        ]);
        Assert.Equal(expected, InputSnapshotFunctions.Apply(snapshot, configuration, false).Controllers[IntegratedPortIndex].LeftX);
    }

    [Fact]
    public void ProfilePublishesOneIntegratedPortAndEveryVisibleCommand()
    {
        var settings = InputSettingsFunctions.Describe(new MachineConfiguration(ModelConstants.Psp, GWGUI.Emulation.Sony.Emulators.Ppsspp.Constants.PpssppConstants.Id));

        var port = Assert.Single(settings.ControllerPorts);
        var choice = Assert.Single(port.ControllerChoices, item => item.Id == ControllerType.Joystick.ToString());
        Assert.Equal(EmulationControllerVisualIds.SonyPsp1000, choice.DefaultVisualId);
        var ids = choice.BindingDefinitions!.Select(item => item.Id).ToHashSet();
        Assert.All(choice.BindingDefinitions!.Where(item => item.OppositeDirectionId is not null), item =>
            Assert.Equal(item.Id, choice.BindingDefinitions!.Single(other => other.Id == item.OppositeDirectionId).OppositeDirectionId));
        Assert.Contains(InputSettingsFunctionsConstants.PspStickUp, ids);
        Assert.Contains(InputSettingsFunctionsConstants.PspStickDown, ids);
        Assert.Contains(InputSettingsFunctionsConstants.PspStickLeft, ids);
        Assert.Contains(InputSettingsFunctionsConstants.PspStickRight, ids);
        Assert.Contains(EmulationControllerCommandIds.X, ids);
        Assert.Contains(EmulationControllerCommandIds.Y, ids);
        Assert.Contains(EmulationControllerCommandIds.L, ids);
        Assert.Contains(EmulationControllerCommandIds.R, ids);
        Assert.DoesNotContain(EmulationControllerCommandIds.L2, ids);
        Assert.All(choice.VisualCommandIds!, item => Assert.Contains(item.Value, ids));
    }

    [Fact]
    public void KeyboardBindingsReachPpssppButtonsAndAnalogStick()
    {
        var bindings = new Dictionary<string, string>
        {
            [EmulationControllerCommandIds.B] = KeyboardA,
            [EmulationControllerCommandIds.Y] = KeyboardB,
            [EmulationControllerCommandIds.Select] = KeyboardC,
            [EmulationControllerCommandIds.Start] = KeyboardD,
            [EmulationControllerCommandIds.Up] = KeyboardE,
            [EmulationControllerCommandIds.A] = KeyboardF,
            [EmulationControllerCommandIds.X] = KeyboardG,
            [EmulationControllerCommandIds.L] = KeyboardH,
            [EmulationControllerCommandIds.R] = KeyboardI,
            [InputSettingsFunctionsConstants.PspStickLeft] = KeyboardJ,
            [InputSettingsFunctionsConstants.PspStickUp] = KeyboardK
        };
        var configuration = new InputConfiguration(ControllerBindings: [new ControllerBinding(IntegratedPortIndex, ControllerType.Joystick, ButtonMappings: bindings)]);
        var snapshot = EmulationInputSnapshot.Empty with
        {
            Keys = new HashSet<EmulationKey>
            {
                EmulationKey.A, EmulationKey.B, EmulationKey.C, EmulationKey.D,
                EmulationKey.E, EmulationKey.F, EmulationKey.G, EmulationKey.H,
                EmulationKey.I, EmulationKey.J, EmulationKey.K
            }
        };

        var mapped = InputSnapshotFunctions.Apply(snapshot, configuration, false);
        var controller = mapped.Controllers[IntegratedPortIndex];

        Assert.Equal(ExpectedButtonMask, controller.Buttons);
        Assert.Equal(short.MinValue, controller.LeftX);
        Assert.Equal(short.MinValue, controller.LeftY);
    }

    [Fact]
    public void OppositeStickDirectionsCancelWithoutSuppressingUnmappedPhysicalAxis()
    {
        var source = EmulationControllerState.Empty with { LeftY = short.MaxValue };
        var snapshot = EmulationInputSnapshot.Empty with
        {
            Keys = new HashSet<EmulationKey> { EmulationKey.A, EmulationKey.B },
            Controllers = [source]
        };
        var configuration = new InputConfiguration(ControllerBindings:
        [
            new ControllerBinding(IntegratedPortIndex, ControllerType.Joystick,
                ButtonMappings: new Dictionary<string, string>
                {
                    [InputSettingsFunctionsConstants.PspStickLeft] = KeyboardA,
                    [InputSettingsFunctionsConstants.PspStickRight] = KeyboardB
                })
        ]);

        var mapped = InputSnapshotFunctions.Apply(snapshot, configuration, false);

        Assert.Equal(default, mapped.Controllers[IntegratedPortIndex].LeftX);
        Assert.Equal(short.MaxValue, mapped.Controllers[IntegratedPortIndex].LeftY);
    }
}
