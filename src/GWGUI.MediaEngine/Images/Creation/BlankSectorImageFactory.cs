using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Images.Conversion.Fat12;
using GWGUI.MediaEngine.Images.Formats.Floppy.Adf;
using GWGUI.MediaEngine.Images.Formats.Floppy.Apple;
using GWGUI.MediaEngine.Images.Formats.Floppy.CommodoreDos;
using GWGUI.MediaEngine.Images.Formats.Floppy.Raw;
using GWGUI.MediaEngine.Images.Models.Sectors;
using GWGUI.MediaEngine.Images.Reading.Reconstruction.Sectors;
using GWGUI.MediaEngine.Images.Reading.Reconstruction.Sectors.Apple;
using GWGUI.MediaFileSystems.Formats.Commodore;

namespace GWGUI.MediaEngine.Images.Creation;

/// <summary>Construit les images sectorielles vierges avant l'injection de fichiers.</summary>
internal static class BlankSectorImageFactory
{
    public static SectorImage Create(string formatId)
    {
        if (Fat12TargetGeometryCatalog.TryResolve(formatId, out var fat))
            return CreateLinearBlank(formatId, fat.SectorSize, fat.Cylinders, fat.Heads, fat.SectorsPerTrack,
                SectorNumbering.OneBased);
        if (formatId is DiskImageFormatIds.AmigaDos or DiskImageFormatIds.AmigaDosHighDensity)
        {
            var geometry = formatId == DiskImageFormatIds.AmigaDos
                ? AmigaAdfGeometry.DoubleDensity : AmigaAdfGeometry.HighDensity;
            return CreateLinearBlank(formatId, geometry.BlockSize, geometry.Cylinders, geometry.Heads,
                geometry.SectorsPerTrack, SectorNumbering.ZeroBased);
        }
        if (formatId is DiskImageFormatIds.AppleIIAppleDos113 or DiskImageFormatIds.AppleIIAppleDos140)
            return CreateLinearBlank(formatId, AppleIIGeometry.SectorSize, AppleIIGeometry.TrackCount, 1,
                formatId == DiskImageFormatIds.AppleIIAppleDos113
                    ? AppleIIGeometry.Dos32SectorsPerTrack : AppleIIGeometry.SectorsPerTrack,
                SectorNumbering.ZeroBased);
        if (formatId is DiskImageFormatIds.AppleIIProDos140 or DiskImageFormatIds.AppleIIISos)
            return CreateLinearBlank(formatId, AppleIIGeometry.ProDosBlockSize, AppleIIGeometry.TrackCount,
                1, AppleIIGeometry.ProDosBlocksPerTrack, SectorNumbering.ZeroBased);
        if (formatId == DiskImageFormatIds.AppleIIProDos800)
        {
            var geometry = MacintoshGcrGeometry.ForHeads(MacintoshGcrGeometry.DoubleSidedHeadCount);
            return MacintoshGcrSectorImageBuilder.Create(new byte[geometry.Capacity], formatId, geometry);
        }
        if (formatId is DiskImageFormatIds.Commodore1541 or DiskImageFormatIds.Commodore1571)
        {
            var tracks = Commodore1541Geometry.StandardTrackCount;
            var sides = formatId == DiskImageFormatIds.Commodore1541 ? 1 : Commodore1571Geometry.SideCount;
            var count = Commodore1541Geometry.BlocksPerSide(tracks) * sides;
            return Commodore1541SectorImageBuilder.Create(new byte[count * Commodore1541Geometry.SectorSize],
                formatId, tracks, sides, count, null,
                (expected, actual) => new InvalidDataException($"Expected {expected} error entries; found {actual}."),
                CancellationToken.None);
        }
        if (formatId == DiskImageFormatIds.Commodore1581)
            return CreateLinearBlank(formatId, Commodore1581Geometry.LogicalBlockSize,
                Commodore1581Geometry.LogicalCylinderCount, Commodore1581Geometry.LogicalHeadCount,
                Commodore1581Geometry.LogicalBlocksPerTrack, SectorNumbering.ZeroBased);
        throw new ArgumentException($"Unsupported migration target format '{formatId}'.", nameof(formatId));
    }

    private static SectorImage CreateLinearBlank(string formatId, int blockSize, int cylinders,
        int heads, int sectorsPerTrack, SectorNumbering numbering)
    {
        var geometry = new LinearSectorImageGeometry(blockSize, cylinders, heads, sectorsPerTrack, numbering);
        return LinearSectorImageBuilder.Create(new byte[geometry.Capacity], formatId, geometry);
    }

}
