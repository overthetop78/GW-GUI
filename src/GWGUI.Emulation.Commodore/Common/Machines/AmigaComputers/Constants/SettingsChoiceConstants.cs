namespace GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants;

internal static class SettingsChoiceConstants
{
    internal static IReadOnlyDictionary<CpuCompatibility, EmulationSettingsChoice> CpuCompatibilityChoices { get; } =
        new Dictionary<CpuCompatibility, EmulationSettingsChoice>
        {
            [CpuCompatibility.Normal] = new("normal", SettingsResourceKeys.CpuCompatibilityNormal),
            [CpuCompatibility.Compatible] = new("compatible", SettingsResourceKeys.CpuCompatibilityCompatible),
            [CpuCompatibility.MemoryExact] = new("memory", SettingsResourceKeys.CpuCompatibilityMemory),
            [CpuCompatibility.Exact] = new("exact", SettingsResourceKeys.CpuCompatibilityExact),
        };
    internal static IReadOnlyDictionary<VideoStandard, EmulationSettingsChoice> VideoStandardChoices { get; } =
        new Dictionary<VideoStandard, EmulationSettingsChoice>
        {
            [VideoStandard.PALAuto] = new("PAL auto", string.Empty, "PAL auto"),
            [VideoStandard.NTSCAuto] = new("NTSC auto", string.Empty, "NTSC auto"),
            [VideoStandard.PAL] = new("PAL", string.Empty, "PAL"),
            [VideoStandard.NTSC] = new("NTSC", string.Empty, "NTSC"),
        };
    internal static IReadOnlyDictionary<VideoResolution, EmulationSettingsChoice> VideoResolutionChoices { get; } =
        new Dictionary<VideoResolution, EmulationSettingsChoice>
        {
            [VideoResolution.Automatic] = new("auto", SettingsValueConstants.VisualAutomatic),
            [VideoResolution.AutomaticLow] = new("auto-lores", SettingsResourceKeys.VideoResolutionAutoLow),
            [VideoResolution.AutomaticSuperHigh] = new("auto-superhires", SettingsResourceKeys.VideoResolutionAutoSuperHigh),
            [VideoResolution.Low] = new("lores", SettingsResourceKeys.VideoResolutionLow),
            [VideoResolution.High] = new("hires", SettingsResourceKeys.VideoResolutionHigh),
            [VideoResolution.SuperHigh] = new("superhires", SettingsResourceKeys.VideoResolutionSuperHigh),
        };
    internal static IReadOnlyDictionary<VideoAspect, EmulationSettingsChoice> VideoAspectChoices { get; } =
        new Dictionary<VideoAspect, EmulationSettingsChoice>
        {
            [VideoAspect.Automatic] = new("auto", SettingsValueConstants.VisualAutomatic),
            [VideoAspect.PAL] = new("PAL", string.Empty, "PAL"),
            [VideoAspect.NTSC] = new("NTSC", string.Empty, "NTSC"),
            [VideoAspect.SquarePixels] = new("1:1", string.Empty, "1:1"),
        };
    internal static IReadOnlyDictionary<CropMode, EmulationSettingsChoice> CropModeChoices { get; } =
        new Dictionary<CropMode, EmulationSettingsChoice>
        {
            [CropMode.Disabled] = new("disabled", SettingsResourceKeys.ValueDisabled),
            [CropMode.Minimum] = new("minimum", SettingsResourceKeys.ValueMinimum),
            [CropMode.VerySmall] = new("smaller", SettingsResourceKeys.ValueVerySmall),
            [CropMode.Small] = new("small", SettingsResourceKeys.ValueSmall),
            [CropMode.Medium] = new("medium", SettingsResourceKeys.ValueMedium),
            [CropMode.Large] = new("large", SettingsResourceKeys.ValueLarge),
            [CropMode.VeryLarge] = new("larger", SettingsResourceKeys.ValueVeryLarge),
            [CropMode.Maximum] = new("maximum", SettingsResourceKeys.ValueMaximum),
            [CropMode.Automatic] = new("auto", SettingsValueConstants.VisualAutomatic),
        };
    internal static IReadOnlyDictionary<VideoLineMode, EmulationSettingsChoice> VideoLineModeChoices { get; } =
        new Dictionary<VideoLineMode, EmulationSettingsChoice>
        {
            [VideoLineMode.Automatic] = new("auto", SettingsValueConstants.VisualAutomatic),
            [VideoLineMode.Single] = new("single", SettingsResourceKeys.VideoLineModeSingle),
            [VideoLineMode.Double] = new("double", SettingsResourceKeys.VideoLineModeDouble),
        };
    internal static IReadOnlyDictionary<RefreshRateChange, EmulationSettingsChoice> RefreshRateChangeChoices { get; } =
        new Dictionary<RefreshRateChange, EmulationSettingsChoice>
        {
            [RefreshRateChange.Disabled] = new("disabled", SettingsResourceKeys.ValueDisabled),
            [RefreshRateChange.Enabled] = new("enabled", SettingsResourceKeys.ValueEnabled),
            [RefreshRateChange.Locked] = new("locked", SettingsResourceKeys.StateLocked),
        };
    internal static IReadOnlyDictionary<BlitterMode, EmulationSettingsChoice> BlitterModeChoices { get; } =
        new Dictionary<BlitterMode, EmulationSettingsChoice>
        {
            [BlitterMode.Normal] = new("false", SettingsResourceKeys.ValueDisabled),
            [BlitterMode.Immediate] = new("immediate", SettingsResourceKeys.StateImmediate),
            [BlitterMode.Waiting] = new("waiting", SettingsResourceKeys.StateWaiting),
        };
    internal static IReadOnlyDictionary<CollisionMode, EmulationSettingsChoice> CollisionModeChoices { get; } =
        new Dictionary<CollisionMode, EmulationSettingsChoice>
        {
            [CollisionMode.None] = new("none", SettingsValueConstants.HostToolsNone),
            [CollisionMode.Sprites] = new("sprites", SettingsResourceKeys.VideoCollisionSprites),
            [CollisionMode.Playfields] = new("playfields", SettingsResourceKeys.VideoCollisionPlayfields),
            [CollisionMode.Full] = new("full", SettingsResourceKeys.VideoCollisionFull),
        };
    internal static IReadOnlyDictionary<AudioInterpolation, EmulationSettingsChoice> AudioInterpolationChoices { get; } =
        new Dictionary<AudioInterpolation, EmulationSettingsChoice>
        {
            [AudioInterpolation.None] = new("none", SettingsValueConstants.HostToolsNone),
            [AudioInterpolation.Anti] = new("anti", SettingsResourceKeys.AudioInterpolationAnti),
            [AudioInterpolation.Sinc] = new("sinc", string.Empty, "Sinc"),
            [AudioInterpolation.RH] = new("rh", string.Empty, "RH"),
            [AudioInterpolation.Crux] = new("crux", string.Empty, "Crux"),
        };
    internal static IReadOnlyDictionary<AudioFilter, EmulationSettingsChoice> AudioFilterChoices { get; } =
        new Dictionary<AudioFilter, EmulationSettingsChoice>
        {
            [AudioFilter.Emulated] = new("emulated", SettingsResourceKeys.AudioFilterEmulated),
            [AudioFilter.Disabled] = new("off", SettingsResourceKeys.ValueDisabled),
            [AudioFilter.Enabled] = new("on", SettingsResourceKeys.ValueEnabled),
        };
    internal static IReadOnlyDictionary<AudioFilterType, EmulationSettingsChoice> AudioFilterTypeChoices { get; } =
        new Dictionary<AudioFilterType, EmulationSettingsChoice>
        {
            [AudioFilterType.Automatic] = new("auto", SettingsValueConstants.VisualAutomatic),
            [AudioFilterType.Standard] = new("standard", SettingsResourceKeys.ValueStandard),
            [AudioFilterType.Enhanced] = new("enhanced", SettingsResourceKeys.ValueEnhanced),
        };
    internal static IReadOnlyDictionary<AnalogMouseMode, EmulationSettingsChoice> AnalogMouseModeChoices { get; } =
        new Dictionary<AnalogMouseMode, EmulationSettingsChoice>
        {
            [AnalogMouseMode.Disabled] = new("disabled", SettingsResourceKeys.ValueDisabled),
            [AnalogMouseMode.Left] = new("left", SettingsResourceKeys.ControllerStickLeft),
            [AnalogMouseMode.Right] = new("right", SettingsResourceKeys.ControllerStickRight),
            [AnalogMouseMode.Both] = new("both", SettingsResourceKeys.ControllerStickBoth),
        };
}
