namespace GWGUI.Emulation.Wenquxing.Common.Machines.ElectronicDictionaries.Dictionaries;
internal static class HardwareModelCatalog
{
    // Machine photographs and printed brand: AloysHF/WQXEmu res, revision 05e34f90afe9ef448eb7198e443fbce8031c781c.
    // WQXEmu pinned sources: timer.rs, memory.rs and each machine implementation.
    // NC2000/NC3000: documented INTERNAL_RAM_SIZE only; do not report their larger host buffer as total physical RAM.
    internal static IReadOnlyList<HardwareModel> All { get; } = [
        new(MachineModel.NC1020, "6502", 5_120_000, 32_768),
        new(MachineModel.PC1000, "6502", 5_120_000, 65_536),
        new(MachineModel.CC800, "6502", 5_120_000, 65_536),
        new(MachineModel.NC2000, "6502", 5_120_000, 24_576, "Emulation.Wenquxing.Memory.Internal"),
        new(MachineModel.NC3000, "6502", 10_240_000, 24_576, "Emulation.Wenquxing.Memory.Internal")
    ];
}