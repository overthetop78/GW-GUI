using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Nintendo.Common.Machines.Common.Constants;
using GWGUI.Emulation.Nintendo.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Nintendo.Common.Machines.Common.Enums;
using GWGUI.Emulation.Nintendo.Common.Machines.Common.Functions;

namespace GWGUI.Tests.Emulation.Nintendo;

public sealed class GameCubeControllerTests
{
    private const int FirstPortIndex = 0;
    private const uint ExpectedButtons = (1u << 0) | (1u << 1) | (1u << 3)
        | (1u << 4) | (1u << 8) | (1u << 9) | (1u << 11)
        | (1u << 12) | (1u << 13) | (1u << 14) | (1u << 15);
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
    private const string KeyboardL = "Keyboard:L";
    private const string KeyboardM = "Keyboard:M";
    private const string KeyboardN = "Keyboard:N";
    private const string KeyboardO = "Keyboard:O";

    [Fact]
    public void ProfilePublishesFourPortsAndMappableCommands()
    {
        var settings = InputSettingsFunctions.Describe(
            new MachineConfiguration(ModelConstants.GameCube, string.Empty));

        Assert.Equal(ModelConstants.GameCubeControllerPortCount,
            settings.ControllerPorts.Count);
        var choice = Assert.Single(settings.ControllerPorts[FirstPortIndex]
            .ControllerChoices, item => item.Id == ControllerType.Joystick.ToString());
        Assert.Equal(EmulationControllerVisualIds.NintendoGameCubePad, choice.DefaultVisualId);
        var ids = choice.BindingDefinitions!.Select(item => item.Id).ToHashSet();
        Assert.Contains(InputSettingsFunctionsConstants.GameCubeZ, ids);
        Assert.Contains(InputSettingsFunctionsConstants.GameCubeL, ids);
        Assert.Contains(InputSettingsFunctionsConstants.GameCubeR, ids);
        Assert.Contains(InputSettingsFunctionsConstants.GameCubeLHalf, ids);
        Assert.Contains(InputSettingsFunctionsConstants.GameCubeRHalf, ids);
        Assert.Contains(InputSettingsFunctionsConstants.GameCubeStickUp, ids);
        Assert.Contains(InputSettingsFunctionsConstants.GameCubeCUp, ids);
        Assert.All(choice.VisualCommandIds!, item => Assert.Contains(item.Value, ids));
    }

    [Fact]
    public void KeyboardBindingsReachDolphinButtonsAndBothSticks()
    {
        var mappings = new Dictionary<string, string>
        {
            [EmulationControllerCommandIds.B] = KeyboardA,
            [EmulationControllerCommandIds.Y] = KeyboardB,
            [EmulationControllerCommandIds.Start] = KeyboardC,
            [EmulationControllerCommandIds.Up] = KeyboardD,
            [EmulationControllerCommandIds.A] = KeyboardE,
            [EmulationControllerCommandIds.X] = KeyboardF,
            [InputSettingsFunctionsConstants.GameCubeZ] = KeyboardG,
            [InputSettingsFunctionsConstants.GameCubeL] = KeyboardH,
            [InputSettingsFunctionsConstants.GameCubeR] = KeyboardI,
            [InputSettingsFunctionsConstants.GameCubeLHalf] = KeyboardJ,
            [InputSettingsFunctionsConstants.GameCubeRHalf] = KeyboardK,
            [InputSettingsFunctionsConstants.GameCubeStickRight] = KeyboardL,
            [InputSettingsFunctionsConstants.GameCubeStickUp] = KeyboardM,
            [InputSettingsFunctionsConstants.GameCubeCLeft] = KeyboardN,
            [InputSettingsFunctionsConstants.GameCubeCDown] = KeyboardO
        };
        var configuration = new InputConfiguration(ControllerBindings:
        [
            new ControllerBinding(FirstPortIndex, ControllerType.Joystick,
                ButtonMappings: mappings)
        ]);
        var snapshot = EmulationInputSnapshot.Empty with
        {
            Keys = new HashSet<EmulationKey>
            {
                EmulationKey.A, EmulationKey.B, EmulationKey.C, EmulationKey.D,
                EmulationKey.E, EmulationKey.F, EmulationKey.G, EmulationKey.H,
                EmulationKey.I, EmulationKey.J, EmulationKey.K, EmulationKey.L,
                EmulationKey.M, EmulationKey.N, EmulationKey.O
            }
        };

        var controller = InputSnapshotFunctions.Apply(snapshot, configuration, false)
            .Controllers[FirstPortIndex];

        Assert.Equal(ExpectedButtons, controller.Buttons);
        Assert.Equal(short.MaxValue, controller.LeftX);
        Assert.Equal(short.MinValue, controller.LeftY);
        Assert.Equal(short.MinValue, controller.RightX);
        Assert.Equal(short.MaxValue, controller.RightY);
    }

    [Fact]
    public void OppositeStickDirectionsCancelAndOtherPhysicalAxesRemain()
    {
        var source = EmulationControllerState.Empty with
        {
            RightY = short.MaxValue,
            LeftY = short.MinValue
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
                    [InputSettingsFunctionsConstants.GameCubeCLeft] = KeyboardA,
                    [InputSettingsFunctionsConstants.GameCubeCRight] = KeyboardB
                })
        ]);

        var controller = InputSnapshotFunctions.Apply(snapshot, configuration, false)
            .Controllers[FirstPortIndex];

        Assert.Equal(default, controller.RightX);
        Assert.Equal(short.MaxValue, controller.RightY);
        Assert.Equal(short.MinValue, controller.LeftY);
    }
}
