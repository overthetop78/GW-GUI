namespace GWGUI.Emulation.Epoch.Emulators.Common.Interop.Constants;

internal static class MediaRuntimeConstants
{
    internal const string ConsoleFirmwareFileName = "upd7801g.s01";
    internal static readonly IReadOnlyList<string> ScvCartridgeExtensions = [".cart", ".bin", ".rom", ".0", ".zip"];
    internal static readonly IReadOnlyList<string> PdCartridgeExtensions = [PdProgramExtension, PdPatternExtension, ".zip"];
    internal const string PdProgramExtension = ".bin777";
    internal const string PdPatternExtension = ".ptn777";
    internal const string PdProgramFileName = "cartridge.bin777";
    internal const string PdPatternFileName = "cartridge.ptn777";
    internal const int PdRawProgramBytes = 0xF00;
    internal const int PdProgramHeaderBytes = 0x100;
    internal const int PdProgramRecordBytes = 4;
    internal const string PdProgramMagic = "_CassetteVision_000";
    internal const byte PdProgramVersionZero = (byte)'0';
    internal const byte PdProgramVersionOne = (byte)'1';
    internal const int PdRawPatternBytes = 0x4D0;
    internal const int PdFormattedPatternBytes = 0x340;
    internal const string PdPatternMagic = "*CassetteVision*0000";
    internal const string ArchiveExtension = "zip";
    internal const string MultipartExtension = ".0";
    internal const int FirstCompanionPart = 1;
    internal const int LastCompanionPart = 3;
    internal const string EpochConsole = "EPOCH";
    internal const string YenoConsole = "YENO";
    internal const string LadyConsole = "EPOCHLADY";

    internal static string ConsoleModel(MachineModel model) => model switch
    {
        MachineModel.SuperCassetteVision => EpochConsole,
        MachineModel.SuperCassetteVisionYeno => YenoConsole,
        MachineModel.SuperCassetteVisionLady => LadyConsole,
        _ => throw new ArgumentOutOfRangeException(nameof(model))
    };
}
