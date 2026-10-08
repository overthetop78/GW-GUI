using GWGUI.Emulation.Nec.Emulators.BeetlePceFast.Constants;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Nec.Common.Machines.Common.Functions;
using static GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions.CoreSettingsDescriptionFunctions;

namespace GWGUI.Emulation.Nec.Emulators.BeetlePceFast.Functions;

internal static class BeetlePceFastSettingsDescriptionFunctions
{
    internal static EmulationSettingsField[] Video(IReadOnlyDictionary<string, string> options) =>
        [Select(SettingsConstants.PcePalette, EmulationMachineTab.Video,
                SettingsDescriptionFunctionsConstants.Video,
                BeetlePceFastSettingsConstants.ResourcePalette,
                options.GetValueOrDefault(SettingsConstants.PcePalette,
                    BeetlePceFastSettingsConstants.PaletteRgb),
                [new(BeetlePceFastSettingsConstants.PaletteRgb,
                    BeetlePceFastSettingsConstants.ResourcePaletteRgb),
                 new(BeetlePceFastSettingsConstants.PaletteComposite,
                    BeetlePceFastSettingsConstants.ResourcePaletteComposite)]),
               Select(SettingsConstants.PceHorizontalOverscan, EmulationMachineTab.Video,
                SettingsDescriptionFunctionsConstants.Video,
                BeetlePceFastSettingsConstants.ResourceHorizontalWidth,
                options.GetValueOrDefault(SettingsConstants.PceHorizontalOverscan,
                    BeetlePceFastSettingsConstants.DefaultPceHorizontalWidth),
                Enumerable.Range(BeetlePceFastSettingsConstants.PceHorizontalWidthStart,
                    BeetlePceFastSettingsConstants.PceHorizontalWidthCount)
                    .Select(value => Invariant(
                        (value * BeetlePceFastSettingsConstants.HorizontalWidthStep).ToString(),
                        (value * BeetlePceFastSettingsConstants.HorizontalWidthStep).ToString(),
                        value * BeetlePceFastSettingsConstants.HorizontalWidthStep))),
               Select(SettingsConstants.PceFrameSkip, EmulationMachineTab.Video,
                SettingsDescriptionFunctionsConstants.Video,
                BeetlePceFastSettingsConstants.ResourceFrameSkip,
                options.GetValueOrDefault(SettingsConstants.PceFrameSkip,
                    SettingsDescriptionFunctionsConstants.Disabled),
                [new(SettingsDescriptionFunctionsConstants.Disabled,
                    BeetlePceFastSettingsConstants.ResourceFrameSkipDisabled),
                 new(BeetlePceFastSettingsConstants.FrameSkipAuto,
                    BeetlePceFastSettingsConstants.ResourceFrameSkipAuto),
                 new(BeetlePceFastSettingsConstants.FrameSkipManual,
                    BeetlePceFastSettingsConstants.ResourceFrameSkipManual)]),
               Select(SettingsConstants.PceFrameSkipThreshold, EmulationMachineTab.Video,
                SettingsDescriptionFunctionsConstants.Video,
                BeetlePceFastSettingsConstants.ResourceFrameSkipThreshold,
                options.GetValueOrDefault(SettingsConstants.PceFrameSkipThreshold,
                    BeetlePceFastSettingsConstants.DefaultPceFrameSkipThreshold),
                Enumerable.Range(SettingsDescriptionFunctionsConstants.FirstChoice,
                    BeetlePceFastSettingsConstants.FrameSkipThresholdCount)
                    .Select(value => Invariant(
                        (BeetlePceFastSettingsConstants.FrameSkipThresholdStart
                            + value * BeetlePceFastSettingsConstants.FrameSkipThresholdStep).ToString(),
                        $"{BeetlePceFastSettingsConstants.FrameSkipThresholdStart
                            + value * BeetlePceFastSettingsConstants.FrameSkipThresholdStep}{SettingsDescriptionFunctionsConstants.PercentUnit}",
                        BeetlePceFastSettingsConstants.FrameSkipThresholdStart
                            + value * BeetlePceFastSettingsConstants.FrameSkipThresholdStep))),
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
                    BeetlePceFastSettingsConstants.DefaultPceInitialScanline),
                Enumerable.Range(BeetlePceFastSettingsConstants.PceFirstScanlineStart,
                    BeetlePceFastSettingsConstants.PceFirstScanlineCount)
                    .Select(value => Invariant(value.ToString(), value.ToString(), value))),
               Select(SettingsConstants.PceLastScanline, EmulationMachineTab.Video,
                SettingsDescriptionFunctionsConstants.Video,
                SettingsDescriptionFunctionsConstants.ResourceLastScanline,
                options.GetValueOrDefault(SettingsConstants.PceLastScanline,
                    BeetlePceFastSettingsConstants.DefaultPceLastScanline),
                Enumerable.Range(BeetlePceFastSettingsConstants.PceLastScanlineStart,
                    BeetlePceFastSettingsConstants.PceLastScanlineCount)
                    .Select(value => Invariant(value.ToString(), value.ToString(), value)))];

    internal static void AddCdAudio(List<EmulationSettingsField> audio,
        IReadOnlyDictionary<string, string> options)
    {
            foreach (var (id, label) in new[]
            {
                (SettingsConstants.PceCdVolume, BeetlePceFastSettingsConstants.ResourceCdVolume),
                (SettingsConstants.PceAdpcmVolume, BeetlePceFastSettingsConstants.ResourceAdpcmVolume),
                (SettingsConstants.PceCdPsgVolume, BeetlePceFastSettingsConstants.ResourceCdPsgVolume)
            })
                audio.Add(Select(id, EmulationMachineTab.Audio,
                    SettingsDescriptionFunctionsConstants.Audio, label,
                    options.GetValueOrDefault(id, BeetlePceFastSettingsConstants.DefaultVolume),
                    Enumerable.Range(BeetlePceFastSettingsConstants.VolumeStart,
                        BeetlePceFastSettingsConstants.VolumeCount)
                        .Select(value => Invariant(
                            (value * BeetlePceFastSettingsConstants.VolumeStep).ToString(),
                            $"{value * BeetlePceFastSettingsConstants.VolumeStep}{SettingsDescriptionFunctionsConstants.PercentUnit}",
                            value * BeetlePceFastSettingsConstants.VolumeStep))));
            audio.Add(Toggle(SettingsConstants.PceAdpcmLowPass, EmulationMachineTab.Audio,
                SettingsDescriptionFunctionsConstants.Audio,
                BeetlePceFastSettingsConstants.ResourceAdpcmLowPass,
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
            Enumerable.Range(BeetlePceFastSettingsConstants.PceMouseSensitivityStart,
                    BeetlePceFastSettingsConstants.PceMouseSensitivityCount)
                .Select(value => Invariant(
                    (value / (decimal)SettingsDescriptionFunctionsConstants.MouseSensitivityDivisor)
                        .ToString(SettingsDescriptionFunctionsConstants.DecimalFormat,
                            System.Globalization.CultureInfo.InvariantCulture),
                    (value / (decimal)SettingsDescriptionFunctionsConstants.MouseSensitivityDivisor)
                        .ToString(SettingsDescriptionFunctionsConstants.DecimalFormat,
                            System.Globalization.CultureInfo.InvariantCulture))));
}
