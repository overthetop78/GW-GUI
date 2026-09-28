namespace GWGUI.Emulation.Sony.Common.Machines.Common.Dictionaries;

public static class ModelCatalog
{
    public static IReadOnlyList<Model> All { get; } =
    [
        new("PlayStation", "PlayStation / PS one", "ps1", 2048, false, 0, 0, false, false, false, false),
        new("PlayStation2", "PlayStation 2 / PStwo", "ps2", 32768, false, 0, 0, false, false, false, false),
        new("Psp", "PlayStation Portable", "psp", 32768, false, 0, 0, false, false, true, true),
        new("PsVita", "PlayStation Vita", "vita", 512 * 1024, false, 0, 0, false, false, true, true),
        new("PlayStation3", "PlayStation 3", "ps3", 256 * 1024, false, 0, 0, false, false, false, false),
        new("PlayStation4", "PlayStation 4", "ps4", 8 * 1024 * 1024, false, 0, 0, false, false, false, false),
        new("PlayStation5", "PlayStation 5", "ps5", 16 * 1024 * 1024, false, 0, 0, false, false, false, false)
    ];

    public static Model Get(string id) => All.FirstOrDefault(model =>
            model.Id.Equals(id, StringComparison.Ordinal))
        ?? throw new ArgumentOutOfRangeException(nameof(id), id, null);

    public static string BackendModelFor(string id) => Get(id).BackendModel;
}
