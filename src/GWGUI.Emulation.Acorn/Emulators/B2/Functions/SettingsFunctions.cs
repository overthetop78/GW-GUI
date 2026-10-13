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
            new(OptionConstants.Model, SettingsConstants.ResourceDiscController, null, EmulationMachineTab.Storage.ToString(),
                defaultModel, configuration?.Options.GetValueOrDefault(OptionConstants.Model, defaultModel) ?? defaultModel,
                models.Select(model => new CoreOptionValue(model.NativeValue, model.NativeValue,
                    InvariantDisplayValue: model.NativeValue.StartsWith("B/", StringComparison.Ordinal) ? model.NativeValue[2..] : model.NativeValue)).ToArray(),
                RequiresRestart: true, ResourceKey: SettingsConstants.ResourceDiscController),
            new(OptionConstants.AutoBoot, SettingsConstants.ResourceAutoBoot, null, EmulationMachineTab.Storage.ToString(),
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
        var native = configuration.Options.GetValueOrDefault(OptionConstants.Model, B2ModelFunctions.DefaultValue(configuration.Model));
        var storage = new List<EmulationSettingsField>();
        if (configuration.Model == MachineModel.BbcMicroModelB)
            storage.Add(new(OptionConstants.DiscController, EmulationMachineTab.Storage, SettingsConstants.ResourceDiscController,
                SettingsConstants.ResourceDiscController, EmulationSettingsEditor.Selection, ConfigurationOptionFunctions.DiscController(native),
                B2ModelFunctions.GetModels(configuration.Model).Select(model => ConfigurationOptionFunctions.DiscController(model.NativeValue))
                    .Distinct(StringComparer.Ordinal).Select(value => new EmulationSettingsChoice(value, string.Empty, value)).ToArray(),
                RefreshSettingsOnChange: true));
        storage.Add(new(OptionConstants.AutoBoot, EmulationMachineTab.Storage, SettingsConstants.ResourceDiscController,
            SettingsConstants.ResourceAutoBoot, EmulationSettingsEditor.Toggle,
            configuration.Options.GetValueOrDefault(OptionConstants.AutoBoot, OptionConstants.Enabled),
            EnabledValue: OptionConstants.Enabled, DisabledValue: OptionConstants.Disabled));
        var blocks = new List<EmulationSettingsBlock>
        {
            new(SettingsConstants.ResourceDiscController, EmulationMachineTab.Storage, "Emulation.Tab.Storage", storage),
            new(SettingsConstants.ResourceFirmware, EmulationMachineTab.Rom, SettingsConstants.ResourceFirmware,
                [new EmulationSettingsField(SettingsConstants.Firmware, EmulationMachineTab.Rom, EmulationMachineTab.Rom.ToString(),
                    SettingsConstants.ResourceFirmware, EmulationSettingsEditor.Information, "embedded",
                    [new("embedded", SettingsConstants.ResourceEmbeddedFirmware)])])
        };
        if (configuration.Model == MachineModel.BbcMicroModelB && ConfigurationOptionFunctions.DiscController(native) == OptionConstants.ChallengerController)
            blocks.Add(new(OptionConstants.DiscMemory, EmulationMachineTab.Ram, "Emulation.Memory.Extensions",
                [new(OptionConstants.DiscMemory, EmulationMachineTab.Ram, OptionConstants.DiscMemory, "Emulation.Memory.Extensions",
                    EmulationSettingsEditor.Selection, ConfigurationOptionFunctions.DiscMemory(native),
                    [new(OptionConstants.Challenger256, string.Empty, "256 KiB"), new(OptionConstants.Challenger512, string.Empty, "512 KiB")])]));
        return blocks;
    }
}
