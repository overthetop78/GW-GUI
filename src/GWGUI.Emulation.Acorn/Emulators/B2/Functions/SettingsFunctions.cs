using GWGUI.Emulation.Acorn.Emulators.B2.Constants;
using GWGUI.Emulation.Acorn.Emulators.B2.Dictionaries;

namespace GWGUI.Emulation.Acorn.Emulators.B2.Functions;

internal static class EmulatorSettingsFunctions
{
    internal static IReadOnlyList<CoreOption> Options(MachineConfiguration? configuration)
    {
        var models = configuration is null ? B2ModelCatalog.All : B2ModelFunctions.GetModels(configuration.Model);
        var defaultModel = configuration is null ? OptionConstants.ModelBAcorn1770 : B2ModelFunctions.DefaultValue(configuration.Model);
        var options = new List<CoreOption>
        {
            new(OptionConstants.Model, SettingsConstants.ResourceNativeModel, null, EmulationMachineTab.General.ToString(),
                defaultModel, configuration?.Options.GetValueOrDefault(OptionConstants.Model, defaultModel) ?? defaultModel,
                models.Select(model => new CoreOptionValue(model.NativeValue, model.NativeValue, InvariantDisplayValue: model.NativeValue)).ToArray(),
                RequiresRestart: true, ResourceKey: SettingsConstants.ResourceNativeModel),
            new(OptionConstants.AutoBoot, SettingsConstants.ResourceAutoBoot, null, EmulationMachineTab.General.ToString(),
                OptionConstants.Enabled, configuration?.Options.GetValueOrDefault(OptionConstants.AutoBoot, OptionConstants.Enabled) ?? OptionConstants.Enabled,
                [new(OptionConstants.Enabled, OptionConstants.Enabled, ResourceKey: "Emulation.Value.Enabled"), new(OptionConstants.Disabled, OptionConstants.Disabled, ResourceKey: "Emulation.Value.Disabled")],
                RequiresRestart: true, ResourceKey: SettingsConstants.ResourceAutoBoot)
        };
        options.AddRange(B2OptionCatalog.JoypadOptions.Select(key => new CoreOption(key, key, null, null, OptionConstants.NoKey, OptionConstants.NoKey,
            B2OptionCatalog.KeyboardValues.Select(value => new CoreOptionValue(value, value, InvariantDisplayValue: value)).ToArray(), IsVisible: false)));
        return options;
    }

    internal static IReadOnlyList<EmulationSettingsBlock> Describe(MachineConfiguration configuration)
    {
        var options = Options(configuration).Where(option => option.IsVisible).ToArray();
        return [new(SettingsConstants.ResourceNativeModel, EmulationMachineTab.General, SettingsConstants.ResourceNativeModel,
            options.Select(option => new EmulationSettingsField(option.Key, EmulationMachineTab.General, EmulationMachineTab.General.ToString(),
                option.ResourceKey!, option.Key == OptionConstants.AutoBoot ? EmulationSettingsEditor.Toggle : EmulationSettingsEditor.Selection,
                option.CurrentValue, option.Values.Select(value => new EmulationSettingsChoice(value.Value, value.ResourceKey ?? string.Empty, value.InvariantDisplayValue)).ToArray(),
                EnabledValue: OptionConstants.Enabled, DisabledValue: OptionConstants.Disabled, RequiresRestart: false)).ToArray()),
            new(SettingsConstants.ResourceFirmware, EmulationMachineTab.Rom, SettingsConstants.ResourceFirmware,
                [new EmulationSettingsField(SettingsConstants.Firmware, EmulationMachineTab.Rom, EmulationMachineTab.Rom.ToString(),
                    SettingsConstants.ResourceFirmware, EmulationSettingsEditor.Information, null,
                    ExplanationResourceKey: SettingsConstants.ResourceEmbeddedFirmware)])];
    }
}
