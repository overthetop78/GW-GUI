namespace GWGUI.Emulation.Fairchild.Emulators.Common.Interop.Constants;

internal static class MediaRuntimeConstants
{
    internal const string OriginalBios1FileName = "sl31253.bin";
    internal const string SystemIIBios1FileName = "sl90025.bin";
    internal const string Bios2FileName = "sl31254.bin";
    internal static readonly IReadOnlyList<string> CartridgeExtensions = [".bin", ".rom", ".chf"];

    internal static string FileName(MachineModel model, FirmwareCategory role) => role switch
    {
        FirmwareCategory.Bios1 => model == MachineModel.ChannelFSystemII ? SystemIIBios1FileName : OriginalBios1FileName,
        FirmwareCategory.Bios2 => Bios2FileName,
        _ => throw new ArgumentOutOfRangeException(nameof(role))
    };
}
