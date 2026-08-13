using System.Collections.ObjectModel;

namespace GWGUI.Infrastructure.Hardware.Greaseweazle;

public sealed record GreaseweazleFluxCapture
{
    public GreaseweazleFluxCapture(
        IReadOnlyList<uint> fluxIntervals,
        IReadOnlyList<uint> indexIntervals,
        uint sampleFrequency,
        IReadOnlyList<byte> rawStream)
    {
        ArgumentNullException.ThrowIfNull(fluxIntervals);
        ArgumentNullException.ThrowIfNull(indexIntervals);
        ArgumentNullException.ThrowIfNull(rawStream);
        if (sampleFrequency == 0) throw new ArgumentOutOfRangeException(nameof(sampleFrequency));
        FluxIntervals = new ReadOnlyCollection<uint>(fluxIntervals.ToArray());
        IndexIntervals = new ReadOnlyCollection<uint>(indexIntervals.ToArray());
        RawStream = new ReadOnlyCollection<byte>(rawStream.ToArray());
        SampleFrequency = sampleFrequency;
    }

    public IReadOnlyList<uint> FluxIntervals { get; }

    public IReadOnlyList<uint> IndexIntervals { get; }

    public uint SampleFrequency { get; }

    public IReadOnlyList<byte> RawStream { get; }
}
