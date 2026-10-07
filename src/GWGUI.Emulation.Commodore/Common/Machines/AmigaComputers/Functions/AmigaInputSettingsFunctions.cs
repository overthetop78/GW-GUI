using CdtvKeyboard = GWGUI.Emulation.Commodore.Common.Machines.CommodoreCDTV.Constants.AmigaCdtvKeyboardConstants;
using CdtvDefaults = GWGUI.Emulation.Commodore.Common.Machines.CommodoreCDTV.Dictionaries.AmigaCdtvKeyboardDictionary;
using CdtvModels = GWGUI.Emulation.Commodore.Common.Machines.CommodoreCDTV.Constants.ModelConstants;
using ComputerKeyboard = GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants.AmigaComputerKeyboardConstants;
using ComputerDefaults = GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Dictionaries.AmigaComputerKeyboardDictionary;

namespace GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Functions;

internal static class AmigaInputSettingsFunctions
{
    internal static IReadOnlyList<InputBindingDefinition> KeyboardDefinitions(MachineConfiguration configuration)
    {
        var keys = configuration.Model == CdtvModels.CDTV ? CdtvKeyboard.SpecialKeys : ComputerKeyboard.SpecialKeys;
        var defaults = configuration.Model == CdtvModels.CDTV ? CdtvDefaults.DefaultHostKeys : ComputerDefaults.DefaultHostKeys;
        return keys.Select(key => InputSettingsFunctions.Definition(key.ToString(),
            InputSettingsFunctions.KeyResource(key), InputSettingsFunctions.DefaultKey(key, defaults))).ToArray();
    }

    internal static MachineConfiguration Apply(MachineConfiguration configuration)
    {
        var options = new Dictionary<string, string>(configuration.Options ?? new Dictionary<string, string>());
        options[SettingsConstants.OptionTurboFire] = configuration.Input?.ControllerBindings?
            .Any(binding => binding.ButtonMappings?.Any(item => item.Key == InputSettingsFunctionsConstants.L2
                && !string.IsNullOrWhiteSpace(item.Value)) == true) == true
            ? InputSettingsFunctionsConstants.Enabled : InputSettingsFunctionsConstants.Disabled;
        options[SettingsConstants.OptionTurboFireButton] = InputSettingsFunctionsConstants.L2;
        return configuration with { Options = options };
    }
}
