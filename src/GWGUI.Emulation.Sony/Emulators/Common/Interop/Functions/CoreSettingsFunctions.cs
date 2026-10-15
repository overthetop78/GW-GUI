using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using System.Globalization;
using GWGUI.Emulation.Sony.Emulators.Common.Functions;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;

internal static class CoreSettingsFunctions
{
    internal static MachineConfiguration Configure(CoreDefinition definition, MachineConfiguration configuration)
    {
        var options = new Dictionary<string, string>(configuration.Options ?? new Dictionary<string, string>(), StringComparer.Ordinal);
        foreach (var option in definition.Options)
        {
            var selected = configuration.Options?.GetValueOrDefault(option.Key) ?? option.DefaultValue;
            options[option.Key] = option.NumericRange is null ? option.Values.Any(value => value.Value == selected) ? selected : option.DefaultValue : NativeNumber(option, selected);
        }
        if (configuration.Model == ModelConstants.PlayStation
            && configuration.FirmwarePaths?.Values.Any(path => !string.IsNullOrWhiteSpace(path)) == true)
        {
            if (definition.Id == Emulators.PcsxRearmed.Constants.CoreConstants.Id)
                options[Emulators.PcsxRearmed.Constants.OptionConstants.BiosKey] = Emulators.PcsxRearmed.Constants.OptionConstants.BiosAuto;
            else if (definition.Id == Emulators.BeetlePsx.Constants.CoreConstants.Id)
                options[Emulators.BeetlePsx.Constants.OptionConstants.OverrideBiosKey] = Emulators.BeetlePsx.Constants.OptionConstants.OverrideBiosDisabled;
            else if (definition.Id == Emulators.BeetlePsxHw.Constants.CoreConstants.Id)
                options[Emulators.BeetlePsxHw.Constants.OptionConstants.OverrideBiosKey] = Emulators.BeetlePsxHw.Constants.OptionConstants.OverrideBiosDisabled;
        }
        return AnalogToggleFunctions.Configure(MemoryCardFunctions.Configure(GraphicsSettingsFunctions.Configure(configuration with { Options = options }, definition.Options)));
    }

    internal static IReadOnlyList<EmulationSettingsBlock> Blocks(CoreDefinition definition, MachineConfiguration configuration) => Blocks(definition.Options, configuration);

    internal static bool IsVisible(CoreOption option) => option.IsVisible
        && !CoreSettingsConstants.HiddenOptions.Contains(option.Key)
        && !MemoryCardFunctions.IsManagedOption(option.Key)
        && !AnalogToggleFunctions.IsManagedOption(option.Key);

    internal static IReadOnlyList<EmulationSettingsBlock> Blocks(IReadOnlyList<CoreOption> options, MachineConfiguration configuration)
    {
        configuration = GraphicsSettingsFunctions.Configure(configuration, options);
        var fields = options.Where(IsVisible).Select(source =>
        {
            var option = GraphicsSettingsFunctions.Present(source, configuration.Options?.GetValueOrDefault(source.Key) ?? source.DefaultValue, configuration.GraphicsApi);
            var tab = option.Tab == EmulationMachineTab.General && configuration.Model is not (ModelConstants.PlayStation2 or ModelConstants.Psp) ? EmulationMachineTab.Cpu : option.Tab;
            if (CoreSettingsConstants.IsCdAudio(option.Key)) tab = EmulationMachineTab.Storage;
            if (option.Key == Emulators.Pcee2.Constants.OptionConstants.FastBootKey)
                tab = EmulationMachineTab.Rom;
            if (option.Key == Emulators.BeetlePsx.Constants.OptionConstants.GpuOverclockKey
                || option.Key == Emulators.BeetlePsxHw.Constants.OptionConstants.GpuOverclockKey
                || option.Key == Emulators.BeetlePsx.Constants.OptionConstants.LineRenderKey
                || option.Key == Emulators.BeetlePsxHw.Constants.OptionConstants.LineRenderKey)
                tab = EmulationMachineTab.Video;
            var enabled = option.Values.FirstOrDefault(value => CoreSettingsConstants.EnabledLabels.Contains(value.Label));
            var disabled = option.Values.FirstOrDefault(value => CoreSettingsConstants.DisabledLabels.Contains(value.Label));
            var toggle = enabled is not null && disabled is not null && (option.Values.Count == 2 || option.Key == Emulators.PcsxRearmed.Constants.OptionConstants.ShowBiosBootlogoKey);
            var selectedValue = configuration.Options?.GetValueOrDefault(option.Key) ?? option.DefaultValue;
            if (toggle && selectedValue != disabled!.Value) selectedValue = enabled!.Value;
            var group = tab != EmulationMachineTab.Video ? string.Empty
                : CoreSettingsConstants.IsBasicVideo(option.Key) ? CoreSettingsConstants.DisplayGroup
                : option.Key.Contains(CoreSettingsConstants.GeometryGroup, StringComparison.OrdinalIgnoreCase)
                    ? CoreSettingsConstants.GeometryGroup
                : option.Key.Contains(CoreSettingsConstants.TextureGroup, StringComparison.OrdinalIgnoreCase)
                    ? CoreSettingsConstants.TextureGroup
                : CoreSettingsConstants.IsScreenOption(option.Key) ? CoreSettingsConstants.ScreenGroup
                : option.Key.Contains(CoreSettingsConstants.FrameKeyword, StringComparison.OrdinalIgnoreCase) || option.Key.Contains(CoreSettingsConstants.TimingKeyword, StringComparison.OrdinalIgnoreCase) ? CoreSettingsConstants.TimingGroup : string.Empty;
            var blockId = tab == EmulationMachineTab.Audio ? CoreSettingsConstants.AudioOptionsBlock
                : tab == EmulationMachineTab.Ram ? SettingsDescriptionFunctionsConstants.Memory
                : tab == EmulationMachineTab.Cpu ? CoreSettingsConstants.IsCpuClock(option.Key)
                    || option.Key == Emulators.Ppsspp.Constants.OptionConstants.LockedCpuSpeedKey
                    ? CoreSettingsConstants.AccelerationBlock
                    : option.Key.Contains(CoreSettingsConstants.CompatibilityKeyword, StringComparison.OrdinalIgnoreCase)
                        || option.Key.Contains(CoreSettingsConstants.DynarecKeyword, StringComparison.OrdinalIgnoreCase)
                        ? CoreSettingsConstants.OptionsBlock + CoreSettingsConstants.CompatibilityGroup
                        : SettingsDescriptionFunctionsConstants.Cpu
                : CoreSettingsConstants.OptionsBlock + tab + group;
            return new EmulationSettingsField(option.Key, tab, blockId,
                CoreSettingsConstants.IsCpuClock(option.Key) ? CoreSettingsConstants.CpuScaleLabel
                    : ControllerOptionConstants.LabelsBySuffix.FirstOrDefault(pair => option.Key.StartsWith(ControllerOptionConstants.SwanPortPrefix, StringComparison.Ordinal) && option.Key.EndsWith(pair.Key, StringComparison.Ordinal)).Value ?? option.Name,
                toggle ? EmulationSettingsEditor.Toggle : option.NumericRange is null ? EmulationSettingsEditor.Selection : EmulationSettingsEditor.Slider,
                option.NumericRange is null ? selectedValue : ReadNumber(option, configuration.Options?.GetValueOrDefault(option.Key) ?? option.DefaultValue).ToString(CoreSettingsConstants.NumericFormat, CultureInfo.InvariantCulture),
                option.Values.Select(value => value.Label.StartsWith(CoreSettingsConstants.OptionResourcePrefix, StringComparison.Ordinal) ? new EmulationSettingsChoice(value.Value, value.Label) : new EmulationSettingsChoice(value.Value, string.Empty, value.Label)).ToArray(),
                ExplanationResourceKey: ControllerOptionConstants.IsAnalogCalibration(option.Key) ? ControllerOptionConstants.AnalogCalibrationHelp : option.Description,
                DetailedExplanationResourceKey: option.CategorizedDescription ?? option.Description,
                RequiresRestart: option.RequiresRestart,
                NumericRange: option.NumericRange,
                NumericValue: option.Key == Emulators.SwanStation.Constants.OptionConstants.ConsoleEnable8MBRAMKey ? CoreSettingsConstants.DevelopmentRamExtensionBytes : null,
                EnabledValue: enabled?.Value ?? EmulationSettingConstants.Enabled,
                DisabledValue: disabled?.Value ?? EmulationSettingConstants.Disabled,
                AppliesToControllerIds: ControllerOptionFunctions.Types(option),
                ControllerPortNumber: ControllerOptionFunctions.PortNumber(option),
                RefreshSettingsOnChange: option.Key.Contains(ControllerOptionConstants.Multitap, StringComparison.OrdinalIgnoreCase)
                    || option.Key == Emulators.Ppsspp.Constants.OptionConstants.PspModelKey || option.GraphicsApiValues is not null,
                IsGlobalControllerOption: option.Key.Contains(ControllerOptionConstants.Multitap, StringComparison.OrdinalIgnoreCase));
        }).ToArray();
        return fields.GroupBy(field => (field.Tab, field.BlockId)).Select(group => new EmulationSettingsBlock(
            group.Key.BlockId, group.Key.Tab,
            group.Key.BlockId == CoreSettingsConstants.AccelerationBlock ? CoreSettingsConstants.AccelerationTitle
                : group.Key.BlockId.EndsWith(CoreSettingsConstants.CompatibilityGroup, StringComparison.Ordinal)
                    ? CoreSettingsConstants.CompatibilityTitle
                : group.Key.Tab == EmulationMachineTab.Audio ? CoreSettingsConstants.AudioOptionsTitle
                : group.Key.Tab != EmulationMachineTab.Video ? CoreSettingsConstants.Sections[group.Key.Tab].Title
                : group.Key.BlockId.EndsWith(CoreSettingsConstants.DisplayGroup, StringComparison.Ordinal)
                    || group.Key.BlockId.EndsWith(CoreSettingsConstants.ScreenGroup, StringComparison.Ordinal)
                    ? SettingsDescriptionFunctionsConstants.ResourceVideo
                : group.Key.BlockId.EndsWith(CoreSettingsConstants.GeometryGroup, StringComparison.Ordinal)
                    ? CoreSettingsConstants.GeometryTitle
                : group.Key.BlockId.EndsWith(CoreSettingsConstants.TextureGroup, StringComparison.Ordinal)
                    ? CoreSettingsConstants.TexturesTitle
                : group.Key.BlockId.EndsWith(CoreSettingsConstants.TimingGroup, StringComparison.Ordinal) ? CoreSettingsConstants.TimingTitle : CoreSettingsConstants.RenderingTitle,
            group.ToArray(), CoreSettingsConstants.Sections[group.Key.Tab].Icon,
            SettingsDescriptionFunctionsConstants.SingleColumnLayout,
            IsCollapsible: group.Key.Tab == EmulationMachineTab.Video
                && !group.Key.BlockId.EndsWith(CoreSettingsConstants.DisplayGroup, StringComparison.Ordinal),
            IsCompact: group.Key.Tab is EmulationMachineTab.Cpu or EmulationMachineTab.Video or EmulationMachineTab.Audio)).ToArray();
    }

    private static double ReadNumber(CoreOption option, string selected)
    {
        var range = option.NumericRange!;
        if (selected == option.NativeZeroValue) return 0;
        if (range.UnitSuffix.Length != 0 && selected.EndsWith(range.UnitSuffix, StringComparison.Ordinal))
            selected = selected[..^range.UnitSuffix.Length];
        if (!double.TryParse(selected, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
            || !double.IsFinite(value))
            return selected == option.DefaultValue ? range.Minimum : ReadNumber(option, option.DefaultValue);
        var clamped = Math.Clamp(value, range.Minimum, range.Maximum);
        return Math.Clamp(range.Minimum + Math.Round((clamped - range.Minimum) / range.Step, MidpointRounding.AwayFromZero) * range.Step, range.Minimum, range.Maximum);
    }

    internal static string NativeNumber(CoreOption option, string selected)
    {
        var number = ReadNumber(option, selected);
        return number == 0 && option.NativeZeroValue is not null ? option.NativeZeroValue : number.ToString(option.NativeNumericFormat ?? CoreSettingsConstants.NumericFormat, CultureInfo.InvariantCulture) + option.NumericRange!.UnitSuffix;
    }
}
