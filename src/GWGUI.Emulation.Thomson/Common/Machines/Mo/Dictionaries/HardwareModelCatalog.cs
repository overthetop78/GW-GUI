namespace GWGUI.Emulation.Thomson.Common.Machines.Mo.Dictionaries;

internal static class HardwareModelCatalog
{
    // MO5: Deep silence (Mikaël Restoux), https://commons.wikimedia.org/wiki/File:Thomson_MO5_face.JPG (CC BY-SA 3.0).
    // MO6: Philippe Dubois, https://commons.wikimedia.org/wiki/File:MO6.jpg ; license on the file page.
    // Physical base RAM including video RAM, without optional expansions; MAME thomson.cpp hardware documentation.
    internal static IReadOnlyList<HardwareModel> All { get; } = [new(MachineModel.Mo5, "Motorola MC6809E", 1_000_000, 49_152), new(MachineModel.Mo6, "Motorola MC6809E", 1_000_000, 131_072)];
}
