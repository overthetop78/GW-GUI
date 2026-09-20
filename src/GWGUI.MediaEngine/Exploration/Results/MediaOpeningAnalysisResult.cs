using GWGUI.MediaEngine.Contracts;

namespace GWGUI.MediaEngine.Exploration.Results;

/// <summary>Collects one loaded media document and the explorations derived from that same document.</summary>
public sealed record MediaOpeningAnalysisResult(
    MediaImageDocument Document,
    ExploredDiskImage DiskExploration,
    ExploredMediaImage? MediaExploration);
