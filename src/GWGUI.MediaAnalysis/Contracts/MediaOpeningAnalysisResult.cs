using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Exploration.Results;

namespace GWGUI.MediaAnalysis.Contracts;

/// <summary>Collects one loaded media document and the explorations derived from that same document.</summary>
public sealed record MediaOpeningAnalysisResult(
    MediaImageDocument Document,
    ExploredDiskImage DiskExploration,
    ExploredMediaImage? MediaExploration);
