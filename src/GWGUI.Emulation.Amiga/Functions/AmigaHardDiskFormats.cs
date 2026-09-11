using GWGUI.Emulation.HardDisks;

namespace GWGUI.Emulation.Amiga.Functions;

internal static class AmigaHardDiskFormats
{
    private static readonly DiskFormatRegistry Formats = DiskFormatRegistry.CreateDefault();

    // Raw single-volume hardfiles use the UAE virtual controller. Stay below
    // the classic filesystem's 2 GiB signed-offset boundary. This is a safe
    // supported profile, not a claim that all Amiga controllers stop at 2 GiB.
    internal static readonly IReadOnlyList<HardDiskImageFormat> All = Describe(
        [new("amiga-hdf", ".hdf", "UAE", 2L * 1024 * 1024 * 1024 - 512, 40L * 1024 * 1024,
            [HardDiskPreparation.Blank, HardDiskPreparation.AmigaOfs, HardDiskPreparation.AmigaFfs,
             HardDiskPreparation.AmigaRdbOfs, HardDiskPreparation.AmigaRdbFfs]),
         new("amiga-hdz", ".hdz", "UAE", 2L * 1024 * 1024 * 1024 - 512, 40L * 1024 * 1024,
             [HardDiskPreparation.Blank, HardDiskPreparation.AmigaOfs, HardDiskPreparation.AmigaFfs],
             GWGUI.Emulation.HardDisks.Containers.DiskContainerKind.Gzip)]);

    private static IReadOnlyList<HardDiskImageFormat> Describe(IEnumerable<HardDiskImageFormat> formats) =>
        formats.Select(Formats.Describe).ToArray();
}
