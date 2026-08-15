namespace GWGUI.Emulation.Amiga;

public sealed record AmigaModel(string Id, string DisplayName, string BackendModel,
    IReadOnlyList<string> CpuModels, string Chipset, int ChipMemoryKib, int SlowMemoryKib,
    int FastMemoryMib, bool HasCdDrive, string RecommendedKickstart,
    int MaximumFloppyDrives = 4, bool SupportsHardDrives = true, int MaximumHardDrives = 1,
    int MouseButtonCount = 2, bool SupportsCd32Controller = false)
{
    public string DefaultCpu => CpuModels[0];
}

public static class AmigaModelCatalog
{
    private static readonly HashSet<string> LegacyBackendIds =
        ["A500OG", "A1200OG", "A2000OG", "A4030", "A4040", "CD32FR"];
    public static IReadOnlyList<AmigaModel> All { get; } =
    [
        new("A500", "Amiga 500", "A500", ["68000"], "OCS", 512, 512, 0, false, "1.3", 4, true, 4),
        new("A500PLUS", "Amiga 500 Plus", "A500PLUS", ["68000"], "ECS", 1024, 0, 0, false, "2.04", 4, true, 4),
        new("A600", "Amiga 600", "A600", ["68000"], "ECS", 1024, 0, 0, false, "3.1", 4, true, 4),
        new("A1000", "Amiga 1000", "A500OG", ["68000"], "OCS", 512, 0, 0, false, "1.2", 4, true, 2),
        new("A1200", "Amiga 1200", "A1200", ["68020"], "AGA", 2048, 0, 0, false, "3.1", 4, true, 4),
        new("A2000", "Amiga 2000", "A2000", ["68000"], "ECS", 1024, 0, 0, false, "3.1", 4, true, 8),
        new("A3000", "Amiga 3000", "A2000", ["68030"], "ECS", 2048, 0, 8, false, "3.1", 4, true, 8),
        new("A4000", "Amiga 4000", "A4040", ["68040", "68030"], "AGA", 2048, 0, 8, false, "3.1", 4, true, 8),
        new("CDTV", "Commodore CDTV", "CDTV", ["68000"], "OCS", 1024, 0, 0, true, "1.3 CDTV", 1, true, 2),
        new("CD32", "Amiga CD32", "CD32", ["68020"], "AGA", 2048, 0, 0, true, "3.1 CD32", 0, false, 0, 2, true)
    ];

    public static AmigaModel Get(string id) => All.FirstOrDefault(model => model.Id.Equals(id, StringComparison.Ordinal))
        ?? FromLegacyId(id)
        ?? throw new ArgumentOutOfRangeException(nameof(id), id, "Unsupported Amiga model.");

    public static AmigaModel? FromLegacyId(string id) => id switch
    {
        "A500OG" => All.First(model => model.Id == "A500"),
        "A1200OG" => All.First(model => model.Id == "A1200"),
        "A2000OG" => All.First(model => model.Id == "A2000"),
        "A4030" or "A4040" => All.First(model => model.Id == "A4000"),
        "CD32FR" => All.First(model => model.Id == "CD32"),
        _ => null
    };

    public static string BackendModelFor(string id) => LegacyBackendIds.Contains(id) ? id : Get(id).BackendModel;
}
