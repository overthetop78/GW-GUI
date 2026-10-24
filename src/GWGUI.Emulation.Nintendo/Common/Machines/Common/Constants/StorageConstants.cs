using CommonCoreHostFunctions = GWGUI.Emulation.Nintendo.Common.Functions.CoreHostFunctions;

namespace GWGUI.Emulation.Nintendo.Common.Machines.Common.Constants;

internal static class StorageSettingsFunctionsConstants
{
    internal const string PokemonMiniExtension = ".min";
    internal const string M3u = ".m3u";
    internal const string Cue = ".cue";
    internal static readonly string Chd = CommonCoreHostFunctions.MediaProcessing.FileExtensions["Chd"];
    internal static readonly string Iso = CommonCoreHostFunctions.MediaProcessing.FileExtensions["Iso"];
    internal static readonly string Gcm = CommonCoreHostFunctions.MediaProcessing.FileExtensions["Gcm"];
    internal static readonly string Wud = CommonCoreHostFunctions.MediaProcessing.FileExtensions["Wud"];
    internal static readonly string Wux = CommonCoreHostFunctions.MediaProcessing.FileExtensions["Wux"];
    internal const string FloppyDriveCountOption = "storage.floppyDriveCount";
    internal const string CassetteDriveEnabledOption = "storage.cassetteDriveEnabled";
    internal const string CartridgeSlotEnabledOption = "storage.cartridgeSlotEnabled";
    internal const string CompactDiscDriveEnabledOption = "storage.compactDiscDriveEnabled";
    internal const string FloppyDriveLabel = "A:";
    internal const string SecondFloppyDriveLabel = "B:";
    internal const string CassetteDriveLabel = "Cassette";
    internal const string CartridgeSlotLabel = "Cartridge";
    internal const string CompactDiscDriveLabel = "Optical disc";

    internal static readonly IReadOnlyList<string> FamicomDiskExtensions = [CommonCoreHostFunctions.MediaProcessing.FileExtensions["Fds"]];

    internal static readonly IReadOnlyList<string> NesExtensions = [CommonCoreHostFunctions.MediaProcessing.FileExtensions["Nes"]];

    internal static readonly IReadOnlyList<string> SnesExtensions = [CommonCoreHostFunctions.MediaProcessing.FileExtensions["Sfc"], CommonCoreHostFunctions.MediaProcessing.FileExtensions["Smc"]];

    internal static readonly IReadOnlyList<string> VirtualBoyExtensions = [CommonCoreHostFunctions.MediaProcessing.FileExtensions["Vb"]];

    internal static readonly IReadOnlyList<string> Nintendo64Extensions = [CommonCoreHostFunctions.MediaProcessing.FileExtensions["N64"], CommonCoreHostFunctions.MediaProcessing.FileExtensions["Z64"], CommonCoreHostFunctions.MediaProcessing.FileExtensions["V64"]];

    internal static readonly IReadOnlyList<string> GameBoyExtensions = [CommonCoreHostFunctions.MediaProcessing.FileExtensions["Gb"]];

    internal static readonly IReadOnlyList<string> GameBoyColorExtensions = [CommonCoreHostFunctions.MediaProcessing.FileExtensions["Gbc"], CommonCoreHostFunctions.MediaProcessing.FileExtensions["Cgb"]];

    internal static readonly IReadOnlyList<string> GameBoyAdvanceExtensions = [CommonCoreHostFunctions.MediaProcessing.FileExtensions["Gba"]];

    internal static readonly IReadOnlyList<string> NintendoDsExtensions = [CommonCoreHostFunctions.MediaProcessing.FileExtensions["Nds"]];

    internal static readonly IReadOnlyList<string> NintendoDsiExtensions = [".nds", ".dsi", ".ids"];

    internal static readonly IReadOnlyList<string> GameWatchExtensions = [CommonCoreHostFunctions.MediaProcessing.FileExtensions["Mgw"]];

    internal static readonly IReadOnlyList<string> Nintendo3DsExtensions =
        [CommonCoreHostFunctions.MediaProcessing.FileExtensions["ThreeDs"], CommonCoreHostFunctions.MediaProcessing.FileExtensions["Cia"],
         CommonCoreHostFunctions.MediaProcessing.FileExtensions["ThreeDsx"], CommonCoreHostFunctions.MediaProcessing.FileExtensions["Cci"],
         CommonCoreHostFunctions.MediaProcessing.FileExtensions["Cxi"], CommonCoreHostFunctions.MediaProcessing.FileExtensions["Axf"],
         CommonCoreHostFunctions.MediaProcessing.FileExtensions["Elf"], CommonCoreHostFunctions.MediaProcessing.FileExtensions["App"]];

    internal static readonly IReadOnlyList<string> OpticalExtensions = [Cue, Chd, Iso, Gcm];

    internal static readonly IReadOnlyList<string> WiiUExtensions = [Wud, Wux];
}
