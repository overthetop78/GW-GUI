using GWGUI.MediaEngine.Constants;

namespace GWGUI.Emulation.Nintendo.Common.Machines.Common.Constants;

internal static class StorageSettingsFunctionsConstants
{
    internal const string PokemonMiniExtension = ".min";
    internal const string M3u = ".m3u";
    internal const string Cue = ".cue";
    internal const string Chd = DiskImageFileExtensions.Chd;
    internal const string Iso = DiskImageFileExtensions.Iso;
    internal const string Gcm = DiskImageFileExtensions.Gcm;
    internal const string Wud = DiskImageFileExtensions.Wud;
    internal const string Wux = DiskImageFileExtensions.Wux;
    internal const string FloppyDriveCountOption = "storage.floppyDriveCount";
    internal const string CassetteDriveEnabledOption = "storage.cassetteDriveEnabled";
    internal const string CartridgeSlotEnabledOption = "storage.cartridgeSlotEnabled";
    internal const string CompactDiscDriveEnabledOption = "storage.compactDiscDriveEnabled";
    internal const string FloppyDriveLabel = "A:";
    internal const string SecondFloppyDriveLabel = "B:";
    internal const string CassetteDriveLabel = "Cassette";
    internal const string CartridgeSlotLabel = "Cartridge";
    internal const string CompactDiscDriveLabel = "Optical disc";

    internal static readonly IReadOnlyList<string> FamicomDiskExtensions =
        [DiskImageFileExtensions.Fds];

    internal static readonly IReadOnlyList<string> NesExtensions =
        [DiskImageFileExtensions.Nes];

    internal static readonly IReadOnlyList<string> SnesExtensions =
        [DiskImageFileExtensions.Sfc, DiskImageFileExtensions.Smc];

    internal static readonly IReadOnlyList<string> VirtualBoyExtensions =
        [DiskImageFileExtensions.Vb];

    internal static readonly IReadOnlyList<string> Nintendo64Extensions =
        [DiskImageFileExtensions.N64, DiskImageFileExtensions.Z64, DiskImageFileExtensions.V64];

    internal static readonly IReadOnlyList<string> GameBoyExtensions =
        [DiskImageFileExtensions.Gb];

    internal static readonly IReadOnlyList<string> GameBoyColorExtensions =
        [DiskImageFileExtensions.Gbc, DiskImageFileExtensions.Cgb];

    internal static readonly IReadOnlyList<string> GameBoyAdvanceExtensions =
        [DiskImageFileExtensions.Gba];

    internal static readonly IReadOnlyList<string> NintendoDsExtensions =
        [DiskImageFileExtensions.Nds];

    internal static readonly IReadOnlyList<string> NintendoDsiExtensions = [".nds", ".dsi", ".ids"];

    internal static readonly IReadOnlyList<string> GameWatchExtensions =
        [DiskImageFileExtensions.Mgw];

    internal static readonly IReadOnlyList<string> Nintendo3DsExtensions =
        [DiskImageFileExtensions.ThreeDs, DiskImageFileExtensions.Cia,
         DiskImageFileExtensions.ThreeDsx, DiskImageFileExtensions.Cci,
         DiskImageFileExtensions.Cxi, DiskImageFileExtensions.Axf,
         DiskImageFileExtensions.Elf, DiskImageFileExtensions.App];

    internal static readonly IReadOnlyList<string> OpticalExtensions =
        [Cue, Chd, Iso, Gcm];

    internal static readonly IReadOnlyList<string> WiiUExtensions =
        [Wud, Wux];
}
