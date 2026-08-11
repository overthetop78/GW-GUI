using GWGUI.MediaEngine.Containers.I86f;
using GWGUI.MediaEngine.Containers.Scp;

namespace GWGUI.MediaEngine.Flux.Conversion;

internal static class I86fBitCellFluxConverter
{
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
