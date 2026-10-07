using GWGUI.Emulation.Nec.Emulators.BeetlePceFast.Constants;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Nec.Emulators.BeetleSgx.Constants;
using static GWGUI.Emulation.Nec.Common.Machines.Common.Functions.SettingsDescriptionFunctions;

namespace GWGUI.Emulation.Nec.Emulators.BeetleSgx.Functions;

internal static class BeetleSgxSettingsDescriptionFunctions
{
    internal static EmulationSettingsField[] Video(IReadOnlyDictionary<string, string> options) =>
    [
        Select(BeetleSgxSettingsConstants.HorizontalOverscan, EmulationMachineTab.Video,
            SettingsDescriptionFunctionsConstants.Video,
            BeetlePceFastSettingsConstants.ResourceHorizontalWidth,
            options.GetValueOrDefault(BeetleSgxSettingsConstants.HorizontalOverscan,
                BeetleSgxSettingsConstants.DefaultHorizontalOverscan),
            Enumerable.Range(SettingsDescriptionFunctionsConstants.FirstChoice,
                BeetleSgxSettingsConstants.HorizontalOverscanCount)
                .Select(index => BeetleSgxSettingsConstants.HorizontalOverscanStart
                    + index * BeetleSgxSettingsConstants.HorizontalOverscanStep)
                .Select(value => Invariant(value.ToString(), value.ToString(), value))),
        Select(BeetleSgxSettingsConstants.AspectRatio, EmulationMachineTab.Video,
            SettingsDescriptionFunctionsConstants.Video,
            BeetleSgxSettingsConstants.ResourceAspectRatio,
            options.GetValueOrDefault(BeetleSgxSettingsConstants.AspectRatio,
                BeetleSgxSettingsConstants.AspectAuto),
            [new(BeetleSgxSettingsConstants.AspectAuto, BeetleSgxSettingsConstants.ResourceAuto),
                Invariant(BeetleSgxSettingsConstants.AspectSixFive,
                    BeetleSgxSettingsConstants.AspectSixFive),
                Invariant(BeetleSgxSettingsConstants.AspectFourThree,
                    BeetleSgxSettingsConstants.AspectFourThree)]),
        Select(BeetleSgxSettingsConstants.InitialScanline, EmulationMachineTab.Video,
            SettingsDescriptionFunctionsConstants.Video,
            SettingsDescriptionFunctionsConstants.ResourceInitialScanline,
            options.GetValueOrDefault(BeetleSgxSettingsConstants.InitialScanline,
                BeetleSgxSettingsConstants.DefaultInitialScanline),
            Enumerable.Range(BeetleSgxSettingsConstants.InitialScanlineStart,
                BeetleSgxSettingsConstants.InitialScanlineCount)
                .Select(value => Invariant(value.ToString(), value.ToString(), value))),
        Select(BeetleSgxSettingsConstants.LastScanline, EmulationMachineTab.Video,
            SettingsDescriptionFunctionsConstants.Video,
            SettingsDescriptionFunctionsConstants.ResourceLastScanline,
            options.GetValueOrDefault(BeetleSgxSettingsConstants.LastScanline,
                BeetleSgxSettingsConstants.DefaultLastScanline),
            Enumerable.Range(BeetleSgxSettingsConstants.LastScanlineStart,
                BeetleSgxSettingsConstants.LastScanlineCount)
                .Select(value => Invariant(value.ToString(), value.ToString(), value))),
        Toggle(BeetleSgxSettingsConstants.NoSpriteLimit, EmulationMachineTab.Video,
            SettingsDescriptionFunctionsConstants.Video,
            SettingsDescriptionFunctionsConstants.ResourceNoSpriteLimit,
            options.GetValueOrDefault(BeetleSgxSettingsConstants.NoSpriteLimit,
                SettingsDescriptionFunctionsConstants.Disabled)
                == SettingsDescriptionFunctionsConstants.Enabled)
    ];

    internal static void AddCdAudio(List<EmulationSettingsField> audio,
        IReadOnlyDictionary<string, string> options)
    {
        foreach (var (id, resource) in new[]
        {
            (BeetleSgxSettingsConstants.CdVolume, BeetlePceFastSettingsConstants.ResourceCdVolume),
            (BeetleSgxSettingsConstants.AdpcmVolume,
                BeetlePceFastSettingsConstants.ResourceAdpcmVolume),
            (BeetleSgxSettingsConstants.CdPsgVolume,
                BeetlePceFastSettingsConstants.ResourceCdPsgVolume)
        })
            audio.Add(Select(id, EmulationMachineTab.Audio,
                SettingsDescriptionFunctionsConstants.Audio, resource,
                options.GetValueOrDefault(id, BeetleSgxSettingsConstants.DefaultVolume),
                Enumerable.Range(BeetleSgxSettingsConstants.VolumeStart,
                    BeetleSgxSettingsConstants.VolumeCount)
                    .Select(index => index * BeetleSgxSettingsConstants.VolumeStep)
                    .Select(value => Invariant(value.ToString(),
                        $"{value}{SettingsDescriptionFunctionsConstants.PercentUnit}", value))));
    }

    internal static EmulationSettingsField Mouse(IReadOnlyDictionary<string, string> options) =>
        Select(BeetleSgxSettingsConstants.MouseSensitivity, EmulationMachineTab.Mouse,
            SettingsDescriptionFunctionsConstants.Mouse,
            SettingsDescriptionFunctionsConstants.ResourceMouseSensitivity,
            options.GetValueOrDefault(BeetleSgxSettingsConstants.MouseSensitivity,
                BeetleSgxSettingsConstants.DefaultMouseSensitivity),
            Enumerable.Range(BeetleSgxSettingsConstants.MouseSensitivityStart,
                BeetleSgxSettingsConstants.MouseSensitivityCount)
                .Select(value => (value /
                    (decimal)SettingsDescriptionFunctionsConstants.MouseSensitivityDivisor)
                    .ToString(SettingsDescriptionFunctionsConstants.DecimalFormat,
                        System.Globalization.CultureInfo.InvariantCulture))
                .Select(value => Invariant(value, value)));
}
