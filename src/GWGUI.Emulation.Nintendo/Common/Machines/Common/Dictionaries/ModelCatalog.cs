namespace GWGUI.Emulation.Nintendo.Common.Machines.Common.Dictionaries;

public static class ModelCatalog
{
    public static IReadOnlyList<Model> All { get; } =
    [
        new("GameWatch", "Game & Watch", "gamewatch", 1, false, 0, 0, false, false, false, false, 0, 0),
        new("Nes", "Nintendo Entertainment System / Famicom", "nes", 2, false, 0, 0, false, false, true, true),
        new("FamicomDisk", "Famicom Disk System", "fds", 32, false, 1, 1, false, false, false, false),
        new("Snes", "Super Nintendo / Super Famicom", "snes", 128, false, 0, 0, false, false, true, true),
        new("VirtualBoy", "Virtual Boy", "virtualboy", 1024, false, 0, 0, false, false, true, true),
        new("Nintendo64", "Nintendo 64", "n64", 4096, false, 0, 0, false, false, true, true),
        new("GameBoy", "Game Boy", "gb", 8, false, 0, 0, false, false, true, true),
        new("GameBoyColor", "Game Boy Color", "gbc", 32, false, 0, 0, false, false, true, true),
        new("GameBoyAdvance", "Game Boy Advance", "gba", 256, false, 0, 0, false, false, true, true),
        new("NintendoDs", "Nintendo DS / DSi", "nds", 4096, false, 0, 0, false, false, true, true),
        new("Nintendo3Ds", "Nintendo 3DS", "3ds", 128 * 1024, false, 0, 0, false, false, true, true),
        new("GameCube", "Nintendo GameCube", "gamecube", 24 * 1024, false, 0, 0, false, false, false, false,
            HasBuiltInCompactDiscDrive: true, SupportsCompactDiscDrive: true),
        new("Wii", "Nintendo Wii", "wii", 88 * 1024, false, 0, 0, false, false, false, false,
            HasBuiltInCompactDiscDrive: true, SupportsCompactDiscDrive: true),
        new("WiiU", "Nintendo Wii U", "wiiu", 2048 * 1024, false, 0, 0, false, false, false, false,
            HasBuiltInCompactDiscDrive: true, SupportsCompactDiscDrive: true),
        new("Switch", "Nintendo Switch", "switch", 4096 * 1024, false, 0, 0, false, false, true, true)
    ];

    public static Model Get(string id) => All.FirstOrDefault(model =>
            model.Id.Equals(id, StringComparison.Ordinal))
        ?? throw new ArgumentOutOfRangeException(nameof(id), id, null);

    public static string BackendModelFor(string id) => Get(id).BackendModel;
}
