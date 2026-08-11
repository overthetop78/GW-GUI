using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Containers.Scp;

/// <summary>Représente une révolution capturée d'une piste SCP et ses intervalles de flux décodés.</summary>
public sealed record ScpRevolution
{
    /// <summary>Initialise une révolution SCP en copiant les intervalles fournis.</summary>
    /// <param name="indexTimeTicks">Durée de la révolution, exprimée en pas temporels SCP.</param>
    /// <param name="declaredFluxCount">Nombre de mots de flux déclaré dans le descripteur SCP ; il peut différer du nombre d'intervalles après fusion des marqueurs de dépassement.</param>
    /// <param name="fluxIntervals">Intervalles entre transitions magnétiques, exprimés en pas temporels SCP.</param>
    /// <exception cref="ArgumentNullException"><paramref name="fluxIntervals"/> est nul.</exception>
    public ScpRevolution(uint indexTimeTicks, uint declaredFluxCount, IReadOnlyList<uint> fluxIntervals) : this(new FluxRevolution(indexTimeTicks, fluxIntervals), declaredFluxCount) { }

    /// <summary>Initialise une révolution SCP depuis une révolution générique sans recopier ses intervalles.</summary>
    /// <param name="flux">Révolution générique à raccorder.</param>
    /// <param name="declaredFluxCount">Nombre de mots de flux déclaré dans le descripteur SCP.</param>
    /// <exception cref="ArgumentNullException"><paramref name="flux"/> est nul.</exception>
    public ScpRevolution(FluxRevolution flux, uint declaredFluxCount)
    {
        ArgumentNullException.ThrowIfNull(flux);
        DeclaredFluxCount = declaredFluxCount;
        Flux = flux;
    }

    /// <summary>Obtient la durée déclarée de la révolution, exprimée en pas temporels SCP.</summary>
    public uint IndexTimeTicks => Flux.IndexTimeTicks;

    /// <summary>Obtient le nombre de mots de flux déclaré dans le descripteur SCP.</summary>
    public uint DeclaredFluxCount { get; }

    /// <summary>Obtient les intervalles entre transitions magnétiques, exprimés en pas temporels SCP.</summary>
    public IReadOnlyList<uint> FluxIntervals => Flux.FluxIntervals;

    /// <summary>Obtient la représentation générique de la révolution de flux.</summary>
    public FluxRevolution Flux { get; }

    /// <summary>Convertit la durée déclarée de la révolution en millisecondes.</summary>
    /// <param name="resolutionNanoseconds">Durée strictement positive d'un pas temporel, en nanosecondes.</param>
    /// <returns>Durée de la révolution, en millisecondes.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="resolutionNanoseconds"/> est nul ou négatif.</exception>
    public double DurationMilliseconds(int resolutionNanoseconds)
    {
        ValidateResolution(resolutionNanoseconds);
        return IndexTimeTicks * resolutionNanoseconds / TimeUnitConstants.NanosecondsPerMillisecond;
    }

    /// <summary>Calcule la vitesse de rotation correspondant à la durée déclarée de la révolution.</summary>
    /// <param name="resolutionNanoseconds">Durée strictement positive d'un pas temporel, en nanosecondes.</param>
    /// <returns>Vitesse de rotation, en tours par minute.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="resolutionNanoseconds"/> est nul ou négatif.</exception>
    public double Rpm(int resolutionNanoseconds)
    {
        return TimeUnitConstants.MillisecondsPerMinute / DurationMilliseconds(resolutionNanoseconds);
    }

    /// <summary>Vérifie qu'une résolution permet les conversions temporelles.</summary>
    /// <param name="resolutionNanoseconds">Durée d'un pas temporel, en nanosecondes.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="resolutionNanoseconds"/> est nul ou négatif.</exception>
    private static void ValidateResolution(int resolutionNanoseconds)
    {
        if (resolutionNanoseconds <= 0) throw ScpExceptions.InvalidResolution(resolutionNanoseconds);
    }
}
