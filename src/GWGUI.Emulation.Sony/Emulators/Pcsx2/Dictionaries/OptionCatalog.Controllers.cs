using GWGUI.Emulation.Sony.Emulators.Pcsx2.Constants;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Enums;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Functions;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;

namespace GWGUI.Emulation.Sony.Emulators.Pcsx2.Dictionaries;

internal static partial class OptionCatalog
{
    private static IReadOnlyList<CoreOption> ControllersOptions { get; } =
    [
        Define(OptionConstants.AxisDeadzone1Key, OptionConstants.AxisDeadzone1Label, null,
            "15%", EmulationMachineTab.Controllers,
            [], range: OptionRangeConstants.Deadzone),
        Define(OptionConstants.ButtonDeadzone1Key, OptionConstants.ButtonDeadzone1Label, null,
            "0%", EmulationMachineTab.Controllers,
            [], range: OptionRangeConstants.Deadzone),
        Define(OptionConstants.AxisScale1Key, OptionConstants.AxisScale1Label, null,
            "133%", EmulationMachineTab.Controllers,
            [], range: OptionRangeConstants.Sensitivity),
        Define(OptionConstants.InvertLeftStick1Key, OptionConstants.InvertLeftStick1Label, null,
            "disabled", EmulationMachineTab.Controllers,
            [
                new(OptionValueFunctions.ToNative(AnalogAxisInversion.Disabled), OptionValueResourceConstants.DisabledLabelResource),
                new(OptionValueFunctions.ToNative(AnalogAxisInversion.Horizontal), "Emulation.Option.Sony.Value.LeftRight"),
                new(OptionValueFunctions.ToNative(AnalogAxisInversion.Vertical), "Emulation.Option.Sony.Value.UpDown"),
                new(OptionValueFunctions.ToNative(AnalogAxisInversion.Both), "Emulation.Option.Sony.Value.UpDownAndLeftRight"),
            ]),
        Define(OptionConstants.InvertRightStick1Key, OptionConstants.InvertRightStick1Label, null,
            "disabled", EmulationMachineTab.Controllers,
            [
                new(OptionValueFunctions.ToNative(AnalogAxisInversion.Disabled), OptionValueResourceConstants.DisabledLabelResource),
                new(OptionValueFunctions.ToNative(AnalogAxisInversion.Horizontal), "Emulation.Option.Sony.Value.LeftRight"),
                new(OptionValueFunctions.ToNative(AnalogAxisInversion.Vertical), "Emulation.Option.Sony.Value.UpDown"),
                new(OptionValueFunctions.ToNative(AnalogAxisInversion.Both), "Emulation.Option.Sony.Value.UpDownAndLeftRight"),
            ]),
        Define(OptionConstants.AnalogMode1Key, OptionConstants.AnalogMode1Label, OptionConstants.AnalogMode1Help,
            "disabled", EmulationMachineTab.Controllers,
            ToggleChoices),
        Define(OptionConstants.EnableRumble1Key, OptionConstants.EnableRumble1Label, null,
            "100%", EmulationMachineTab.Controllers,
            [], range: OptionRangeConstants.Vibration, nativeZero: ToggleValueConstants.Disabled),
        Define(OptionConstants.AxisDeadzone2Key, OptionConstants.AxisDeadzone2Label, null,
            "15%", EmulationMachineTab.Controllers,
            [], range: OptionRangeConstants.Deadzone),
        Define(OptionConstants.ButtonDeadzone2Key, OptionConstants.ButtonDeadzone2Label, null,
            "0%", EmulationMachineTab.Controllers,
            [], range: OptionRangeConstants.Deadzone),
        Define(OptionConstants.AxisScale2Key, OptionConstants.AxisScale2Label, null,
            "133%", EmulationMachineTab.Controllers,
            [], range: OptionRangeConstants.Sensitivity),
        Define(OptionConstants.InvertLeftStick2Key, OptionConstants.InvertLeftStick2Label, null,
            "disabled", EmulationMachineTab.Controllers,
            [
                new(OptionValueFunctions.ToNative(AnalogAxisInversion.Disabled), OptionValueResourceConstants.DisabledLabelResource),
                new(OptionValueFunctions.ToNative(AnalogAxisInversion.Horizontal), "Emulation.Option.Sony.Value.LeftRight"),
                new(OptionValueFunctions.ToNative(AnalogAxisInversion.Vertical), "Emulation.Option.Sony.Value.UpDown"),
                new(OptionValueFunctions.ToNative(AnalogAxisInversion.Both), "Emulation.Option.Sony.Value.UpDownAndLeftRight"),
            ]),
        Define(OptionConstants.InvertRightStick2Key, OptionConstants.InvertRightStick2Label, null,
            "disabled", EmulationMachineTab.Controllers,
            [
                new(OptionValueFunctions.ToNative(AnalogAxisInversion.Disabled), OptionValueResourceConstants.DisabledLabelResource),
                new(OptionValueFunctions.ToNative(AnalogAxisInversion.Horizontal), "Emulation.Option.Sony.Value.LeftRight"),
                new(OptionValueFunctions.ToNative(AnalogAxisInversion.Vertical), "Emulation.Option.Sony.Value.UpDown"),
                new(OptionValueFunctions.ToNative(AnalogAxisInversion.Both), "Emulation.Option.Sony.Value.UpDownAndLeftRight"),
            ]),
        Define(OptionConstants.AnalogMode2Key, OptionConstants.AnalogMode2Label, OptionConstants.AnalogMode2Help,
            "disabled", EmulationMachineTab.Controllers,
            ToggleChoices),
        Define(OptionConstants.EnableRumble2Key, OptionConstants.EnableRumble2Label, null,
            "100%", EmulationMachineTab.Controllers,
            [], range: OptionRangeConstants.Vibration, nativeZero: ToggleValueConstants.Disabled),
    ];
}
