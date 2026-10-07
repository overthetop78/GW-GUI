namespace GWGUI.Emulation.Nec.Emulators.BeetlePceFast.Constants;

internal static class BeetlePceFastStorageConstants
{
    internal const string PceExtension = ".pce";
    internal const string IsoExtension = ".iso";
    internal const string ImgExtension = ".img";
    internal const string BinExtension = ".bin";
    internal const string CdDriveEnabledOption = "storage.cdDriveEnabled";
    internal const string CartridgeSlotLabel = "HuCARD";
    internal const string DefaultCdSpeed = "1";
    internal const string HuCardSlotResourceKey = "Emulation.Nec.Storage.HuCardSlot";
    internal const string DuoCdRomResourceKey = "Emulation.Nec.Storage.DuoCdRom";
    internal const string CdRom2ResourceKey = "Emulation.Nec.Controller.CdRom2";
    internal const string CdIgnoreErrorsResourceKey = "Emulation.Nec.Storage.CdIgnoreErrors";
    internal static readonly string[] CdSpeeds = ["1", "2", "4", "8"];
}
