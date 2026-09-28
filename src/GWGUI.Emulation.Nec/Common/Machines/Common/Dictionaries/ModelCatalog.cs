namespace GWGUI.Emulation.Nec.Common.Machines.Common.Dictionaries;

using GWGUI.Emulation;
using GWGUI.Emulation.Nec.Common.Machines.Common.Contracts;

internal static class ModelCatalog
{
    internal static IReadOnlyList<Model> All { get; } =
    [
        new("PcEngine", "PC Engine / TurboGrafx-16", "pce", 8, false, 0, 0, false, false, true, true),
        new("CoreGrafx", "PC Engine CoreGrafx", "pce", 8, false, 0, 0, false, false, true, true),
        new("SuperGrafx", "PC Engine SuperGrafx", "supergrafx", 8, false, 0, 0, false, false, true, true),
        new("PcEngineDuo", "PC Engine Duo / TurboDuo", "pcecd", 32, false, 0, 0, false, false, true, true),
        new("TurboExpress", "TurboExpress / PC Engine GT", "pce", 8, false, 0, 0, false, false, true, true),
        new("PcFx", "PC-FX", "pcfx", 32, false, 0, 0, false, false, true, true)
    ];

    internal static Model Get(string id) => All.FirstOrDefault(item =>
        string.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase))
        ?? throw new ArgumentOutOfRangeException(nameof(id), id, null);
}