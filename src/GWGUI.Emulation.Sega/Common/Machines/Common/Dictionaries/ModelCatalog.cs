namespace GWGUI.Emulation.Sega.Common.Machines.Common.Dictionaries;

public static class ModelCatalog
{
    public static IReadOnlyList<Model> All { get; } =
    [
        new("Sg1000", "SG-1000", "sg1000", 16, false, 0, 0, false, false, true, true),
        new("Sc3000", "SC-3000", "sc3000", 16, true, 0, 0, false, false, true, true),
        new("MarkIII", "Mark III", "mark3", 64, false, 0, 0, false, false, true, true),
        new("MasterSystem", "Master System", "mastersystem", 128, false, 0, 0, false, false, true, true),
        new("MegaDrive", "Mega Drive / Genesis", "megadrive", 64, false, 0, 0, false, false, true, true),
        new("MegaCd", "Mega-CD / Sega CD", "megacd", 64, false, 0, 0, false, false, true, true),
        new("ThirtyTwoX", "32X", "32x", 256, false, 0, 0, false, false, true, true),
        new("GameGear", "Game Gear", "gamegear", 24, false, 0, 0, false, false, true, true),
        new("Saturn", "Saturn", "saturn", 2048, false, 0, 0, false, false, true, true),
        new("Dreamcast", "Dreamcast", "dreamcast", 16384, false, 0, 0, false, false, true, true)
    ];

    public static Model Get(string id) => All.FirstOrDefault(model =>
            model.Id.Equals(id, StringComparison.Ordinal))
        ?? throw new ArgumentOutOfRangeException(nameof(id), id, null);

    public static string BackendModelFor(string id) => Get(id).BackendModel;
}
