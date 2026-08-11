using GWGUI.MediaEngine.Containers.I86f;
using GWGUI.MediaEngine.Containers.Scp;

namespace GWGUI.MediaEngine.Flux.Conversion;

/// <summary>Convertit les cellules de bits normalisées d'une piste 86F en intervalles de flux.</summary>
internal static class I86fBitCellFluxConverter
{
    /// <summary>Accumule les cellules jusqu'à chaque transition et exprime leur durée en ticks SCP.</summary>
    /// <param name="bits">Cellules normalisées de la piste.</param>
    /// <returns>Une révolution compatible avec les décodeurs, ou <see langword="null"/> en l'absence de transition.</returns>
    public static ScpRevolution? Convert(IReadOnlyList<bool> bits)
    {
        var intervals = new List<uint>(bits.Count / 2);
        var cells = 0u;
        foreach (var set in bits)
        {
            cells++;
            if (!set) continue;
            intervals.Add(cells * I86fLayout.TicksPerBitCell);
            cells = 0;
        }
        return intervals.Count == 0 ? null : new((uint)(bits.Count * I86fLayout.TicksPerBitCell), (uint)intervals.Count, intervals);
    }
}
