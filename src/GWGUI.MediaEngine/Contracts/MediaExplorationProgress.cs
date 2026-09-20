using GWGUI.MediaEngine.Enums;

namespace GWGUI.MediaEngine.Contracts;

/// <summary>Reports a technical media-exploration stage for presentation by a caller.</summary>
public sealed record MediaExplorationProgress(
    MediaExplorationProgressStage Stage,
    string Detail,
    double Value,
    MediaKind? MediaKind = null);
