using System.Collections.ObjectModel;

namespace GWGUI.MediaEngine.Flux;

/// <summary>Représente une révolution complète de flux rotationnel indépendamment de son conteneur d'origine.</summary>
public sealed record FluxRevolution
{
    /// <summary>Initialise une révolution en copiant ses intervalles de flux.</summary>
    /// <param name="IndexTimeTicks">Durée de la révolution, exprimée en ticks de la source.</param>
    /// <param name="FluxIntervals">Intervalles entre transitions magnétiques, exprimés dans les mêmes ticks.</param>
    /// <exception cref="ArgumentNullException"><paramref name="FluxIntervals"/> est nul.</exception>
    public FluxRevolution(uint IndexTimeTicks, IReadOnlyList<uint> FluxIntervals)
    {
        ArgumentNullException.ThrowIfNull(FluxIntervals);
        this.IndexTimeTicks = IndexTimeTicks;
        this.FluxIntervals = new ReadOnlyCollection<uint>(FluxIntervals.ToArray());
    }

    /// <summary>Obtient la durée de la révolution, exprimée en ticks de la source.</summary>
    public uint IndexTimeTicks { get; }
    /// <summary>Obtient la copie non modifiable des intervalles entre transitions, exprimés en ticks de la source.</summary>
    public IReadOnlyList<uint> FluxIntervals { get; }
}
