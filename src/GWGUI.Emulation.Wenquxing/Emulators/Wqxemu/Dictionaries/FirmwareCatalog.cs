namespace GWGUI.Emulation.Wenquxing.Emulators.Wqxemu.Dictionaries;

internal static class FirmwareCatalog
{
    internal static IReadOnlyList<FirmwareSlot> Requirements(MachineModel model) => model switch
    {
        MachineModel.NC1020 => [Entry(FirmwareCategory.SystemRom, "obj_lu.bin"), Entry(FirmwareCategory.NorFlash, "nc1020.fls")],
        MachineModel.PC1000 => [Entry(FirmwareCategory.SystemRom, "pc1000.rom"), Entry(FirmwareCategory.NorFlash, "pc1000.fls")],
        MachineModel.CC800 => [Entry(FirmwareCategory.SystemRom, "obj.bin"), Entry(FirmwareCategory.NorFlash, "cc800.fls")],
        MachineModel.NC2000 => [Entry(FirmwareCategory.NorFlash, "nc2000.nor"), Entry(FirmwareCategory.NandFlash, "nc2000.nand"), Entry(FirmwareCategory.NandFirstPlane, "nc2000.nand0")],
        MachineModel.NC3000 => [Entry(FirmwareCategory.NorFlash, "nc3000.nor"), Entry(FirmwareCategory.NandFlash, "nc3000.nand"), Entry(FirmwareCategory.NandFirstPlane, "nc3000.nand0", false)],
        _ => throw new ArgumentOutOfRangeException(nameof(model))
    };
    private static FirmwareSlot Entry(FirmwareCategory role, string file, bool required = true) => new(FirmwareFunctions.Field(role), file, role == FirmwareCategory.SystemRom ? "Emulation.Firmware.Rom.System" : "Emulation.Wenquxing.Firmware." + role, required, CandidateFileNames: [file]);
}