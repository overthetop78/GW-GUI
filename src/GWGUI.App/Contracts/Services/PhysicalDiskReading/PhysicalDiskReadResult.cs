using System.Collections.ObjectModel;
using MediaAcquisitionResult = global::GWGUI.MediaEngine.Contracts.MediaAcquisitionResult;
using GWGUI.MediaEngine.Exploration.Contracts;
using GWGUI.MediaEngine.Exploration.Results;

namespace GWGUI.App.Contracts.Services.PhysicalDiskReading;

public sealed record PhysicalDiskReadResult
{
    public PhysicalDiskReadResult(
        string outputPath,
        MediaAcquisitionResult acquisition,
        IReadOnlyList<PhysicalDiskTrackDiagnostic> trackDiagnostics,
        ExploredDiskImage document)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        ArgumentNullException.ThrowIfNull(acquisition);
        ArgumentNullException.ThrowIfNull(trackDiagnostics);
        ArgumentNullException.ThrowIfNull(document);
        OutputPath = outputPath;
        Acquisition = acquisition;
        TrackDiagnostics = new ReadOnlyCollection<PhysicalDiskTrackDiagnostic>(trackDiagnostics.ToArray());
        Document = document;
    }

    public string OutputPath { get; }

    public MediaAcquisitionResult Acquisition { get; }

    public IReadOnlyList<PhysicalDiskTrackDiagnostic> TrackDiagnostics { get; }

    public ExploredDiskImage Document { get; }

    public IImageDisquette Image => Document;
}
