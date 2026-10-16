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
            if (option.Key.EndsWith("skip_bios", StringComparison.Ordinal)
                || option.Key == Emulators.SwanStation.Constants.OptionConstants.BIOSPatchFastBootKey
                || option.Key == Emulators.PcsxRearmed.Constants.OptionConstants.ShowBiosBootlogoKey)
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
            var automaticClock = option.Key == Emulators.PcsxRearmed.Constants.OptionConstants.PsxclockKey;
            var range = automaticClock ? new EmulationSettingsNumericRange(30, 100, 1, " %") : option.NumericRange;
            if (range is not null && option.Key.Contains("scanline", StringComparison.OrdinalIgnoreCase))
                range = range with { IsVertical = true };
            if (range is not null && (option.Key == Emulators.SwanStation.Constants.OptionConstants.GPUResolutionScaleKey
                || option.Key.Contains("internal_resolution", StringComparison.OrdinalIgnoreCase)
                || option.Key.Contains("upscale_multiplier", StringComparison.OrdinalIgnoreCase)
                || option.Key.Contains("CDROM_ReadSpeedup", StringComparison.OrdinalIgnoreCase)
                || option.Key.Contains("CDROM_SeekSpeedup", StringComparison.OrdinalIgnoreCase)))
                range = range with { UnitSuffix = "×" };
            if (toggle && selectedValue != disabled!.Value) selectedValue = enabled!.Value;
            var dimensionChoices = option.Key == Emulators.SwanStation.Constants.OptionConstants.GPUResolutionScaleKey;
            var instantaneousChoice = option.Key == Emulators.SwanStation.Constants.OptionConstants.CDROMSeekSpeedupKey;
            var choices = dimensionChoices
                ? Enumerable.Range(1, 16).Select(scale => PresentChoice(option, new CoreOptionValue(scale.ToString(CultureInfo.InvariantCulture), string.Empty), configuration.Model)).ToArray()
                : option.Values.Select(value => PresentChoice(option, value, configuration.Model)).ToArray();
            if (instantaneousChoice) choices = [new("0", "Emulation.Option.Sony.Value.Instantaneous"), .. choices.Where(choice => choice.Id != "0")];
            var group = tab != EmulationMachineTab.Video ? string.Empty
                : option.Key.Contains("scanline", StringComparison.OrdinalIgnoreCase)
                    && !option.Key.Contains("pal", StringComparison.OrdinalIgnoreCase) ? "ScanNTSC"
                : option.Key.Contains("scanline", StringComparison.OrdinalIgnoreCase)
                    && option.Key.Contains("pal", StringComparison.OrdinalIgnoreCase) ? "ScanPAL"
                : CoreSettingsConstants.IsBasicVideo(option.Key) ? CoreSettingsConstants.DisplayGroup
                : option.Key.Contains(CoreSettingsConstants.GeometryGroup, StringComparison.OrdinalIgnoreCase)
                    ? CoreSettingsConstants.GeometryGroup
                : option.Key.Contains("hdr", StringComparison.OrdinalIgnoreCase) ? "HDR"
                : new[] { "color", "dither", "depth", "black_setup", "phase_error", "primaries", "analog" }
                    .Any(part => option.Key.Contains(part, StringComparison.OrdinalIgnoreCase)) ? "Color"
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
                : tab == EmulationMachineTab.Rom ? SettingsDescriptionFunctionsConstants.Firmware
                : CoreSettingsConstants.OptionsBlock + tab + group;
            return new EmulationSettingsField(option.Key, tab, blockId,
                CoreSettingsConstants.IsCpuClock(option.Key) && !automaticClock ? CoreSettingsConstants.CpuScaleLabel
                    : ControllerOptionConstants.LabelsBySuffix.FirstOrDefault(pair => option.Key.StartsWith(ControllerOptionConstants.SwanPortPrefix, StringComparison.Ordinal) && option.Key.EndsWith(pair.Key, StringComparison.Ordinal)).Value ?? option.Name,
                toggle ? EmulationSettingsEditor.Toggle : range is null ? EmulationSettingsEditor.Selection : EmulationSettingsEditor.Slider,
                option.NumericRange is null ? selectedValue : ReadNumber(option, configuration.Options?.GetValueOrDefault(option.Key) ?? option.DefaultValue).ToString(CoreSettingsConstants.NumericFormat, CultureInfo.InvariantCulture),
                choices,
                ExplanationResourceKey: ControllerOptionConstants.IsAnalogCalibration(option.Key) ? ControllerOptionConstants.AnalogCalibrationHelp : option.Description,
                DetailedExplanationResourceKey: option.CategorizedDescription ?? option.Description,
                RequiresRestart: option.RequiresRestart,
                NumericRange: range,
                NumericValue: option.Key == Emulators.SwanStation.Constants.OptionConstants.ConsoleEnable8MBRAMKey ? CoreSettingsConstants.DevelopmentRamExtensionBytes : null,
                EnabledValue: enabled?.Value ?? EmulationSettingConstants.Enabled,
                DisabledValue: disabled?.Value ?? EmulationSettingConstants.Disabled,
                AppliesToControllerIds: ControllerOptionFunctions.Types(option),
                ControllerPortNumber: ControllerOptionFunctions.PortNumber(option),
                RefreshSettingsOnChange: option.Key.Contains(ControllerOptionConstants.Multitap, StringComparison.OrdinalIgnoreCase)
                    || option.Key == Emulators.Ppsspp.Constants.OptionConstants.PspModelKey || option.GraphicsApiValues is not null,
                IsGlobalControllerOption: option.Key.Contains(ControllerOptionConstants.Multitap, StringComparison.OrdinalIgnoreCase),
                AutomaticValue: automaticClock ? Emulators.PcsxRearmed.Constants.OptionConstants.PsxclockAuto : null,
                UseChoiceLabelsForSlider: dimensionChoices || instantaneousChoice);
        }).ToArray();
        fields = fields.SelectMany(field => field.Id == Emulators.SwanStation.Constants.OptionConstants.ControllerPortsMultitapModeKey
            ? Enumerable.Range(1, 2).Select(port => field with
            {
                Id = field.Id + ".port" + port,
                LabelResourceKey = "Emulation.Sony.Multitap.Port" + port,
                Editor = EmulationSettingsEditor.Toggle,
                Value = field.Value == "BothPorts" || field.Value == $"Port{port}Only" ? EmulationSettingConstants.Enabled : EmulationSettingConstants.Disabled,
                EnabledValue = EmulationSettingConstants.Enabled, DisabledValue = EmulationSettingConstants.Disabled,
                Choices = null
            }) : new[] { field }).ToArray();
        return fields.GroupBy(field => (field.Tab, field.BlockId)).Select(group => new EmulationSettingsBlock(
            group.Key.BlockId, group.Key.Tab,
            group.Key.BlockId == CoreSettingsConstants.AccelerationBlock ? CoreSettingsConstants.AccelerationTitle
                : group.Key.BlockId.EndsWith(CoreSettingsConstants.CompatibilityGroup, StringComparison.Ordinal)
                    ? CoreSettingsConstants.CompatibilityTitle
                : group.Key.Tab == EmulationMachineTab.Audio ? CoreSettingsConstants.AudioOptionsTitle
                : group.Key.Tab != EmulationMachineTab.Video ? CoreSettingsConstants.Sections[group.Key.Tab].Title
                : group.Key.BlockId.EndsWith("ScanNTSC", StringComparison.Ordinal) ? "Emulation.Sony.Video.NTSC"
                : group.Key.BlockId.EndsWith("ScanPAL", StringComparison.Ordinal) ? "Emulation.Sony.Video.PAL"
                : group.Key.BlockId.EndsWith("HDR", StringComparison.Ordinal) ? "Emulation.Sony.Video.HDR"
                : group.Key.BlockId.EndsWith("Color", StringComparison.Ordinal) ? "Emulation.Sony.Video.Color"
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
                && !group.Key.BlockId.EndsWith(CoreSettingsConstants.DisplayGroup, StringComparison.Ordinal)
                && !group.Key.BlockId.EndsWith(CoreSettingsConstants.ScreenGroup, StringComparison.Ordinal),
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

    private static EmulationSettingsChoice PresentChoice(CoreOption option, CoreOptionValue value, string model)
    {
        if (option.Key.Contains("cd_fastload", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(value.Value.TrimEnd('x'), out var speed))
            return new(value.Value, string.Empty, speed + "×");
        if (option.Key.Contains("internal_resolution", StringComparison.OrdinalIgnoreCase)
            || option.Key.Contains("GPU_ResolutionScale", StringComparison.OrdinalIgnoreCase))
        {
            var number = new string(value.Value.TakeWhile(char.IsDigit).ToArray());
            if (int.TryParse(number, out var scale) && scale > 0)
            {
                var width = model == ModelConstants.Psp ? 480 : 320;
                var height = model == ModelConstants.Psp ? 272 : 240;
                return new(value.Value, string.Empty, $"{scale}× ({width} × {height} → {width * scale} × {height * scale} px)");
            }
        }
        return value.Label.StartsWith(CoreSettingsConstants.OptionResourcePrefix, StringComparison.Ordinal)
            ? new(value.Value, value.Label) : new(value.Value, string.Empty, value.Label);
    }

    internal static string NativeNumber(CoreOption option, string selected)
    {
        var number = ReadNumber(option, selected);
        return number == 0 && option.NativeZeroValue is not null ? option.NativeZeroValue : number.ToString(option.NativeNumericFormat ?? CoreSettingsConstants.NumericFormat, CultureInfo.InvariantCulture) + option.NumericRange!.UnitSuffix;
    }
}
