namespace GWGUI.Emulation.Wenquxing.Emulators.Wqxemu.Dictionaries;

internal static class FirmwareCatalog
{
    internal static IReadOnlyList<FirmwareRequirement> Requirements(MachineModel model) => model switch
    {
        MachineModel.NC1020 => [Entry(FirmwareRole.SystemRom, "obj_lu.bin"), Entry(FirmwareRole.NorFlash, "nc1020.fls")],
        MachineModel.PC1000 => [Entry(FirmwareRole.SystemRom, "pc1000.rom"), Entry(FirmwareRole.NorFlash, "pc1000.fls")],
        MachineModel.CC800 => [Entry(FirmwareRole.SystemRom, "obj.bin"), Entry(FirmwareRole.NorFlash, "cc800.fls")],
        MachineModel.NC2000 => [Entry(FirmwareRole.NorFlash, "nc2000.nor"), Entry(FirmwareRole.NandFlash, "nc2000.nand"), Entry(FirmwareRole.NandFirstPlane, "nc2000.nand0")],
        MachineModel.NC3000 => [Entry(FirmwareRole.NorFlash, "nc3000.nor"), Entry(FirmwareRole.NandFlash, "nc3000.nand"), Entry(FirmwareRole.NandFirstPlane, "nc3000.nand0", false)],
        _ => throw new ArgumentOutOfRangeException(nameof(model))
    };
    private static FirmwareRequirement Entry(FirmwareRole role, string file, bool required = true) => new(role, FirmwareFunctions.Field(role), role == FirmwareRole.SystemRom ? "Emulation.Firmware.Rom.System" : "Emulation.Wenquxing.Firmware." + role, [file], required);
}