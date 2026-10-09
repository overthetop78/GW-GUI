namespace GWGUI.Emulation.Sony.Common.Machines.PlayStation.Constants;

internal static class FirmwareConstants
{
    internal const string JapanField = "firmware.japan";
    internal const string NorthAmericaField = "firmware.northamerica";
    internal const string EuropeField = "firmware.europe";
    internal const string PspField = "firmware.psp";
    internal const string Ps3Field = "firmware.ps3";
    internal const string OpenBiosField = "firmware.openbios";
    internal const string JapanFile = "scph5500.bin";
    internal const string NorthAmericaFile = "scph5501.bin";
    internal const string EuropeFile = "scph5502.bin";
    internal const string PspFile = "psxonpsp660.bin";
    internal const string Ps3File = "ps1_rom.bin";
    internal const string OpenBiosFile = "openbios.bin";
    internal const string JapanLabel = "Emulation.Sony.Firmware.PlayStation.Japan";
    internal const string NorthAmericaLabel = "Emulation.Sony.Firmware.PlayStation.NorthAmerica";
    internal const string EuropeLabel = "Emulation.Sony.Firmware.PlayStation.Europe";
    internal const string PspLabel = "Emulation.Sony.Firmware.PlayStation.Psp";
    internal const string Ps3Label = "Emulation.Sony.Firmware.PlayStation.Ps3";
    internal const string OpenBiosLabel = "Emulation.Sony.Firmware.PlayStation.OpenBios";

    internal static FirmwareSlot Japan { get; } = new(JapanField, JapanFile, JapanLabel, false, JapanFile);
    internal static FirmwareSlot NorthAmerica { get; } = new(NorthAmericaField, NorthAmericaFile, NorthAmericaLabel, false, NorthAmericaFile);
    internal static FirmwareSlot Europe { get; } = new(EuropeField, EuropeFile, EuropeLabel, false, EuropeFile);
    internal static FirmwareSlot Psp { get; } = new(PspField, PspFile, PspLabel, false, PspFile);
    internal static FirmwareSlot Ps3 { get; } = new(Ps3Field, Ps3File, Ps3Label, false, Ps3File);
    internal static FirmwareSlot OpenBios { get; } = new(OpenBiosField, OpenBiosFile, OpenBiosLabel, false, OpenBiosFile);
}
