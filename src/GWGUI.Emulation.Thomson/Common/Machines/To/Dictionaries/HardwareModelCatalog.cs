namespace GWGUI.Emulation.Thomson.Common.Machines.To.Dictionaries;

internal static class HardwareModelCatalog
{
    // Physical base RAM including video RAM, without optional expansions; MAME thomson.cpp hardware documentation.
    internal static IReadOnlyList<HardwareModel> All { get; } = [new(MachineModel.To7, "Motorola MC6809E", 1_000_000, 24_576), new(MachineModel.To770, "Motorola MC6809E", 1_000_000, 65_536), new(MachineModel.To8, "Motorola MC6809E", 1_000_000, 262_144), new(MachineModel.To8D, "Motorola MC6809E", 1_000_000, 262_144), new(MachineModel.To9, "Motorola MC6809E", 1_000_000, 131_072), new(MachineModel.To9Plus, "Motorola MC6809E", 1_000_000, 524_288)];
}
