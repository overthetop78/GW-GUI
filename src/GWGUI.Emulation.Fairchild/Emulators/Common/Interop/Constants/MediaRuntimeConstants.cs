namespace GWGUI.Emulation.Fairchild.Emulators.Common.Interop.Constants;

internal static class MediaRuntimeConstants
{
    internal const string ConsoleFirmwareFieldPrefix = "firmware.";
    internal const string OriginalBios1FileName = "sl31253.bin";
    internal const string SystemIIBios1FileName = "sl90025.bin";
    internal const string Bios2FileName = "sl31254.bin";
    internal const string OriginalBios1Md5 = "ac9804d4c0e9d07e33472e3726ed15c3";
    internal const string SystemIIBios1Md5 = "95d339631d867c8f1d15a5f2ec26069d";
    internal const string Bios2Md5 = "da98f4bb3242ab80d76629021bb27585";
    internal const int BiosBytes = 1024;
    internal static readonly IReadOnlyList<string> CartridgeExtensions = [".bin", ".rom", ".chf"];

    internal static string FileName(MachineModel model, FirmwareCategory role) => role switch
    {
        FirmwareCategory.Bios1 => model == MachineModel.ChannelFSystemII ? SystemIIBios1FileName : OriginalBios1FileName,
        FirmwareCategory.Bios2 => Bios2FileName,
        _ => throw new ArgumentOutOfRangeException(nameof(role))
    };
    internal static string Md5(MachineModel model, FirmwareCategory role) => role switch
    {
        FirmwareCategory.Bios1 => model == MachineModel.ChannelFSystemII ? SystemIIBios1Md5 : OriginalBios1Md5,
        FirmwareCategory.Bios2 => Bios2Md5,
        _ => throw new ArgumentOutOfRangeException(nameof(role))
    };
}
