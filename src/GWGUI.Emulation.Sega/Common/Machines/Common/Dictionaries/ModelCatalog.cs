using GWGUI.Emulation.Sega.Common.Machines.Common.Contracts;

namespace GWGUI.Emulation.Sega.Common.Machines.Common.Dictionaries;

public static class ModelCatalog
{
    public static IReadOnlyList<ModelDefinition> All { get; } =
    [
        new("Sg1000", "Emulation.Sega.Model.Sg1000", ""),
        new("Sc3000", "Emulation.Sega.Model.Sc3000", ""),
        new("MarkIII", "Emulation.Sega.Model.MarkIII", ""),
        new("MasterSystem", "Emulation.Sega.Model.MasterSystem", ""),
        new("MegaDrive", "Emulation.Sega.Model.MegaDrive", ""),
        new("MegaCd", "Emulation.Sega.Model.MegaCd", ""),
        new("ThirtyTwoX", "Emulation.Sega.Model.ThirtyTwoX", ""),
        new("GameGear", "Emulation.Sega.Model.GameGear", ""),
        new("Saturn", "Emulation.Sega.Model.Saturn", ""),
        new("Dreamcast", "Emulation.Sega.Model.Dreamcast", "")
    ];

    public static ModelDefinition Get(string id) => All.FirstOrDefault(model =>
        string.Equals(model.Id, id, StringComparison.OrdinalIgnoreCase))
        ?? throw new ArgumentOutOfRangeException(nameof(id), id, "Unknown Sega machine.");
}
