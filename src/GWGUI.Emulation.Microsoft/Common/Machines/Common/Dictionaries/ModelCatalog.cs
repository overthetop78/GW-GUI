namespace GWGUI.Emulation.Microsoft.Common.Machines.Common.Dictionaries;

public static class ModelCatalog
{
    public static IReadOnlyList<Model> All { get; } =
    [
        new("Xbox", "Xbox", "xbox", 64 * 1024, false, 0, 0, false, false, false, false),
        new("Xbox360", "Xbox 360", "xbox360", 512 * 1024, false, 0, 0, false, false, false, false)
    ];

    public static Model Get(string id) => All.FirstOrDefault(model =>
            model.Id.Equals(id, StringComparison.Ordinal))
        ?? throw new ArgumentOutOfRangeException(nameof(id), id, null);

    public static string BackendModelFor(string id) => Get(id).BackendModel;
}
