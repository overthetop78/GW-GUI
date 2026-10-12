namespace GWGUI.Emulation.Thomson.Common.Machines.Mo.Dictionaries;

internal static class HardwareModelCatalog
{
    // Physical base RAM including video RAM, without optional expansions; MAME thomson.cpp hardware documentation.
    internal static IReadOnlyList<HardwareModel> All { get; } = [new(MachineModel.Mo5, "Motorola MC6809E", 1_000_000, 49_152), new(MachineModel.Mo6, "Motorola MC6809E", 1_000_000, 131_072)];
}
