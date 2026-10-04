using GWGUI.Emulation.Nec.Emulators.BeetlePcfx.Constants;
using GWGUI.Emulation.Nec.Common.Machines.Common.Functions;
using static GWGUI.Emulation.Nec.Common.Machines.Common.Functions.SettingsDescriptionFunctions;

namespace GWGUI.Emulation.Nec.Emulators.BeetlePcfx.Functions;

internal static class BeetlePcfxSettingsDescriptionFunctions
{
    internal static EmulationSettingsField[] Video(IReadOnlyDictionary<string, string> options) =>
        [Select(SettingsConstants.PcfxWidth, EmulationMachineTab.Video,
                SettingsDescriptionFunctionsConstants.Video,
                BeetlePcfxSettingsConstants.ResourceDotClockWidth,
                options.GetValueOrDefault(SettingsConstants.PcfxWidth,
                    BeetlePcfxSettingsConstants.DefaultPcfxWidth),
                InvariantChoices(BeetlePcfxSettingsConstants.PcfxWidth256,
                    BeetlePcfxSettingsConstants.PcfxWidth341,
                    BeetlePcfxSettingsConstants.DefaultPcfxWidth)),
               Toggle(SettingsConstants.PcfxChromaInterpolation, EmulationMachineTab.Video,
                SettingsDescriptionFunctionsConstants.Video,
                BeetlePcfxSettingsConstants.ResourceChromaInterpolation,
                options.GetValueOrDefault(SettingsConstants.PcfxChromaInterpolation,
                    SettingsDescriptionFunctionsConstants.Disabled)
                    == SettingsDescriptionFunctionsConstants.Enabled),
               Toggle(SettingsConstants.PcfxNoSpriteLimit, EmulationMachineTab.Video,
                SettingsDescriptionFunctionsConstants.Video,
                SettingsDescriptionFunctionsConstants.ResourceNoSpriteLimit,
                options.GetValueOrDefault(SettingsConstants.PcfxNoSpriteLimit,
                    SettingsDescriptionFunctionsConstants.Disabled)
                    == SettingsDescriptionFunctionsConstants.Enabled),
               Select(SettingsConstants.PcfxInitialScanline, EmulationMachineTab.Video,
                SettingsDescriptionFunctionsConstants.Video,
                SettingsDescriptionFunctionsConstants.ResourceInitialScanline,
                options.GetValueOrDefault(SettingsConstants.PcfxInitialScanline,
                    BeetlePcfxSettingsConstants.DefaultPcfxInitialScanline),
                Enumerable.Range(BeetlePcfxSettingsConstants.PcfxFirstScanlineStart,
                    BeetlePcfxSettingsConstants.PcfxFirstScanlineCount)
                    .Select(value => Invariant(value.ToString(), value.ToString(), value))),
               Select(SettingsConstants.PcfxLastScanline, EmulationMachineTab.Video,
                SettingsDescriptionFunctionsConstants.Video,
                SettingsDescriptionFunctionsConstants.ResourceLastScanline,
                options.GetValueOrDefault(SettingsConstants.PcfxLastScanline,
                    BeetlePcfxSettingsConstants.DefaultPcfxLastScanline),
                Enumerable.Range(BeetlePcfxSettingsConstants.PcfxLastScanlineStart,
                    BeetlePcfxSettingsConstants.PcfxLastScanlineCount)
                    .Select(value => Invariant(value.ToString(), value.ToString(), value)))];

    internal static void AddAudio(List<EmulationSettingsField> audio,
        IReadOnlyDictionary<string, string> options)
    {
            audio.Add(Select(SettingsConstants.PcfxSoundQuality, EmulationMachineTab.Audio,
                SettingsDescriptionFunctionsConstants.Audio,
                BeetlePcfxSettingsConstants.ResourceSoundQuality,
                options.GetValueOrDefault(SettingsConstants.PcfxSoundQuality,
                    BeetlePcfxSettingsConstants.DefaultSoundQuality),
                InvariantChoices(SettingsDescriptionFunctionsConstants.Zero,
                    SettingsDescriptionFunctionsConstants.One,
                    SettingsDescriptionFunctionsConstants.Two,
                    SettingsDescriptionFunctionsConstants.Three,
                    SettingsDescriptionFunctionsConstants.Four,
                    SettingsDescriptionFunctionsConstants.Five)));
            audio.Add(Toggle(SettingsConstants.PcfxSuppressClicks, EmulationMachineTab.Audio,
                SettingsDescriptionFunctionsConstants.Audio,
                BeetlePcfxSettingsConstants.ResourceSuppressClicks,
                options.GetValueOrDefault(SettingsConstants.PcfxSuppressClicks,
                    SettingsDescriptionFunctionsConstants.Enabled)
                    == SettingsDescriptionFunctionsConstants.Enabled));
            audio.Add(Toggle(SettingsConstants.PcfxBuggyCodec, EmulationMachineTab.Audio,
                SettingsDescriptionFunctionsConstants.Audio,
                BeetlePcfxSettingsConstants.ResourceBuggyCodec,
                options.GetValueOrDefault(SettingsConstants.PcfxBuggyCodec,
                    SettingsDescriptionFunctionsConstants.Disabled)
                    == SettingsDescriptionFunctionsConstants.Enabled));
        
    }

    internal static EmulationSettingsField Mouse(IReadOnlyDictionary<string, string> options) =>
        Select(SettingsConstants.PcfxMouseSensitivity, EmulationMachineTab.Mouse,
            SettingsDescriptionFunctionsConstants.Mouse,
            SettingsDescriptionFunctionsConstants.ResourceMouseSensitivity,
            options.GetValueOrDefault(SettingsConstants.PcfxMouseSensitivity,
                SettingsDescriptionFunctionsConstants.DefaultMouseSensitivity),
            Enumerable.Range(BeetlePcfxSettingsConstants.PcfxMouseSensitivityStart,
                    BeetlePcfxSettingsConstants.PcfxMouseSensitivityCount)
                .Select(value => Invariant(
                    (value / (decimal)SettingsDescriptionFunctionsConstants.MouseSensitivityDivisor)
                        .ToString(SettingsDescriptionFunctionsConstants.DecimalFormat,
                            System.Globalization.CultureInfo.InvariantCulture),
                    (value / (decimal)SettingsDescriptionFunctionsConstants.MouseSensitivityDivisor)
                        .ToString(SettingsDescriptionFunctionsConstants.DecimalFormat,
                            System.Globalization.CultureInfo.InvariantCulture))));
}
