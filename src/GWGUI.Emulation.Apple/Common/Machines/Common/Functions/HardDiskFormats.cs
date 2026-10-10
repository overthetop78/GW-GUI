using GWGUI.Emulation.HardDisks;

namespace GWGUI.Emulation.Apple.Common.Machines.Common.Functions;

internal static class HardDiskFormats
{
    private const int SectorBytes = 512;
    private const long AppleWinMaximumBlocks = 0x007FFFFF;
    private const long DefaultBytes = 32L * 1024 * 1024;
    private const string AppleWinFormatId = "applewin-hdv";
    private const string MacintoshFormatId = "minivmac-raw";
    private const string AppleWinExtension = ".hdv";
    private const string MacintoshExtension = ".img";
    private const string AppleWinInterface = "SmartPort";
    private const string MacintoshInterface = "Macintosh";
    private static readonly DiskFormatRegistry Formats = DiskFormatRegistry.CreateDefault();

    internal static IReadOnlyList<HardDiskImageFormat> For(MachineConfiguration configuration)
    {
        if (!ConfigurationFunctions.SupportsRuntime(configuration.Model)) return [];
        // AppleWin Harddisk.h limits reported blocks; Mini vMac vSonyGetSize uses a 32-bit byte count.
        var appleII = configuration.Core == Emulator.AppleWin;
        var maximum = appleII ? AppleWinMaximumBlocks * SectorBytes : uint.MaxValue / SectorBytes * (long)SectorBytes;
        return [Formats.Describe(new(appleII ? AppleWinFormatId : MacintoshFormatId,
            appleII ? AppleWinExtension : MacintoshExtension,
            appleII ? AppleWinInterface : MacintoshInterface, maximum, DefaultBytes,
            [HardDiskPreparation.Blank]))];
    }
}
