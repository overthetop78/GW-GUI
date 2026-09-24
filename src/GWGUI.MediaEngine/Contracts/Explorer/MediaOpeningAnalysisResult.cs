using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Contracts.Explorer;

namespace GWGUI.MediaEngine.Contracts.Explorer;

/// <summary>Collects one loaded media document and its requested explorations.</summary>
public sealed record MediaOpeningAnalysisResult(
    MediaImageDocument Document,
    ExploredDiskImage? DiskExploration,
    ExploredMediaImage? MediaExploration);

