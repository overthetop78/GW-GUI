using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;
using Swan = GWGUI.Emulation.Sony.Emulators.SwanStation.Constants.OptionConstants;
using Rearmed = GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Constants.OptionConstants;
using Beetle = GWGUI.Emulation.Sony.Emulators.BeetlePsx.Constants.OptionConstants;
using BeetleHw = GWGUI.Emulation.Sony.Emulators.BeetlePsxHw.Constants.OptionConstants;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;

internal static class AnalogToggleFunctions
{
    private const string SwanL1R1SelectCombo = "3";
    private const string NoHoldDelay = "0";
    private const uint L1R1SelectMask = (1u << 10) | (1u << 11) | (1u << 2);

    internal static uint ButtonMask(EmulationControllerState controller, PeripheralCategory type) =>
        type is PeripheralCategory.DualShock or PeripheralCategory.AnalogController or PeripheralCategory.AnalogJoystick && ControllerInputFunctions.PortButton(controller, InputSettingsFunctionsConstants.Analog) ? controller.Buttons | L1R1SelectMask : controller.Buttons;

    internal static MachineConfiguration Configure(MachineConfiguration configuration)
    {
        if (configuration.Model != ModelConstants.PlayStation) return configuration;
        var options = new Dictionary<string, string>(configuration.Options ?? new Dictionary<string, string>(), StringComparer.Ordinal);
        switch (configuration.EmulatorId)
        {
            case Emulators.SwanStation.Constants.CoreConstants.Id:
                options[Swan.ControllerAnalogComboKey] = SwanL1R1SelectCombo;
                break;
            case Emulators.PcsxRearmed.Constants.CoreConstants.Id:
                options[Rearmed.AnalogComboKey] = Rearmed.AnalogComboL1PlusR1PlusSelect;
                break;
            case Emulators.BeetlePsx.Constants.CoreConstants.Id:
                ConfigureBeetle(Beetle.AnalogToggleKey, Beetle.AnalogToggleEnabledAnalog, Beetle.AnalogToggleEnabled, Beetle.AnalogToggleComboKey, Beetle.AnalogToggleComboL1PlusR1PlusSelect, Beetle.AnalogToggleHoldKey);
                break;
            case Emulators.BeetlePsxHw.Constants.CoreConstants.Id:
                ConfigureBeetle(BeetleHw.AnalogToggleKey, BeetleHw.AnalogToggleEnabledAnalog, BeetleHw.AnalogToggleEnabled, BeetleHw.AnalogToggleComboKey, BeetleHw.AnalogToggleComboL1PlusR1PlusSelect, BeetleHw.AnalogToggleHoldKey);
                break;
        }
        return configuration with { Options = options };

        void ConfigureBeetle(string toggleKey, string analogDefault, string enabled, string comboKey, string combo, string holdKey)
        {
            if (options.GetValueOrDefault(toggleKey) != analogDefault) options[toggleKey] = enabled;
            options[comboKey] = combo;
            options[holdKey] = NoHoldDelay;
        }
    }

    internal static bool IsManagedOption(string key) => key is Swan.ControllerAnalogComboKey or Rearmed.AnalogComboKey or Beetle.AnalogToggleKey or Beetle.AnalogToggleComboKey or Beetle.AnalogToggleHoldKey or BeetleHw.AnalogToggleKey or BeetleHw.AnalogToggleComboKey or BeetleHw.AnalogToggleHoldKey;
}
