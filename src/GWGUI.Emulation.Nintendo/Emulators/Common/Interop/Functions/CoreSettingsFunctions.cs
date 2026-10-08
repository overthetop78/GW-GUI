using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Dictionaries;

namespace GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Functions;

internal static class CoreSettingsFunctions
{
    internal static bool IsSupportedNintendoOption(CoreOption option) =>
        !Emulators.Mesen2.Constants.OptionConstants.UnconfiguredSystemOptionPrefixes.Any(
            prefix => option.Key.StartsWith(prefix, StringComparison.Ordinal));

    internal static CoreOption WithSupportedGraphicsApis(CoreOption option)
    {
        if (option.Key is not (Emulators.Azahar.Constants.OptionConstants.GraphicsApiOption
            or Emulators.Cemu.Constants.OptionConstants.GraphicsApiOption)) return option;
        var values = option.Values.Where(value => !value.Value.Equals(
            HardwareRenderConstants.VulkanApiName, StringComparison.OrdinalIgnoreCase)).ToArray();
        var defaultValue = values.Any(value => value.Value == option.DefaultValue)
            ? option.DefaultValue : values.First().Value;
        return option with { Values = values, DefaultValue = defaultValue,
            CurrentValue = values.Any(value => value.Value == option.CurrentValue)
                ? option.CurrentValue : defaultValue };
    }

    internal static MachineConfiguration Configure(MachineConfiguration configuration)
    {
        var options = new Dictionary<string, string>(configuration.Options ?? new Dictionary<string, string>(), StringComparer.Ordinal);
        foreach (var option in CoreCatalog.Get(configuration.EmulatorId).Options.Select(WithSupportedGraphicsApis))
        {
            var fallback = DefaultValue(configuration, option);
            var selected = AppliesTo(option, configuration.Model)
                ? configuration.Options?.GetValueOrDefault(option.Key) ?? fallback : fallback;
            options[option.Key] = IsForcedOption(option.Key) ? fallback
                : AvailableValues(option, configuration.Model).Any(value => value.Value == selected)
                    ? selected : fallback;
        }
        return configuration with { Options = options };
    }

    private static string DefaultValue(MachineConfiguration configuration, CoreOption option) =>
        option.Key == Emulators.Mgba.Constants.OptionConstants.HardwareModelOption
            ? configuration.Model switch
            {
                ModelConstants.GameBoy => Emulators.Mgba.Constants.OptionConstants.GameBoyModel,
                ModelConstants.GameBoyColor => Emulators.Mgba.Constants.OptionConstants.GameBoyColorModel,
                _ => option.DefaultValue
            }
            : option.Key == Emulators.VbaM.Constants.OptionConstants.HardwareModelOption
            ? configuration.Model switch
            {
                ModelConstants.GameBoy => Emulators.VbaM.Constants.OptionConstants.GameBoyModel,
                ModelConstants.GameBoyColor => Emulators.VbaM.Constants.OptionConstants.GameBoyColorModel,
                _ => option.DefaultValue
            }
            : option.Key == Emulators.Skyemu.Constants.OptionConstants.CoreOverrideOption
            ? configuration.Model switch
            {
                ModelConstants.GameBoy or ModelConstants.GameBoyColor => Emulators.Skyemu.Constants.OptionConstants.GameBoyCore,
                ModelConstants.GameBoyAdvance => Emulators.Skyemu.Constants.OptionConstants.GameBoyAdvanceCore,
                ModelConstants.NintendoDs => Emulators.Skyemu.Constants.OptionConstants.NintendoDsCore,
                _ => option.DefaultValue
            }
            : option.Key is CoreSettingsConstants.MelonDsConsoleModeOption
            ? configuration.EmulatorId == Emulators.MelonDsDs.Constants.CoreConstants.Id
                ? configuration.Model == ModelConstants.NintendoDsi ? Emulators.MelonDsDs.Constants.OptionConstants.DsiConsoleMode : Emulators.MelonDsDs.Constants.OptionConstants.DsConsoleMode
                : CoreSettingsConstants.DsConsoleMode
            : option.DefaultValue;

    internal static IReadOnlyList<EmulationSettingsBlock> Blocks(MachineConfiguration configuration)
    {
        if (string.IsNullOrEmpty(configuration.EmulatorId)) return [];
        var fields = CoreCatalog.Get(configuration.EmulatorId).Options.Select(WithSupportedGraphicsApis).Where(option => AppliesTo(option, configuration.Model) && !IsForcedOption(option.Key)
            && (configuration.EmulatorId != Emulators.MelonDsDs.Constants.CoreConstants.Id
                || !Emulators.MelonDsDs.Constants.FirmwareConstants.PathOptions.Values.Contains(option.Key, StringComparer.Ordinal))).Select(option => new EmulationSettingsField(
            option.Key, option.Tab, CoreSettingsConstants.OptionsBlock + option.Tab,
            option.Name, EmulationSettingsEditor.Selection,
            configuration.Options?.GetValueOrDefault(option.Key) ?? DefaultValue(configuration, option),
            AvailableValues(option, configuration.Model).Select(value => new EmulationSettingsChoice(value.Value,
                value.Label.StartsWith(CoreSettingsConstants.OptionResourcePrefix, StringComparison.Ordinal) ? value.Label : string.Empty,
                value.Label.StartsWith(CoreSettingsConstants.OptionResourcePrefix, StringComparison.Ordinal) ? null : value.Label)).ToArray(),
            ExplanationResourceKey: option.Description, RequiresRestart: option.RequiresRestart)).ToArray();
        return fields.GroupBy(field => field.Tab).Select(group => new EmulationSettingsBlock(
            CoreSettingsConstants.OptionsBlock + group.Key, group.Key,
            CoreSettingsConstants.Sections[group.Key].Title, group.ToArray(),
            CoreSettingsConstants.Sections[group.Key].Icon, CoreSettingsConstants.OptionsColumnCount)).ToArray();
    }

    private static bool AppliesTo(CoreOption option, string machineId) =>
        option.IsVisible && (option.MachineIds is null || option.MachineIds.Contains(machineId));

    private static bool IsForcedOption(string key) => key is CoreSettingsConstants.MelonDsConsoleModeOption
        or Emulators.Skyemu.Constants.OptionConstants.CoreOverrideOption;

    private static IEnumerable<CoreOptionValue> AvailableValues(CoreOption option, string machineId) =>
        option.Values.Where(value => value.MachineIds is null || value.MachineIds.Contains(machineId));
}
