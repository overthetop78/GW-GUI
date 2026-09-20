using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Exploration.Enums;

namespace GWGUI.MediaEngine.Exploration.Contracts;

/// <summary>Reports a technical media-exploration stage for presentation by a caller.</summary>
public sealed record MediaExplorationProgress(
    MediaExplorationProgressStage Stage,
    string Detail,
    double Value,
    MediaKind? MediaKind = null);
