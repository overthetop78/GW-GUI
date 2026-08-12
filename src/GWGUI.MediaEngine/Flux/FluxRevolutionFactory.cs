using GWGUI.MediaEngine.Encoding;

namespace GWGUI.MediaEngine.Flux;

/// <summary>Construit une révolution de flux générique à partir de cellules binaires temporelles.</summary>
internal static class FluxRevolutionFactory
{
    /// <summary>Convertit les transitions binaires en intervalles exprimés en ticks.</summary>
    /// <param name="bits">Cellules binaires ; une cellule à un représente une transition.</param>
    /// <param name="cellTicks">Durée d'une cellule, en ticks.</param>
    /// <param name="indexTimeTicks">Durée de la révolution complète, en ticks.</param>
    /// <returns>Révolution contenant les intervalles copiés.</returns>
    /// <exception cref="ArgumentNullException">La collection de cellules est nulle.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Une durée est nulle.</exception>
    /// <exception cref="OverflowException">Le produit du nombre de cellules par leur durée dépasse un entier non signé.</exception>
    public static FluxRevolution Create(IReadOnlyList<bool> bits, uint cellTicks, uint indexTimeTicks)
    {
        ArgumentNullException.ThrowIfNull(bits);
        if (cellTicks == 0) throw TrackEncodingExceptions.ZeroBitCell(cellTicks);
        if (indexTimeTicks == 0) throw TrackEncodingExceptions.ZeroIndexTime(indexTimeTicks);
        var intervals = new List<uint>();
        uint cells = 0;
        foreach (var bit in bits)
        {
            cells++;
            if (!bit) continue;
            intervals.Add(Interval(cells, cellTicks));
            cells = 0;
        }
        if (cells > 0) intervals.Add(Interval(cells, cellTicks));
        return new(indexTimeTicks, intervals);
    }

    /// <summary>Calcule un intervalle de flux en contrôlant son dépassement.</summary>
    /// <param name="cells">Nombre de cellules composant l'intervalle.</param>
    /// <param name="cellTicks">Durée d'une cellule, en ticks.</param>
    /// <returns>Durée de l'intervalle, en ticks.</returns>
    private static uint Interval(uint cells, uint cellTicks)
    {
        try { return checked(cells * cellTicks); }
        catch (OverflowException) { throw TrackEncodingExceptions.FluxIntervalOverflow(cells, cellTicks); }
    }
}
