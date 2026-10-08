using GWGUI.Emulation.Nec.Common.Machines.LaserActive.Constants;
using GWGUI.Emulation.Nec.Common.Machines.SuperGrafx.Constants;
using GWGUI.Emulation.Nec.Emulators.BeetlePceFast.Constants;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Nec.Emulators.Geargrafx.Constants;
using static GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions.CoreSettingsDescriptionFunctions;

namespace GWGUI.Emulation.Nec.Emulators.Geargrafx.Functions;

internal static class GeargrafxSettingsDescriptionFunctions
{
    internal static EmulationSettingsField[] Video(string modelId,
        IReadOnlyDictionary<string, string> options)
    {
        var laserActive = modelId == LaserActiveMachineConstants.Id;
        var aspect = laserActive ? GeargrafxSettingsConstants.LaserActiveAspectRatio
            : GeargrafxSettingsConstants.AspectRatio;
        var aspectChoices = laserActive
            ? InvariantChoices(GeargrafxSettingsConstants.AspectFourThree,
                GeargrafxSettingsConstants.AspectOneOne,
                GeargrafxSettingsConstants.AspectSixteenNine,
                GeargrafxSettingsConstants.AspectSixteenTen)
            : InvariantChoices(GeargrafxSettingsConstants.AspectOneOne,
                GeargrafxSettingsConstants.AspectFourThree,
                GeargrafxSettingsConstants.AspectSixFive,
                GeargrafxSettingsConstants.AspectSixteenNine,
                GeargrafxSettingsConstants.AspectSixteenTen);
        var result = new List<EmulationSettingsField>
        {
            Select(aspect, EmulationMachineTab.Video, SettingsDescriptionFunctionsConstants.Video,
                GeargrafxSettingsConstants.ResourceAspectRatio,
                options.GetValueOrDefault(aspect, GeargrafxSettingsConstants.AspectFourThree),
                aspectChoices),
            Select(GeargrafxSettingsConstants.Overscan, EmulationMachineTab.Video,
                SettingsDescriptionFunctionsConstants.Video,
                GeargrafxSettingsConstants.ResourceOverscan,
                options.GetValueOrDefault(GeargrafxSettingsConstants.Overscan,
                    GeargrafxSettingsConstants.Disabled),
                [new(GeargrafxSettingsConstants.Disabled,
                    GeargrafxSettingsConstants.ResourceOverscanDisabled),
                    new(GeargrafxSettingsConstants.Enabled,
                        GeargrafxSettingsConstants.ResourceOverscanEnabled)])
        };
        if (laserActive)
        {
            result.Add(Select(GeargrafxSettingsConstants.LaserActiveFraming,
                EmulationMachineTab.Video, SettingsDescriptionFunctionsConstants.Video,
                GeargrafxSettingsConstants.ResourceFraming,
                options.GetValueOrDefault(GeargrafxSettingsConstants.LaserActiveFraming,
                    GeargrafxSettingsConstants.FramingCropped),
                [new(GeargrafxSettingsConstants.FramingCropped,
                    GeargrafxSettingsConstants.ResourceFramingCropped),
                    new(GeargrafxSettingsConstants.FramingFull,
                        GeargrafxSettingsConstants.ResourceFramingFull)]));
            result.Add(Select(GeargrafxSettingsConstants.LaserActiveHorizontalFraming,
                EmulationMachineTab.Video, SettingsDescriptionFunctionsConstants.Video,
                GeargrafxSettingsConstants.ResourceHorizontalFraming,
                options.GetValueOrDefault(GeargrafxSettingsConstants.LaserActiveHorizontalFraming,
                    GeargrafxSettingsConstants.HorizontalCropped),
                [new(GeargrafxSettingsConstants.HorizontalCropped,
                    GeargrafxSettingsConstants.ResourceHorizontalCropped),
                    new(GeargrafxSettingsConstants.HorizontalFull,
                        GeargrafxSettingsConstants.ResourceHorizontalFull)]));
        }
        return result.ToArray();
    }

    internal static void AddAudio(List<EmulationSettingsField> audio,
        IReadOnlyDictionary<string, string> options)
    {
        foreach (var (id, resource) in new[]
        {
            (GeargrafxSettingsConstants.PsgVolume, BeetlePceFastSettingsConstants.ResourceCdPsgVolume),
            (GeargrafxSettingsConstants.CdVolume, BeetlePceFastSettingsConstants.ResourceCdVolume),
            (GeargrafxSettingsConstants.AdpcmVolume, BeetlePceFastSettingsConstants.ResourceAdpcmVolume)
        })
            audio.Add(Select(id, EmulationMachineTab.Audio,
                SettingsDescriptionFunctionsConstants.Audio, resource,
                options.GetValueOrDefault(id, GeargrafxSettingsConstants.DefaultVolume),
                Enumerable.Range(GeargrafxSettingsConstants.VolumeStart,
                    GeargrafxSettingsConstants.VolumeCount)
                    .Select(index => index * GeargrafxSettingsConstants.VolumeStep)
                    .Select(value => Invariant(value.ToString(),
                        $"{value}{SettingsDescriptionFunctionsConstants.PercentUnit}", value))));
    }

    internal static EmulationSettingsField Mouse(IReadOnlyDictionary<string, string> options) =>
        Select(GeargrafxSettingsConstants.MouseSensitivity, EmulationMachineTab.Mouse,
            SettingsDescriptionFunctionsConstants.Mouse,
            SettingsDescriptionFunctionsConstants.ResourceMouseSensitivity,
            options.GetValueOrDefault(GeargrafxSettingsConstants.MouseSensitivity,
                GeargrafxSettingsConstants.DefaultMouseSensitivity),
            Enumerable.Range(GeargrafxSettingsConstants.MouseStart,
                GeargrafxSettingsConstants.MouseCount)
                .Select(value => Invariant(value.ToString(), value.ToString(), value)));

    internal static EmulationSettingsField CpuRegion(string modelId,
        IReadOnlyDictionary<string, string> options)
    {
        var laserActive = modelId == LaserActiveMachineConstants.Id;
        var key = laserActive ? GeargrafxSettingsConstants.LaserActiveRegion
            : GeargrafxSettingsConstants.Region;
        var values = laserActive
            ? new[] {
                new EmulationSettingsChoice(GeargrafxSettingsConstants.RegionAuto,
                    GeargrafxSettingsConstants.ResourceRegionAuto),
                new EmulationSettingsChoice(GeargrafxSettingsConstants.LaserActiveRegionJapan,
                    GeargrafxSettingsConstants.ResourceRegionJapan),
                new EmulationSettingsChoice(GeargrafxSettingsConstants.LaserActiveRegionUsa,
                    GeargrafxSettingsConstants.ResourceRegionUsa) }
            : modelId == SuperGrafxMachineConstants.Id
                ? new[] {
                    new EmulationSettingsChoice(GeargrafxSettingsConstants.RegionAuto,
                        GeargrafxSettingsConstants.ResourceRegionAuto),
                    Invariant(GeargrafxSettingsConstants.SuperGrafxJapan,
                        GeargrafxSettingsConstants.SuperGrafxJapan) }
                : new[] {
                    new EmulationSettingsChoice(GeargrafxSettingsConstants.RegionAuto,
                        GeargrafxSettingsConstants.ResourceRegionAuto),
                    new EmulationSettingsChoice(GeargrafxSettingsConstants.RegionJapan,
                        GeargrafxSettingsConstants.ResourceRegionJapan),
                    new EmulationSettingsChoice(GeargrafxSettingsConstants.RegionUsa,
                        GeargrafxSettingsConstants.ResourceRegionUsa) };
        return Select(key, EmulationMachineTab.Cpu, SettingsDescriptionFunctionsConstants.Cpu,
            GeargrafxSettingsConstants.ResourceRegion,
            options.GetValueOrDefault(key, GeargrafxSettingsConstants.RegionAuto),
            values);
    }
}
