using GWGUI.MediaEngine.Contracts.Migration;
using GWGUI.MediaEngine.Contracts.Explorer;
using System.IO;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Images.Conversion.Fat12;
using GWGUI.MediaEngine.Images.Creation;
using GWGUI.MediaEngine.Images.Formats.Floppy.Adf;
using GWGUI.MediaEngine.Images.Formats.Floppy.Apple;
using GWGUI.MediaEngine.Images.Formats.Floppy.CommodoreDos;
using GWGUI.MediaEngine.Images.Formats.Floppy.D81;
using GWGUI.MediaEngine.Images.Formats.Floppy.Raw;
using GWGUI.MediaEngine.Images.Formats.Floppy.St;
using GWGUI.MediaEngine.Images.Formats.Floppy.TwoImg;
using GWGUI.MediaEngine.Images.Models.Sectors;
using FileSystemsMigrationService = GWGUI.MediaFileSystems.Migration.FileSystemMigrationService;
using FileSystemsReport = GWGUI.MediaFileSystems.Migration.MigrationValidationReport;

namespace GWGUI.MediaEngine.Images.Creation;

/// <summary>Entrée média : crée le support vierge, demande l'injection puis écrit le format physique.</summary>
public sealed class FileSystemMigrationService
{
    private readonly FileSystemsMigrationService fileSystems = new();

    public MigrationValidationReport Validate(FileSystemVolume source, string targetFormatId,
        bool acceptMetadataLoss = false)
    {
        var target = FileSystemMigrationTargetCatalog.Get(targetFormatId);
        var blank = BlankSectorImageFactory.Create(target.FormatId);
        var plan = fileSystems.CreatePlan(FileSystemVolumeMapper.ToFileSystemsVolume(source), target.FileSystemId);
        return ConvertReport(fileSystems.Validate(plan, blank, acceptMetadataLoss));
    }

    public MigrationResult CreateImage(FileSystemVolume source, string targetFormatId,
        bool acceptMetadataLoss = false)
    {
        var target = FileSystemMigrationTargetCatalog.Get(targetFormatId);
        var blank = BlankSectorImageFactory.Create(target.FormatId);
        var plan = fileSystems.CreatePlan(FileSystemVolumeMapper.ToFileSystemsVolume(source), target.FileSystemId);
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
