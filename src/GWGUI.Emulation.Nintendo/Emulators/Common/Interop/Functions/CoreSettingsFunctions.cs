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
            options[option.Key] = option.Key == CoreSettingsConstants.MelonDsConsoleModeOption
                ? DefaultValue(configuration, option)
                : configuration.Options?.GetValueOrDefault(option.Key) ?? DefaultValue(configuration, option);
        return configuration with { Options = options };
    }

    private static string DefaultValue(MachineConfiguration configuration, CoreOption option) =>
        option.Key is CoreSettingsConstants.MelonDsConsoleModeOption
            ? configuration.EmulatorId == Emulators.MelonDsDs.Constants.CoreConstants.Id
                ? configuration.Model == ModelConstants.NintendoDsi ? Emulators.MelonDsDs.Constants.OptionConstants.DsiConsoleMode : Emulators.MelonDsDs.Constants.OptionConstants.DsConsoleMode
                : CoreSettingsConstants.DsConsoleMode
            : option.DefaultValue;

    internal static IReadOnlyList<EmulationSettingsBlock> Blocks(MachineConfiguration configuration)
    {
        if (string.IsNullOrEmpty(configuration.EmulatorId)) return [];
        var fields = CoreCatalog.Get(configuration.EmulatorId).Options.Select(WithSupportedGraphicsApis).Where(option => option.Key != CoreSettingsConstants.MelonDsConsoleModeOption
            && (configuration.EmulatorId != Emulators.MelonDsDs.Constants.CoreConstants.Id
                || !Emulators.MelonDsDs.Constants.FirmwareConstants.PathOptions.Values.Contains(option.Key, StringComparer.Ordinal))).Select(option => new EmulationSettingsField(
            option.Key, EmulationMachineTab.General, CoreSettingsConstants.OptionsBlock,
            option.Name, EmulationSettingsEditor.Selection,
            configuration.Options?.GetValueOrDefault(option.Key) ?? DefaultValue(configuration, option),
            option.Values.Select(value => new EmulationSettingsChoice(value.Value,
                value.Label.StartsWith(CoreSettingsConstants.OptionResourcePrefix, StringComparison.Ordinal) ? value.Label : string.Empty,
                value.Label.StartsWith(CoreSettingsConstants.OptionResourcePrefix, StringComparison.Ordinal) ? null : value.Label)).ToArray(),
            ExplanationResourceKey: option.Description, RequiresRestart: true)).ToArray();
        return fields.Length == 0 ? [] : [new EmulationSettingsBlock(CoreSettingsConstants.OptionsBlock,
            EmulationMachineTab.General, CoreSettingsConstants.OptionsResource, fields,
            CoreSettingsConstants.OptionsIcon, CoreSettingsConstants.OptionsColumnCount)];
    }
}
