using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari;

public sealed record AtariVideoAudioChoice(string Value, string DisplayName);

public sealed record AtariVideoAudioView(
    IReadOnlyList<AtariVideoAudioChoice> Standards,
    IReadOnlyList<AtariVideoAudioChoice> Regions,
    IReadOnlyList<AtariVideoAudioChoice> Resolutions,
    IReadOnlyList<AtariVideoAudioChoice> ArtifactingModes,
    IReadOnlyList<AtariVideoAudioChoice> ColorHue,
    IReadOnlyList<AtariVideoAudioChoice> ColorSaturation,
    IReadOnlyList<AtariVideoAudioChoice> ColorContrast,
    IReadOnlyList<AtariVideoAudioChoice> ColorBrightness,
    IReadOnlyList<AtariVideoAudioChoice> ColorGamma,
    IReadOnlyList<AtariVideoAudioChoice> ColorDelay,
    IReadOnlyList<AtariVideoAudioChoice> ExternalPalettes,
    IReadOnlyList<AtariVideoAudioChoice> AspectRatios,
    IReadOnlyList<AtariVideoAudioChoice> Cropping,
    IReadOnlyList<AtariVideoAudioChoice> FrameSkips,
    IReadOnlyList<AtariVideoAudioChoice> Renderers,
    IReadOnlyList<AtariVideoAudioChoice> Outputs,
    IReadOnlyList<AtariVideoAudioChoice> Latencies,
    IReadOnlyList<AtariVideoAudioChoice> Volumes,
    IReadOnlyList<AtariVideoAudioChoice> Qualities,
    bool AudioEnabled,
    EmulationVideoRenderer Renderer);
