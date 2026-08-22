using System.Collections.ObjectModel;
using GWGUI.MediaEngine.Decoding;

namespace GWGUI.App.Contracts.Services.PhysicalDiskReading;

public sealed record PhysicalDiskTrackDiagnostic
{
    public PhysicalDiskTrackDiagnostic(
        int cylinder,
        int head,
        FluxDecodeSelection best,
        IReadOnlyList<FluxDecodeSelection> decoderResults)
    {
        ArgumentNullException.ThrowIfNull(best);
        ArgumentNullException.ThrowIfNull(decoderResults);
        Cylinder = cylinder;
        Head = head;
        Best = best;
        DecoderResults = new ReadOnlyCollection<FluxDecodeSelection>(decoderResults.ToArray());
    }

    public int Cylinder { get; }

    public int Head { get; }

    public FluxDecodeSelection Best { get; }

    public IReadOnlyList<FluxDecodeSelection> DecoderResults { get; }
}
