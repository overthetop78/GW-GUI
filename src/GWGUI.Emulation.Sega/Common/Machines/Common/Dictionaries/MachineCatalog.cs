using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Sega.Common.Machines.Common.Dictionaries;

namespace GWGUI.Emulation.Sega.Common.Machines.Common.Dictionaries;

public static class MachineCatalog
{
    public static IReadOnlyList<EmulationMachineDefinition> All { get; } = ModelCatalog.All
        .Select(model => new EmulationMachineDefinition(model.Id, model.DisplayResourceKey))
        .ToArray();
}
