using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Nintendo.Common.Machines.Common.Constants;
using GWGUI.Emulation.Nintendo.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Nintendo.Common.Machines.Common.Enums;
using GWGUI.Emulation.Nintendo.Common.Machines.Common.Functions;

namespace GWGUI.Tests.Emulation.Nintendo;

public sealed class NesMaxControllerTests
{
    private const int FirstPortIndex = 0;
    private const int MesenAButtonBit = 0;
    private const int MesenBButtonBit = 1;
    private const int MesenTurboAButtonBit = 8;
    private const int MesenTurboBButtonBit = 9;
    private const string KeyboardA = "Keyboard:A";
    private const string KeyboardB = "Keyboard:B";
    private const string KeyboardC = "Keyboard:C";
    private const string KeyboardD = "Keyboard:D";

    [Fact]
    public void NesMaxExposesDistinctTurboButtons()
    {
        var settings = InputSettingsFunctions.Describe(
            new MachineConfiguration(ModelConstants.Nes, string.Empty));
        var choice = Assert.Single(settings.ControllerPorts[FirstPortIndex]
            .ControllerChoices, item => item.Id == ControllerType.Joystick.ToString());

        Assert.Contains(EmulationControllerVisualIds.NintendoNesMax,
            choice.CompatibleVisualIds!);
        Assert.Contains(EmulationControllerVisualIds.NintendoNesAdvantage,
            choice.CompatibleVisualIds!);
        var bindingIds = choice.BindingDefinitions!.Select(item => item.Id).ToHashSet();
        Assert.Contains(InputSettingsFunctionsConstants.NesTurboA, bindingIds);
        Assert.Contains(InputSettingsFunctionsConstants.NesTurboB, bindingIds);
        Assert.Equal(InputSettingsFunctionsConstants.NesTurboB,
            choice.VisualCommandIds![EmulationControllerVisualControl.TertiaryAction]);
        Assert.Equal(InputSettingsFunctionsConstants.NesTurboA,
            choice.VisualCommandIds[EmulationControllerVisualControl.QuaternaryAction]);
        Assert.All(choice.VisualCommandIds, item => Assert.Contains(item.Value, bindingIds));

        var famicomSettings = InputSettingsFunctions.Describe(
            new MachineConfiguration(ModelConstants.FamicomDisk, string.Empty));
        var famicomChoice = Assert.Single(famicomSettings.ControllerPorts[FirstPortIndex]
            .ControllerChoices, item => item.Id == ControllerType.Joystick.ToString());
        Assert.DoesNotContain(EmulationControllerVisualIds.NintendoNesMax,
            famicomChoice.CompatibleVisualIds!);
        Assert.DoesNotContain(EmulationControllerVisualIds.NintendoNesAdvantage,
            famicomChoice.CompatibleVisualIds!);
    }

    [Fact]
    public void NesBindingsReachMesenNormalAndTurboButtonsWithoutChangingSnes()
    {
        var configuration = new InputConfiguration(ControllerBindings:
        [
            new ControllerBinding(FirstPortIndex, ControllerType.Joystick,
                ButtonMappings: new Dictionary<string, string>
                {
                    [EmulationControllerCommandIds.A] = KeyboardA,
                    [EmulationControllerCommandIds.B] = KeyboardB,
                    [InputSettingsFunctionsConstants.NesTurboA] = KeyboardC,
                    [InputSettingsFunctionsConstants.NesTurboB] = KeyboardD
                })
        ]);
        var snapshot = EmulationInputSnapshot.Empty with
        {
            Keys = new HashSet<EmulationKey>
            {
                EmulationKey.A, EmulationKey.B, EmulationKey.C, EmulationKey.D
            }
        };

        var nes = InputSnapshotFunctions.Apply(snapshot, ModelConstants.Nes,
            configuration, false).Controllers[FirstPortIndex];
        var snes = InputSnapshotFunctions.Apply(snapshot, ModelConstants.Snes,
            configuration, false).Controllers[FirstPortIndex];

        Assert.Equal((1u << MesenAButtonBit) | (1u << MesenBButtonBit)
            | (1u << MesenTurboAButtonBit) | (1u << MesenTurboBButtonBit),
            nes.Buttons);
        Assert.Equal((1u << MesenAButtonBit) | (1u << MesenTurboAButtonBit),
            snes.Buttons);
    }
}
