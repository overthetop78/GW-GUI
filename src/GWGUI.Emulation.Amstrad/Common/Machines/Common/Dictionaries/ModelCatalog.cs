namespace GWGUI.Emulation.Amstrad.Common.Machines.Common.Dictionaries;

public static class ModelCatalog
{
    public static IReadOnlyList<Model> All { get; } =
        Machines.CpcClassic.Dictionaries.ModelCatalog.All
            .Concat(Machines.CpcPlus.Dictionaries.ModelCatalog.All)
            .Concat(Machines.Gx4000.Dictionaries.ModelCatalog.All)
            .ToArray();

    public static Model Get(string id) => All.FirstOrDefault(model =>
            model.Id.Equals(id, StringComparison.Ordinal))
        ?? throw new ArgumentOutOfRangeException(nameof(id), id, null);

    public static string BackendModelFor(string id) => Get(id).BackendModel;
}
