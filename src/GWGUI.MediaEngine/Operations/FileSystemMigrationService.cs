using System.IO;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Images.Conversion.Fat12;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Images.Formats.Floppy.Adf;
using GWGUI.MediaEngine.Images.Formats.Floppy.Apple;
using GWGUI.MediaEngine.Images.Formats.Floppy.CommodoreDos;
using GWGUI.MediaEngine.Images.Formats.Floppy.D81;
using GWGUI.MediaEngine.Images.Formats.Floppy.Raw;
using GWGUI.MediaEngine.Images.Formats.Floppy.St;
using GWGUI.MediaEngine.Images.Formats.Floppy.TwoImg;
using GWGUI.MediaEngine.Images.Reading.Reconstruction.Sectors;
using GWGUI.MediaEngine.Images.Reading.Reconstruction.Sectors.Apple;
using GWGUI.MediaEngine.Images.Models.Sectors;
using GWGUI.MediaFileSystems.Formats.Commodore;
using FileSystemsMigrationService = GWGUI.MediaFileSystems.Migration.FileSystemMigrationService;
using FileSystemsReport = GWGUI.MediaFileSystems.Migration.MigrationValidationReport;

namespace GWGUI.MediaEngine.Operations;

/// <summary>Entrée média : crée le support vierge, demande l'injection puis écrit le format physique.</summary>
public sealed class FileSystemMigrationService
{
    private readonly FileSystemsMigrationService fileSystems = new();

    public MigrationValidationReport Validate(FileSystemVolume source, string targetFormatId,
        bool acceptMetadataLoss = false)
    {
        var target = FileSystemMigrationTargetCatalog.Get(targetFormatId);
        var blank = CreateBlankImage(target.FormatId);
        var plan = fileSystems.CreatePlan(MediaFileSystemsReaderAdapter.ToFileSystemsVolume(source), target.FileSystemId);
        return ConvertReport(fileSystems.Validate(plan, blank, acceptMetadataLoss));
    }

    public MigrationResult CreateImage(FileSystemVolume source, string targetFormatId,
        bool acceptMetadataLoss = false)
    {
        var target = FileSystemMigrationTargetCatalog.Get(targetFormatId);
        var blank = CreateBlankImage(target.FormatId);
        var plan = fileSystems.CreatePlan(MediaFileSystemsReaderAdapter.ToFileSystemsVolume(source), target.FileSystemId);
        var (filled, report) = fileSystems.Inject(plan, blank, acceptMetadataLoss);
        var blocks = filled.AvailableBlocks.Select(block => new SectorBlock(block.LogicalBlock,
            new SectorAddress(block.Address.Cylinder, block.Address.Head, block.Address.Number), block.Data));
        var image = new SectorImage(filled.FormatId, filled.BlockSize, filled.Cylinders, filled.Heads,
            filled.SectorsPerTrack, blocks, capacity: filled.Capacity,
            logicalBlockCount: filled.LogicalBlockCount);
        return new(image, ConvertReport(report));
    }

    public async Task<MigrationResult> WriteAsync(FileSystemVolume source, string outputPath,
        string targetFormatId, bool acceptMetadataLoss = false,
        CancellationToken cancellationToken = default)
    {
        var result = CreateImage(source, targetFormatId, acceptMetadataLoss);
        await WriteImageAsync(result.Image, outputPath, targetFormatId, cancellationToken).ConfigureAwait(false);
        return result;
    }

    private static SectorImage CreateBlankImage(string formatId)
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

    private static MigrationValidationReport ConvertReport(FileSystemsReport report) =>
        new(report.Losses.Select(loss => new MigrationLoss(
            Enum.Parse<MigrationLossKind>(loss.Kind.ToString()), loss.Path, loss.IsBlocking, loss.Detail)),
            report.MetadataLossAccepted);

    private static async Task WriteImageAsync(SectorImage image, string outputPath,
        string formatId, CancellationToken cancellationToken)
    {
        if (formatId.StartsWith(DiskImageFormatIds.AmigaPrefix, StringComparison.OrdinalIgnoreCase))
        {
            await new AmigaAdfWriter().WriteAsync(image, outputPath, cancellationToken).ConfigureAwait(false);
            return;
        }
        if (Fat12TargetGeometryCatalog.TryResolve(formatId, out _))
        {
            var linear = new LinearSectorImageWriter();
            var fatWriter = new Fat12TargetImageWriter(new AtariStWriter(linear),
                new IbmRawImageWriter(linear), new MsxRawImageWriter(linear));
            await fatWriter.WriteAsync(image, outputPath, formatId, cancellationToken).ConfigureAwait(false);
            return;
        }
        if (formatId.StartsWith(DiskImageFormatIds.AppleIIPrefix, StringComparison.OrdinalIgnoreCase) ||
            formatId.StartsWith(DiskImageFormatIds.AppleIIIPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var extension = Path.GetExtension(outputPath);
            if (extension.Equals(DiskImageFileExtensions.TwoMg, StringComparison.OrdinalIgnoreCase))
                await new TwoImgWriter().WriteAsync(image, outputPath, formatId, cancellationToken).ConfigureAwait(false);
            else if (AppleDiskImageWriter.SupportsExtension(extension) &&
                formatId is not (DiskImageFormatIds.AppleIIProDos800 or DiskImageFormatIds.AppleIIISos))
                await new AppleDiskImageWriter().WriteAsync(image, outputPath, cancellationToken).ConfigureAwait(false);
            else if (GWGUI.MediaEngine.Images.Conversion.Apple.AppleSectorConversionService.CanCreate(formatId, extension))
                await new AppleRawImageWriter().WriteAsync(image, outputPath, formatId, cancellationToken).ConfigureAwait(false);
            else throw new InvalidDataException($"The Apple migration target '{formatId}' cannot be written to '{extension}'.");
            return;
        }
        var suffix = Path.GetExtension(outputPath);
        if ((formatId is DiskImageFormatIds.Commodore1541 or DiskImageFormatIds.Commodore1571) &&
            suffix.Equals(formatId == DiskImageFormatIds.Commodore1541
                ? DiskImageFileExtensions.D64 : DiskImageFileExtensions.D71, StringComparison.OrdinalIgnoreCase))
            await new CommodoreDosContainerWriter().WriteAsync(image, outputPath,
                cancellationToken: cancellationToken).ConfigureAwait(false);
        else if (formatId == DiskImageFormatIds.Commodore1581 &&
            suffix.Equals(DiskImageFileExtensions.D81, StringComparison.OrdinalIgnoreCase))
            await new D81Writer(new LinearSectorImageWriter()).WriteAsync(image, outputPath,
                cancellationToken).ConfigureAwait(false);
        else throw new InvalidDataException($"The Commodore DOS migration target '{formatId}' cannot be written to '{suffix}'.");
    }
}
