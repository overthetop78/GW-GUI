using System.Collections.ObjectModel;
using GWGUI.Infrastructure.Hardware.Greaseweazle;
using GWGUI.MediaEngine.Containers.Scp;

namespace GWGUI.App.Contracts.Services.PhysicalDiskReading;

public sealed record PhysicalDiskFluxAcquisition
{
    public PhysicalDiskFluxAcquisition(
        ScpImage image,
        IReadOnlyDictionary<PhysicalDiskTrackAddress, GreaseweazleFluxCapture> rawCaptures)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(rawCaptures);
        Image = image;
        RawCaptures = new ReadOnlyDictionary<PhysicalDiskTrackAddress, GreaseweazleFluxCapture>(
            new Dictionary<PhysicalDiskTrackAddress, GreaseweazleFluxCapture>(rawCaptures));
    }

    public ScpImage Image { get; }

    public IReadOnlyDictionary<PhysicalDiskTrackAddress, GreaseweazleFluxCapture> RawCaptures { get; }
}
