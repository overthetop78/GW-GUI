using GWGUI.Emulation.Nec.Emulators.BeetlePce.Constants;
using GWGUI.Emulation.Nec.Common.Machines.Common.Functions;
using static GWGUI.Emulation.Nec.Common.Machines.Common.Functions.SettingsDescriptionFunctions;

namespace GWGUI.Emulation.Nec.Emulators.BeetlePce.Functions;

internal static class BeetlePceSettingsDescriptionFunctions
{
    internal static EmulationSettingsField[] Video(IReadOnlyDictionary<string, string> options) =>
        [Select(SettingsConstants.PcePalette, EmulationMachineTab.Video,
                SettingsDescriptionFunctionsConstants.Video,
                BeetlePceSettingsConstants.ResourcePalette,
                options.GetValueOrDefault(SettingsConstants.PcePalette,
                    BeetlePceSettingsConstants.PaletteRgb),
                [new(BeetlePceSettingsConstants.PaletteRgb,
                    BeetlePceSettingsConstants.ResourcePaletteRgb),
                 new(BeetlePceSettingsConstants.PaletteComposite,
                    BeetlePceSettingsConstants.ResourcePaletteComposite)]),
               Select(SettingsConstants.PceHorizontalOverscan, EmulationMachineTab.Video,
                SettingsDescriptionFunctionsConstants.Video,
                BeetlePceSettingsConstants.ResourceHorizontalWidth,
                options.GetValueOrDefault(SettingsConstants.PceHorizontalOverscan,
                    BeetlePceSettingsConstants.DefaultPceHorizontalWidth),
                Enumerable.Range(BeetlePceSettingsConstants.PceHorizontalWidthStart,
                    BeetlePceSettingsConstants.PceHorizontalWidthCount)
                    .Select(value => Invariant(
                        (value * BeetlePceSettingsConstants.HorizontalWidthStep).ToString(),
                        (value * BeetlePceSettingsConstants.HorizontalWidthStep).ToString(),
                        value * BeetlePceSettingsConstants.HorizontalWidthStep))),
               Select(SettingsConstants.PceFrameSkip, EmulationMachineTab.Video,
                SettingsDescriptionFunctionsConstants.Video,
                BeetlePceSettingsConstants.ResourceFrameSkip,
                options.GetValueOrDefault(SettingsConstants.PceFrameSkip,
                    SettingsDescriptionFunctionsConstants.Disabled),
                [new(SettingsDescriptionFunctionsConstants.Disabled,
                    BeetlePceSettingsConstants.ResourceFrameSkipDisabled),
                 new(BeetlePceSettingsConstants.FrameSkipAuto,
                    BeetlePceSettingsConstants.ResourceFrameSkipAuto),
                 new(BeetlePceSettingsConstants.FrameSkipManual,
                    BeetlePceSettingsConstants.ResourceFrameSkipManual)]),
               Select(SettingsConstants.PceFrameSkipThreshold, EmulationMachineTab.Video,
                SettingsDescriptionFunctionsConstants.Video,
                BeetlePceSettingsConstants.ResourceFrameSkipThreshold,
                options.GetValueOrDefault(SettingsConstants.PceFrameSkipThreshold,
                    BeetlePceSettingsConstants.DefaultPceFrameSkipThreshold),
                Enumerable.Range(SettingsDescriptionFunctionsConstants.FirstChoice,
                    BeetlePceSettingsConstants.FrameSkipThresholdCount)
                    .Select(value => Invariant(
                        (BeetlePceSettingsConstants.FrameSkipThresholdStart
                            + value * BeetlePceSettingsConstants.FrameSkipThresholdStep).ToString(),
                        $"{BeetlePceSettingsConstants.FrameSkipThresholdStart
                            + value * BeetlePceSettingsConstants.FrameSkipThresholdStep}{SettingsDescriptionFunctionsConstants.PercentUnit}",
                        BeetlePceSettingsConstants.FrameSkipThresholdStart
                            + value * BeetlePceSettingsConstants.FrameSkipThresholdStep))),
               Toggle(SettingsConstants.PceNoSpriteLimit, EmulationMachineTab.Video,
                SettingsDescriptionFunctionsConstants.Video,
                SettingsDescriptionFunctionsConstants.ResourceNoSpriteLimit,
                options.GetValueOrDefault(SettingsConstants.PceNoSpriteLimit,
                    SettingsDescriptionFunctionsConstants.Disabled)
                    == SettingsDescriptionFunctionsConstants.Enabled),
               Select(SettingsConstants.PceInitialScanline, EmulationMachineTab.Video,
                SettingsDescriptionFunctionsConstants.Video,
                SettingsDescriptionFunctionsConstants.ResourceInitialScanline,
                options.GetValueOrDefault(SettingsConstants.PceInitialScanline,
                    BeetlePceSettingsConstants.DefaultPceInitialScanline),
                Enumerable.Range(BeetlePceSettingsConstants.PceFirstScanlineStart,
                    BeetlePceSettingsConstants.PceFirstScanlineCount)
                    .Select(value => Invariant(value.ToString(), value.ToString(), value))),
               Select(SettingsConstants.PceLastScanline, EmulationMachineTab.Video,
                SettingsDescriptionFunctionsConstants.Video,
                SettingsDescriptionFunctionsConstants.ResourceLastScanline,
                options.GetValueOrDefault(SettingsConstants.PceLastScanline,
                    BeetlePceSettingsConstants.DefaultPceLastScanline),
                Enumerable.Range(BeetlePceSettingsConstants.PceLastScanlineStart,
                    BeetlePceSettingsConstants.PceLastScanlineCount)
                    .Select(value => Invariant(value.ToString(), value.ToString(), value)))];

    internal static void AddCdAudio(List<EmulationSettingsField> audio,
        IReadOnlyDictionary<string, string> options)
    {
            foreach (var (id, label) in new[]
            {
                (SettingsConstants.PceCdVolume, BeetlePceSettingsConstants.ResourceCdVolume),
                (SettingsConstants.PceAdpcmVolume, BeetlePceSettingsConstants.ResourceAdpcmVolume),
                (SettingsConstants.PceCdPsgVolume, BeetlePceSettingsConstants.ResourceCdPsgVolume)
            })
                audio.Add(Select(id, EmulationMachineTab.Audio,
                    SettingsDescriptionFunctionsConstants.Audio, label,
                    options.GetValueOrDefault(id, BeetlePceSettingsConstants.DefaultVolume),
                    Enumerable.Range(BeetlePceSettingsConstants.VolumeStart,
                        BeetlePceSettingsConstants.VolumeCount)
                        .Select(value => Invariant(
                            (value * BeetlePceSettingsConstants.VolumeStep).ToString(),
                            $"{value * BeetlePceSettingsConstants.VolumeStep}{SettingsDescriptionFunctionsConstants.PercentUnit}",
                            value * BeetlePceSettingsConstants.VolumeStep))));
            audio.Add(Toggle(SettingsConstants.PceAdpcmLowPass, EmulationMachineTab.Audio,
                SettingsDescriptionFunctionsConstants.Audio,
                BeetlePceSettingsConstants.ResourceAdpcmLowPass,
                options.GetValueOrDefault(SettingsConstants.PceAdpcmLowPass,
                    SettingsDescriptionFunctionsConstants.Disabled)
                    == SettingsDescriptionFunctionsConstants.Enabled));
        
    }

    internal static EmulationSettingsField Mouse(IReadOnlyDictionary<string, string> options) =>
        Select(SettingsConstants.PceMouseSensitivity, EmulationMachineTab.Mouse,
            SettingsDescriptionFunctionsConstants.Mouse,
            SettingsDescriptionFunctionsConstants.ResourceMouseSensitivity,
            options.GetValueOrDefault(SettingsConstants.PceMouseSensitivity,
                SettingsDescriptionFunctionsConstants.DefaultMouseSensitivity),
            Enumerable.Range(BeetlePceSettingsConstants.PceMouseSensitivityStart,
                    BeetlePceSettingsConstants.PceMouseSensitivityCount)
                .Select(value => Invariant(
                    (value / (decimal)SettingsDescriptionFunctionsConstants.MouseSensitivityDivisor)
                        .ToString(SettingsDescriptionFunctionsConstants.DecimalFormat,
                            System.Globalization.CultureInfo.InvariantCulture),
                    (value / (decimal)SettingsDescriptionFunctionsConstants.MouseSensitivityDivisor)
                        .ToString(SettingsDescriptionFunctionsConstants.DecimalFormat,
                            System.Globalization.CultureInfo.InvariantCulture))));
}
