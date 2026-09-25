namespace GWGUI.Emulation.Amiga.Common.Dictionaries;

using Cd32ModelConstants = GWGUI.Emulation.Amiga.Common.Machines.AmigaCD32.Constants.ModelConstants;
using ComputerModelConstants = GWGUI.Emulation.Amiga.Common.Machines.AmigaComputers.Constants.ModelConstants;

public static class ModelCatalog
{
    private static readonly HashSet<string> LegacyBackendIds =
        [ComputerModelConstants.A500OG, ComputerModelConstants.A1200OG,
            ComputerModelConstants.A2000OG, ComputerModelConstants.A4030,
            ComputerModelConstants.A4040, Cd32ModelConstants.CD32FR];
    public static IReadOnlyList<Model> All { get; } =
        Machines.AmigaComputers.Dictionaries.ModelCatalog.All
            .Concat(Machines.AmigaCDTV.Dictionaries.ModelCatalog.All)
            .Concat(Machines.AmigaCD32.Dictionaries.ModelCatalog.All)
            .ToArray();

    public static Model Get(string id) => All.FirstOrDefault(model => model.Id.Equals(id, StringComparison.Ordinal))
        ?? FromLegacyId(id)
        ?? throw new ArgumentOutOfRangeException(nameof(id), id, ModelConstants.UnsupportedModel);

    public static Model? FromLegacyId(string id) => id switch
    {
        ComputerModelConstants.A500OG => All.First(model => model.Id == ComputerModelConstants.A500),
        ComputerModelConstants.A1200OG => All.First(model => model.Id == ComputerModelConstants.A1200),
        ComputerModelConstants.A2000OG => All.First(model => model.Id == ComputerModelConstants.A2000),
        ComputerModelConstants.A4030 or ComputerModelConstants.A4040 =>
            All.First(model => model.Id == ComputerModelConstants.A4000),
        Cd32ModelConstants.CD32FR => All.First(model => model.Id == Cd32ModelConstants.CD32),
        _ => null
    };

    public static string BackendModelFor(string id) => LegacyBackendIds.Contains(id) ? id : Get(id).BackendModel;
}
