using GWGUI.MediaEngine.Constants;

namespace GWGUI.Emulation.Sega.Common.Machines.Common.Constants;

internal static class StorageSettingsFunctionsConstants
{
    internal const string Dsk = ".dsk";
    internal const string M3u = ".m3u";
    internal const string Cdt = ".cdt";
    internal const string Tap = ".tap";
    internal const string Voc = ".voc";
    internal const string Sms = ".sms";
    internal const string Sg = ".sg";
    internal const string Md = DiskImageFileExtensions.Md;
    internal const string Mdx = DiskImageFileExtensions.Mdx;
    internal const string Sgd = DiskImageFileExtensions.Sgd;
    internal const string Smd = DiskImageFileExtensions.Smd;
    internal const string Bms = DiskImageFileExtensions.Bms;
    internal const string SixtyEightK = DiskImageFileExtensions.SixtyEightK;
    internal const string Gen = DiskImageFileExtensions.Gen;
    internal const string ThirtyTwoX = ".32x";
    internal const string Mv = ".mv";
    internal const string Cpr = ".cpr";
    internal const string Cue = ".cue";
    internal const string Ccd = DiskImageFileExtensions.Ccd;
    internal const string Chd = ".chd";
    internal const string Iso = ".iso";
    internal const string Gdi = ".gdi";
    internal const string Cdi = ".cdi";
    internal const string Zip = ".zip";
    internal const string Bin = ".bin";
    internal const string Sc = ".sc";
    internal const string Gg = ".gg";
    internal const string Rom = ".rom";
    internal const string Mds = ".mds";
    internal const string Cas = ".cas";
    internal static IReadOnlySet<string> FloppyExtensions { get; } = new HashSet<string>([Dsk], StringComparer.OrdinalIgnoreCase);
    internal static IReadOnlySet<string> CassetteExtensions { get; } = new HashSet<string>([Cas, Tap, Cdt, Voc], StringComparer.OrdinalIgnoreCase);
    internal static IReadOnlySet<string> OpticalExtensions { get; } = new HashSet<string>([Cue, Ccd, Chd, Iso, Mds, Zip, Gdi, Cdi, M3u], StringComparer.OrdinalIgnoreCase);
    internal static IReadOnlySet<string> MegaDriveCartridgeExtensions { get; } = new HashSet<string>([Md, Mdx, Sgd, Smd, Bms, SixtyEightK, Gen, Bin, ThirtyTwoX], StringComparer.OrdinalIgnoreCase);
    internal static IReadOnlySet<string> EightBitCartridgeExtensions { get; } = new HashSet<string>([Sms, Sg, Sc, Gg, Rom, Bin, Zip], StringComparer.OrdinalIgnoreCase);
    internal static IReadOnlySet<string> ArcadeCartridgeExtensions { get; } = new HashSet<string>([Zip, Bin, Dat, Lst], StringComparer.OrdinalIgnoreCase);
    internal const string Dat = ".dat";
    internal const string Lst = ".lst";
    internal const string FloppyDriveCountOption = "storage.floppyDriveCount";
    internal const string CassetteDriveEnabledOption = "storage.cassetteDriveEnabled";
    internal const string CartridgeSlotEnabledOption = "storage.cartridgeSlotEnabled";
    internal const string SegaCardSlotEnabledOption = "storage.segaCardSlotEnabled";
    internal const string CompactDiscDriveEnabledOption = "storage.compactDiscDriveEnabled";
    internal const string FloppyDriveLabel = "A:";
    internal const string SecondFloppyDriveLabel = "B:";
    internal const string CassetteDriveLabel = "Cassette";
    internal const string CartridgeSlotLabel = "Cartridge";
    internal const string SegaCardSlotLabel = "Sega Card";
    internal const string CompactDiscDriveLabel = "Optical disc";
}
