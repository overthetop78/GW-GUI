namespace GWGUI.Emulation.Amiga;

public sealed record AmigaModel(string Id, string DisplayName, string Cpu, string Chipset,
    int ChipMemoryKib, int SlowMemoryKib, int FastMemoryMib, bool HasCdDrive, string RecommendedKickstart);

public static class AmigaModelCatalog
{
    public static IReadOnlyList<AmigaModel> All { get; } =
    [
        new("A500OG", "Amiga 500 · 512 Kio", "68000", "OCS", 512, 0, 0, false, "1.3"),
        new("A500", "Amiga 500 · 512 Kio + 512 Kio Slow", "68000", "OCS", 512, 512, 0, false, "1.3"),
        new("A500PLUS", "Amiga 500 Plus", "68000", "ECS", 1024, 0, 0, false, "2.04"),
        new("A600", "Amiga 600", "68000", "ECS", 2048, 0, 8, false, "3.1"),
        new("A1200OG", "Amiga 1200 · standard", "68EC020", "AGA", 2048, 0, 0, false, "3.1"),
        new("A1200", "Amiga 1200 · 8 Mio Fast", "68EC020", "AGA", 2048, 0, 8, false, "3.1"),
        new("A2000OG", "Amiga 2000 · Kickstart 1.3", "68000", "OCS", 512, 512, 0, false, "1.3"),
        new("A2000", "Amiga 2000 · Kickstart 3.1", "68000", "ECS", 1024, 0, 0, false, "3.1"),
        new("A4030", "Amiga 4000/030", "68030", "AGA", 2048, 0, 8, false, "3.1"),
        new("A4040", "Amiga 4000/040", "68040", "AGA", 2048, 0, 8, false, "3.1"),
        new("CDTV", "Commodore CDTV", "68000", "OCS", 1024, 0, 0, true, "1.3 CDTV"),
        new("CD32", "Amiga CD32", "68EC020", "AGA", 2048, 0, 0, true, "3.1 CD32"),
        new("CD32FR", "Amiga CD32 · 8 Mio Fast", "68EC020", "AGA", 2048, 0, 8, true, "3.1 CD32")
    ];

    public static AmigaModel Get(string id) => All.FirstOrDefault(model => model.Id.Equals(id, StringComparison.Ordinal))
        ?? throw new ArgumentOutOfRangeException(nameof(id), id, "Unsupported Amiga model.");
}
