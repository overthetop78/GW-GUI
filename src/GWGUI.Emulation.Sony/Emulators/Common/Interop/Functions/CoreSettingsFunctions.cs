using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using System.Globalization;
using Rpcs3Options = GWGUI.Emulation.Sony.Emulators.Rpcs3.Constants.OptionConstants;
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
        return SystemLanguageFunctions.Configure(AnalogToggleFunctions.Configure(MemoryCardFunctions.Configure(GraphicsSettingsFunctions.Configure(configuration with { Options = options }, definition.Options))));
    }

    internal static IReadOnlyList<EmulationSettingsBlock> Blocks(CoreDefinition definition, MachineConfiguration configuration) => Blocks(definition.Options, configuration);

    internal static bool IsVisible(CoreOption option) => option.IsVisible
        && !CoreSettingsConstants.HiddenOptions.Contains(option.Key)
        && !MemoryCardFunctions.IsManagedOption(option.Key)
        && !SystemLanguageFunctions.IsManagedOption(option.Key)
        && !AnalogToggleFunctions.IsManagedOption(option.Key);

    internal static IReadOnlyList<EmulationSettingsBlock> Blocks(IReadOnlyList<CoreOption> options, MachineConfiguration configuration)
    {
        configuration = GraphicsSettingsFunctions.Configure(configuration, options);
        var fields = options.Where(IsVisible).Select(source =>
        {
            var option = GraphicsSettingsFunctions.Present(source, configuration.Options?.GetValueOrDefault(source.Key) ?? source.DefaultValue, configuration.GraphicsApi);
            var tab = option.Tab == EmulationMachineTab.General && configuration.Model is not (ModelConstants.PlayStation2 or ModelConstants.Psp) ? EmulationMachineTab.Cpu : option.Tab;
            if (CoreSettingsConstants.IsCdAudio(option.Key)) tab = EmulationMachineTab.Storage;
            if (configuration.Model == ModelConstants.PlayStation3)
            {
                if (option.Key == Emulators.Rpcs3.Constants.OptionConstants.LicenseAreaKey)
                    tab = EmulationMachineTab.Rom;
                if (option.Key == Emulators.Rpcs3.Constants.OptionConstants.EnterButtonKey)
                    tab = EmulationMachineTab.Controllers;
                if (option.Key == Emulators.Rpcs3.Constants.OptionConstants.DatabaseOverrideKey)
                    tab = EmulationMachineTab.Cheats;
                if (option.Key is Emulators.Rpcs3.Constants.OptionConstants.ShowTrophyPopupsKey
                    or Emulators.Rpcs3.Constants.OptionConstants.ShowPpuCompilationHintKey
                    or Emulators.Rpcs3.Constants.OptionConstants.ShowShaderCompilationHintKey)
                    tab = EmulationMachineTab.Video;
                if (option.Key is Emulators.Rpcs3.Constants.OptionConstants.DisableFifoReorderingKey
                    or Emulators.Rpcs3.Constants.OptionConstants.DisableHwBlendingKey
                    or Emulators.Rpcs3.Constants.OptionConstants.DisableHwColorspaceKey
                    or Emulators.Rpcs3.Constants.OptionConstants.DisableZcullQueriesKey
                    or Emulators.Rpcs3.Constants.OptionConstants.CpuBlitKey
                    or Emulators.Rpcs3.Constants.OptionConstants.GpuTextureScalingKey
                    or Emulators.Rpcs3.Constants.OptionConstants.FbAliasingBiasKey
                    or Emulators.Rpcs3.Constants.OptionConstants.DriverRecoveryTimeoutKey)
                    tab = EmulationMachineTab.Video;
            }
            if (option.Key == Emulators.Pcee2.Constants.OptionConstants.CheatsKey
                || option.Key == Emulators.Pcsx2.Constants.OptionConstants.EnableCheatsKey
                || option.Key == Emulators.Ppsspp.Constants.OptionConstants.CheatsKey)
                tab = EmulationMachineTab.Cheats;
            if (option.Key == Emulators.Pcsx2.Constants.OptionConstants.Dev9EthKey)
                tab = EmulationMachineTab.Ethernet;
            if (configuration.Model == ModelConstants.PlayStation3 && option.Key is
                Emulators.Rpcs3.Constants.OptionConstants.NetworkEnabledKey or Emulators.Rpcs3.Constants.OptionConstants.PsnStatusKey
                or Emulators.Rpcs3.Constants.OptionConstants.UpnpKey or Emulators.Rpcs3.Constants.OptionConstants.DnsKey
                or Emulators.Rpcs3.Constants.OptionConstants.ShowRpcnPopupsKey)
                tab = EmulationMachineTab.Ethernet;
            if (configuration.Model == ModelConstants.PlayStation2 && option.Key.Contains("crosshair", StringComparison.OrdinalIgnoreCase))
                tab = EmulationMachineTab.Controllers;
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
            var enabled = option.Values.FirstOrDefault(value => CoreSettingsConstants.EnabledLabels.Contains(value.Label))
                ?? option.Values.FirstOrDefault(value => value.Value == EmulationSettingConstants.Enabled);
            var disabled = option.Values.FirstOrDefault(value => CoreSettingsConstants.DisabledLabels.Contains(value.Label))
                ?? option.Values.FirstOrDefault(value => value.Value == EmulationSettingConstants.Disabled);
            var toggle = enabled is not null && disabled is not null && (option.Values.Count == 2 || option.Key == Emulators.PcsxRearmed.Constants.OptionConstants.ShowBiosBootlogoKey);
            var selectedValue = configuration.Options?.GetValueOrDefault(option.Key) ?? option.DefaultValue;
            var automaticClock = option.Key == Emulators.PcsxRearmed.Constants.OptionConstants.PsxclockKey;
            var range = automaticClock ? new EmulationSettingsNumericRange(30, 100, 1, " %") : option.NumericRange;
            if (option.Key == Emulators.Pcee2.Constants.OptionConstants.DeinterlaceModeKey)
                range = null;
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
            if (option.Key == Emulators.Pcee2.Constants.OptionConstants.DeinterlaceModeKey && option.NumericRange is not null)
                choices = Enumerable.Range(0, 10).Select(mode => new EmulationSettingsChoice(mode.ToString(CultureInfo.InvariantCulture), "Emulation.Sony.Video.Deinterlace." + mode)).ToArray();
            if (instantaneousChoice) choices = [new("0", "Emulation.Option.Sony.Value.Instantaneous"), .. choices.Where(choice => choice.Id != "0")];
            var sectorCount = option.Key == Emulators.PcsxRearmed.Constants.OptionConstants.CdReadaheadKey
                || option.Key == Emulators.SwanStation.Constants.OptionConstants.CDROMReadaheadSectorsKey;
            if (sectorCount)
            {
                if (range is not null)
                    choices = Enumerable.Range((int)range.Minimum, (int)(range.Maximum - range.Minimum) + 1)
                        .Select(number => new EmulationSettingsChoice(number.ToString(CultureInfo.InvariantCulture), string.Empty, number.ToString(CultureInfo.InvariantCulture))).ToArray();
                choices = choices.Select(choice => choice.Id switch
                {
                    "0" => new EmulationSettingsChoice("0", "Emulation.Option.Sony.Value.Disabled"),
                    "333000" => new EmulationSettingsChoice("333000", "Emulation.Sony.CD.ReadWholeDisc"),
                    _ => choice
                }).ToArray();
            }
            var group = tab != EmulationMachineTab.Video ? string.Empty
                : option.Key.Contains("scanline", StringComparison.OrdinalIgnoreCase)
                    && !option.Key.Contains("pal", StringComparison.OrdinalIgnoreCase) ? "ScanNTSC"
                : option.Key.Contains("scanline", StringComparison.OrdinalIgnoreCase)
                    && option.Key.Contains("pal", StringComparison.OrdinalIgnoreCase) ? "ScanPAL"
                : option.Key is Emulators.Rpcs3.Constants.OptionConstants.ShowTrophyPopupsKey
                    or Emulators.Rpcs3.Constants.OptionConstants.ShowPpuCompilationHintKey
                    or Emulators.Rpcs3.Constants.OptionConstants.ShowShaderCompilationHintKey ? "Notifications"
                : CoreSettingsConstants.IsBasicVideo(option.Key) ? CoreSettingsConstants.DisplayGroup
                : option.Key.Contains(CoreSettingsConstants.GeometryGroup, StringComparison.OrdinalIgnoreCase)
                    ? CoreSettingsConstants.GeometryGroup
                : option.Key.Contains("hd_cache", StringComparison.OrdinalIgnoreCase)
                    || option.Key.Contains("hd_caching", StringComparison.OrdinalIgnoreCase) ? "TextureCache"
                : option.Key.Contains("_hd_", StringComparison.OrdinalIgnoreCase)
                    || new[] { "track_textures", "dump_textures", "replace_textures", "texture_directory", "reduce_palette_range" }
                        .Any(part => option.Key.Contains(part, StringComparison.OrdinalIgnoreCase)) ? "TexturePacks"
                : option.Key.Contains("hw_download", StringComparison.OrdinalIgnoreCase) ? "Transfers"
                : new[] { "filter", "msaa", "supersampl", "super_sampling", "smoothing", "mdec", "_fxaa", "_cas_", "mipmapping" }
                    .Any(part => option.Key.Contains(part, StringComparison.OrdinalIgnoreCase)) ? "ImageFilters"
                : option.Key.Contains("hdr", StringComparison.OrdinalIgnoreCase) ? "HDR"
                : new[] { "color", "dither", "depth", "black_setup", "phase_error", "primaries", "analog", "video_cable", "blending" }
                    .Any(part => option.Key.Contains(part, StringComparison.OrdinalIgnoreCase)) ? "Color"
                : option.Key.Contains(CoreSettingsConstants.TextureGroup, StringComparison.OrdinalIgnoreCase)
                    ? CoreSettingsConstants.TextureGroup
                : CoreSettingsConstants.IsScreenOption(option.Key) ? CoreSettingsConstants.ScreenGroup
                : option.Key.Contains(CoreSettingsConstants.FrameKeyword, StringComparison.OrdinalIgnoreCase) || option.Key.Contains(CoreSettingsConstants.TimingKeyword, StringComparison.OrdinalIgnoreCase) ? CoreSettingsConstants.TimingGroup : string.Empty;
            var blockId = option.Key is Emulators.Pcsx2.Constants.OptionConstants.HintLanguageUnlockKey
                or Emulators.Pcsx2.Constants.OptionConstants.UseExternalGameindexKey
                or Emulators.Pcsx2.Constants.OptionConstants.GameEnhancementsHintKey
                or Emulators.Rpcs3.Constants.OptionConstants.DatabaseOverrideKey
                ? CoreSettingsConstants.OptionsBlock + "GamePatches" + CoreSettingsConstants.CompatibilityGroup
                : tab == EmulationMachineTab.Audio ? CoreSettingsConstants.AudioOptionsBlock
                : tab == EmulationMachineTab.Ram ? SettingsDescriptionFunctionsConstants.Memory
                : tab == EmulationMachineTab.Cpu ? CoreSettingsConstants.IsCpuClock(option.Key)
                    || option.Key == Emulators.Ppsspp.Constants.OptionConstants.LockedCpuSpeedKey
                    ? CoreSettingsConstants.AccelerationBlock
                    : configuration.Model == ModelConstants.PlayStation3
                        ? CoreSettingsConstants.OptionsBlock + Rpcs3CpuGroup(option.Key)
                    : configuration.Model == ModelConstants.PlayStation2 && (option.Key.Contains("_rec_", StringComparison.OrdinalIgnoreCase) || option.Key.Contains("cpu_recompiler", StringComparison.OrdinalIgnoreCase))
                        ? CoreSettingsConstants.OptionsBlock + "Recompilation"
                    : configuration.Model == ModelConstants.PlayStation2 && (option.Key.Contains("vu1", StringComparison.OrdinalIgnoreCase) || option.Key.Contains("mtvu", StringComparison.OrdinalIgnoreCase))
                        ? CoreSettingsConstants.OptionsBlock + "VectorUnits"
                    : configuration.Model == ModelConstants.PlayStation2 && option.Key.Contains("cycle_", StringComparison.OrdinalIgnoreCase)
                        ? CoreSettingsConstants.OptionsBlock + "CpuTiming"
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
                AppliesToControllerIds: configuration.Model == ModelConstants.PlayStation2 && option.Key.Contains("crosshair", StringComparison.OrdinalIgnoreCase)
                    ? [ControllerType.GunCon2.ToString()] : ControllerOptionFunctions.Types(option),
                ControllerPortNumber: ControllerOptionFunctions.PortNumber(option),
                RefreshSettingsOnChange: option.Key.Contains(ControllerOptionConstants.Multitap, StringComparison.OrdinalIgnoreCase)
                    || option.Key == Emulators.Ppsspp.Constants.OptionConstants.PspModelKey || option.GraphicsApiValues is not null,
                IsGlobalControllerOption: option.Key.Contains(ControllerOptionConstants.Multitap, StringComparison.OrdinalIgnoreCase)
                    || option.Key == Emulators.Rpcs3.Constants.OptionConstants.EnterButtonKey,
                AutomaticValue: automaticClock ? Emulators.PcsxRearmed.Constants.OptionConstants.PsxclockAuto : null,
                UseChoiceLabelsForSlider: dimensionChoices || instantaneousChoice || sectorCount,
                ValueUnitResourceKey: sectorCount ? "Visual.SectorsUnit" : null);
        }).ToArray();
        fields = fields.SelectMany(field => ControllerOptionConstants.MultitapModes.TryGetValue(field.Id, out var mode)
            ? Enumerable.Range(1, 2).Select(port => field with
            {
                Id = field.Id + ".port" + port,
                LabelResourceKey = "Emulation.Sony.Multitap.Port" + port,
                Editor = EmulationSettingsEditor.Toggle,
                Value = field.Value == mode.Both || field.Value == (port == 1 ? mode.Port1 : mode.Port2) ? EmulationSettingConstants.Enabled : EmulationSettingConstants.Disabled,
                EnabledValue = EmulationSettingConstants.Enabled, DisabledValue = EmulationSettingConstants.Disabled,
                Choices = null
            }) : new[] { field }).ToArray();
        return fields.GroupBy(field => (field.Tab, field.BlockId)).Select(group => new EmulationSettingsBlock(
            group.Key.BlockId, group.Key.Tab,
            group.Key.BlockId == CoreSettingsConstants.AccelerationBlock ? CoreSettingsConstants.AccelerationTitle
                : group.Key.BlockId.EndsWith("Recompilation", StringComparison.Ordinal) ? "Emulation.Sony.Cpu.Recompilation"
                : group.Key.BlockId.EndsWith("Rpcs3PpuAccuracy", StringComparison.Ordinal) ? "Emulation.Sony.Cpu.PpuAccuracy"
                : group.Key.BlockId.EndsWith("Rpcs3SpuAccuracy", StringComparison.Ordinal) ? "Emulation.Sony.Cpu.SpuAccuracy"
                : group.Key.BlockId.EndsWith("Rpcs3SpuExecution", StringComparison.Ordinal) ? "Emulation.Sony.Cpu.SpuExecution"
                : group.Key.BlockId.EndsWith("Rpcs3Timing", StringComparison.Ordinal) ? "Emulation.Sony.Cpu.Scheduling"
                : group.Key.BlockId.EndsWith("VectorUnits", StringComparison.Ordinal) ? "Emulation.Sony.Cpu.VectorUnits"
                : group.Key.BlockId.EndsWith("CpuTiming", StringComparison.Ordinal) ? "Emulation.Sony.Cpu.Timing"
                : group.Key.BlockId.EndsWith(CoreSettingsConstants.CompatibilityGroup, StringComparison.Ordinal)
                    ? CoreSettingsConstants.CompatibilityTitle
                : group.Key.Tab == EmulationMachineTab.Audio ? CoreSettingsConstants.AudioOptionsTitle
                : group.Key.Tab != EmulationMachineTab.Video ? CoreSettingsConstants.Sections[group.Key.Tab].Title
                : group.Key.BlockId.EndsWith("Notifications", StringComparison.Ordinal) ? "Emulation.Sony.Video.Notifications"
                : group.Key.BlockId.EndsWith("ScanNTSC", StringComparison.Ordinal) ? "Emulation.Sony.Video.NTSC"
                : group.Key.BlockId.EndsWith("ScanPAL", StringComparison.Ordinal) ? "Emulation.Sony.Video.PAL"
                : group.Key.BlockId.EndsWith("HDR", StringComparison.Ordinal) ? "Emulation.Sony.Video.HDR"
                : group.Key.BlockId.EndsWith("Color", StringComparison.Ordinal) ? "Emulation.Sony.Video.Color"
                : group.Key.BlockId.EndsWith("TextureCache", StringComparison.Ordinal) ? "Emulation.Sony.Video.TextureCache"
                : group.Key.BlockId.EndsWith("TexturePacks", StringComparison.Ordinal) ? "Emulation.Sony.Video.TexturePacks"
                : group.Key.BlockId.EndsWith("ImageFilters", StringComparison.Ordinal) ? "Emulation.Sony.Video.ImageFilters"
                : group.Key.BlockId.EndsWith("Transfers", StringComparison.Ordinal) ? "Emulation.Sony.Video.Transfers"
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
        if (model == ModelConstants.PlayStation2 && option.Key == Emulators.Pcee2.Constants.OptionConstants.DitheringKey
            && int.TryParse(value.Value, out var dither))
            return new(value.Value, "Emulation.Sony.Video.Dithering." + dither);
        if (model == ModelConstants.PlayStation2 && option.Key == Emulators.Pcee2.Constants.OptionConstants.EeCycleRateKey)
        {
            var percentage = value.Value switch { "-3" => 50, "-2" => 60, "-1" => 75, "0" => 100, "1" => 130, "2" => 180, "3" => 300, _ => 100 };
            return new(value.Value, string.Empty, percentage.ToString(CultureInfo.InvariantCulture) + " %");
        }
        if (model == ModelConstants.PlayStation2 && option.Key == Emulators.Pcee2.Constants.OptionConstants.BlendingAccuracyKey)
            return new(value.Value, "Emulation.Sony.Video.Blending." + value.Value);
        if (model == ModelConstants.PlayStation2 && option.Key == Emulators.Pcee2.Constants.OptionConstants.EeCycleSkipKey)
            return new(value.Value, "Emulation.Sony.Cpu.CycleSkip." + value.Value);
        if (option.Key.Contains("upscale_multiplier", StringComparison.OrdinalIgnoreCase))
            return new(value.Value, string.Empty, value.Value + "×");
        if (option.Key.Contains("cd_fastload", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(value.Value.TrimEnd('x'), out var speed))
            return new(value.Value, string.Empty, speed + "×");
        if (model == ModelConstants.Psp && option.Key == Emulators.Ppsspp.Constants.OptionConstants.InternalResolutionKey)
        {
            var dimensions = value.Value.Split('x');
            if (dimensions.Length == 2 && int.TryParse(dimensions[0], out var width)
                && int.TryParse(dimensions[1], out var height) && width > 0 && height > 0)
                return new(value.Value, string.Empty, $"{width / 480}× ({width} × {height} px)");
        }
        if (option.Key.Contains("internal_resolution", StringComparison.OrdinalIgnoreCase)
            || option.Key.Contains("GPU_ResolutionScale", StringComparison.OrdinalIgnoreCase))
        {
            var number = new string(value.Value.TakeWhile(char.IsDigit).ToArray());
            if (int.TryParse(number, out var scale) && scale > 0)
            {
                var width = model == ModelConstants.Psp ? 480 : 320;
                var height = model == ModelConstants.Psp ? 272 : 240;
                return new(value.Value, string.Empty, $"{scale}× ({width * scale} × {height * scale} px)");
            }
        }
        return value.Label.StartsWith("Emulation.", StringComparison.Ordinal)
            ? new(value.Value, value.Label) : new(value.Value, string.Empty, value.Label);
    }

    private static string Rpcs3CpuGroup(string key) => key switch
    {
        Rpcs3Options.SpuXfloatAccuracyKey or Rpcs3Options.SpuBlockSizeKey
            or Rpcs3Options.SpuAccurateDmaKey or Rpcs3Options.SpuAccurateReservationsKey
            or Rpcs3Options.DisableGetllarSpinOptKey or Rpcs3Options.SpuCacheLineStoresKey => "Rpcs3SpuAccuracy",
        Rpcs3Options.SetDazFtzKey or Rpcs3Options.PpuSetFpccKey or Rpcs3Options.PpuSetSatBitKey
            or Rpcs3Options.PpuNjModeKey or Rpcs3Options.PpuAccurateVectorNanKey
            or Rpcs3Options.AccuratePpu128Key or Rpcs3Options.AccurateDfmaKey => "Rpcs3PpuAccuracy",
        Rpcs3Options.SleepTimersAccuracyKey or Rpcs3Options.ClocksScaleKey
            or Rpcs3Options.ThreadSchedulerKey or Rpcs3Options.PpuThreadsKey
            or Rpcs3Options.MaxPreemptCountKey => "Rpcs3Timing",
        _ when key.Contains("decoder", StringComparison.OrdinalIgnoreCase)
            || key.Contains("llvm", StringComparison.OrdinalIgnoreCase) => "Recompilation",
        _ when key.Contains("spu", StringComparison.OrdinalIgnoreCase)
            || key.Contains("mfc", StringComparison.OrdinalIgnoreCase) => "Rpcs3SpuExecution",
        _ => "Compatibility"
    };

    internal static string NativeNumber(CoreOption option, string selected)
    {
        var number = ReadNumber(option, selected);
        return number == 0 && option.NativeZeroValue is not null ? option.NativeZeroValue : number.ToString(option.NativeNumericFormat ?? CoreSettingsConstants.NumericFormat, CultureInfo.InvariantCulture) + option.NumericRange!.UnitSuffix;
    }
}
