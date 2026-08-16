using GWGUI.Emulation;

namespace GWGUI.App.Controls;

internal sealed record AtariVideoAudioChoice(string Value, string DisplayName);

internal sealed record AtariVideoAudioView(
    IReadOnlyList<AtariVideoAudioChoice> Standards,
    IReadOnlyList<AtariVideoAudioChoice> Regions,
    IReadOnlyList<AtariVideoAudioChoice> Resolutions,
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
