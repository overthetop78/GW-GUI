using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Nintendo.Common.Machines.Common.Constants;
using GWGUI.Emulation.Nintendo.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Nintendo.Common.Machines.Common.Enums;
using GWGUI.Emulation.Nintendo.Common.Machines.Common.Functions;

namespace GWGUI.Tests.Emulation.Nintendo;

public sealed class Nintendo64ControllerTests
{
    private const int FirstPortIndex = 0;
    private const uint ExpectedButtonMask = (1u << 0) | (1u << 1) | (1u << 3)
        | (1u << 4) | (1u << 10) | (1u << 11) | (1u << 12);
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
    public void ProfilePublishesFourPortsAndEveryVisibleCommand()
    {
        var settings = InputSettingsFunctions.Describe(
            new MachineConfiguration(ModelConstants.Nintendo64, string.Empty));

        Assert.Equal(ModelConstants.Nintendo64ControllerPortCount,
            settings.ControllerPorts.Count);
        var choice = Assert.Single(settings.ControllerPorts[FirstPortIndex]
            .ControllerChoices, item => item.Id == ControllerType.Joystick.ToString());
        Assert.Equal(EmulationControllerVisualIds.Nintendo64Pad, choice.DefaultVisualId);
        var ids = choice.BindingDefinitions!.Select(item => item.Id).ToHashSet();
        Assert.Contains(InputSettingsFunctionsConstants.Nintendo64A, ids);
        Assert.Contains(InputSettingsFunctionsConstants.Nintendo64B, ids);
        Assert.Contains(InputSettingsFunctionsConstants.Nintendo64CUp, ids);
        Assert.Contains(InputSettingsFunctionsConstants.Nintendo64CDown, ids);
        Assert.Contains(InputSettingsFunctionsConstants.Nintendo64CLeft, ids);
        Assert.Contains(InputSettingsFunctionsConstants.Nintendo64CRight, ids);
        Assert.Contains(InputSettingsFunctionsConstants.Nintendo64StickUp, ids);
        Assert.Contains(InputSettingsFunctionsConstants.Nintendo64StickDown, ids);
        Assert.Contains(InputSettingsFunctionsConstants.Nintendo64StickLeft, ids);
        Assert.Contains(InputSettingsFunctionsConstants.Nintendo64StickRight, ids);
        Assert.Contains(EmulationControllerCommandIds.L2, ids);
        Assert.All(choice.VisualCommandIds!, item => Assert.Contains(item.Value, ids));
    }

    [Fact]
    public void KeyboardBindingsReachMupenButtonsAndBothAnalogAxes()
    {
        var bindings = new Dictionary<string, string>
        {
            [InputSettingsFunctionsConstants.Nintendo64A] = KeyboardA,
            [InputSettingsFunctionsConstants.Nintendo64B] = KeyboardB,
            [InputSettingsFunctionsConstants.Nintendo64CRight] = KeyboardC,
            [InputSettingsFunctionsConstants.Nintendo64CUp] = KeyboardD,
            [InputSettingsFunctionsConstants.Nintendo64StickLeft] = KeyboardE,
            [InputSettingsFunctionsConstants.Nintendo64StickUp] = KeyboardF,
            [EmulationControllerCommandIds.L] = KeyboardG,
            [EmulationControllerCommandIds.R] = KeyboardH,
            [EmulationControllerCommandIds.L2] = KeyboardI,
            [EmulationControllerCommandIds.Start] = KeyboardJ,
            [EmulationControllerCommandIds.Up] = KeyboardK
        };
        var configuration = new InputConfiguration(ControllerBindings:
        [
            new ControllerBinding(FirstPortIndex, ControllerType.Joystick,
                ButtonMappings: bindings)
        ]);
        var snapshot = EmulationInputSnapshot.Empty with
        {
            Keys = new HashSet<EmulationKey>
            {
                EmulationKey.A, EmulationKey.B, EmulationKey.C, EmulationKey.D,
                EmulationKey.E, EmulationKey.F, EmulationKey.G, EmulationKey.H,
                EmulationKey.I, EmulationKey.J, EmulationKey.K
            }
        };

        var mapped = InputSnapshotFunctions.Apply(snapshot, ModelConstants.Nintendo64,
            configuration, false);
        var controller = mapped.Controllers[FirstPortIndex];

        Assert.Equal(ExpectedButtonMask, controller.Buttons);
        Assert.Equal(short.MinValue, controller.LeftX);
        Assert.Equal(short.MinValue, controller.LeftY);
        Assert.Equal(short.MinValue, controller.RightX);
        Assert.Equal(short.MinValue, controller.RightY);
    }

    [Fact]
    public void OppositeDirectionsCancelWithoutSuppressingUnmappedPhysicalAxes()
    {
        var source = EmulationControllerState.Empty with
        {
            RightX = short.MaxValue,
            LeftY = short.MaxValue
        };
        var snapshot = EmulationInputSnapshot.Empty with
        {
            Keys = new HashSet<EmulationKey> { EmulationKey.A, EmulationKey.B },
            Controllers = [source]
        };
        var configuration = new InputConfiguration(ControllerBindings:
        [
            new ControllerBinding(FirstPortIndex, ControllerType.Joystick,
                ButtonMappings: new Dictionary<string, string>
                {
                    [InputSettingsFunctionsConstants.Nintendo64StickLeft] = KeyboardA,
                    [InputSettingsFunctionsConstants.Nintendo64StickRight] = KeyboardB
                })
        ]);

        var mapped = InputSnapshotFunctions.Apply(snapshot, ModelConstants.Nintendo64,
            configuration, false);

        Assert.Equal(default, mapped.Controllers[FirstPortIndex].LeftX);
        Assert.Equal(short.MaxValue, mapped.Controllers[FirstPortIndex].LeftY);
        Assert.Equal(short.MaxValue, mapped.Controllers[FirstPortIndex].RightX);
    }
}
