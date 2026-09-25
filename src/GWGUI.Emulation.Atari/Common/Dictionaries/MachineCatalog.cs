using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Dictionaries;

public static class MachineCatalog
{
    public static IReadOnlyList<EmulationMachineDefinition> All => ModelCatalog.All;
}
