using GWGUI.MediaEngine.Flux;

namespace GWGUI.MediaEngine.TrackImages;

/// <summary>Associe une révolution de flux à la résolution temporelle de ses ticks.</summary>
public sealed record TrackFluxRevolution
{
    public TrackFluxRevolution(int resolutionNanoseconds, FluxRevolution flux)
    {
        if (resolutionNanoseconds <= 0) throw new ArgumentOutOfRangeException(nameof(resolutionNanoseconds));
        ArgumentNullException.ThrowIfNull(flux);
        ResolutionNanoseconds = resolutionNanoseconds;
        Flux = flux;
    }

    public int ResolutionNanoseconds { get; }
    public FluxRevolution Flux { get; }
}
