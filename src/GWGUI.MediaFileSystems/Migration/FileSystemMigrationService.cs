using GWGUI.MediaFileSystems.Constants;
using GWGUI.MediaFileSystems.Contracts;
using GWGUI.MediaFileSystems.Definitions;
using GWGUI.MediaFileSystems.FileSystems.Amiga;
using GWGUI.MediaFileSystems.FileSystems.Apple.Dos;
using GWGUI.MediaFileSystems.FileSystems.Apple.ProDos;
using GWGUI.MediaFileSystems.FileSystems.Apple.Sos;
using GWGUI.MediaFileSystems.FileSystems.Commodore.Dos;
using GWGUI.MediaFileSystems.FileSystems.Fat12;
using GWGUI.MediaFileSystems.Interfaces;

namespace GWGUI.MediaFileSystems.Migration;

/// <summary>Injecte les vrais fichiers dans une image vierge fournie par le moteur.</summary>
public sealed class FileSystemMigrationService
{
    public MigrationPlan CreatePlan(FileSystemVolume source, string targetFileSystemId) =>
        MigrationPlanner.Create(source, targetFileSystemId);

    public MigrationValidationReport Validate(MigrationPlan plan, IMediaSectorImage blankImage,
        bool acceptMetadataLoss = false)
    {
        var capabilities = ResolveCapabilities(plan, blankImage);
        return MigrationValidator.Validate(plan, capabilities, acceptMetadataLoss);
    }

    public (MediaSectorWritePlan Image, MigrationValidationReport Report) Inject(
        MigrationPlan plan, IMediaSectorImage blankImage, bool acceptMetadataLoss = false)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(blankImage);
        var capabilities = ResolveCapabilities(plan, blankImage);
        var report = MigrationValidator.Validate(plan, capabilities, acceptMetadataLoss);
        MigrationValidator.EnsureExecutable(report);
        RequireBlankImage(blankImage);
        var writable = MigrationMetadataReducer.Reduce(plan, capabilities);
        var geometry = new MediaSectorGeometry(blankImage.FormatId, blankImage.BlockSize,
            blankImage.Cylinders, blankImage.Heads, blankImage.SectorsPerTrack, blankImage.BlockCount);
        var addresses = Enumerable.Range(0, blankImage.BlockCount).Select(index =>
        {
            blankImage.TryGetBlock(index, out var block);
            return new MediaSectorAddress(block.Cylinder, block.Head, block.PhysicalSectorNumber);
        }).ToArray();

        MediaSectorWritePlan filled = plan.TargetFileSystemId switch
        {
            FileSystemIds.AmigaDosFfs => new AmigaDosVolumeWriter().Create(writable, AmigaDosVariant.Ffs, geometry),
            FileSystemIds.AmigaDosOfs => new AmigaDosVolumeWriter().Create(writable, AmigaDosVariant.Ofs, geometry),
            FileSystemIds.Fat12 => new Fat12VolumeWriter().Create(writable,
                new Fat12TargetGeometry(blankImage.FormatId, blankImage.BlockSize,
                    blankImage.Cylinders, blankImage.Heads, blankImage.SectorsPerTrack,
                    blankImage.FormatId.StartsWith(MediaImageFormatIds.MsxPrefix, StringComparison.OrdinalIgnoreCase))),
            FileSystemIds.AppleDos => new AppleDosVolumeWriter().Create(writable, blankImage.FormatId),
            FileSystemIds.ProDos => new ProDosVolumeWriter().Create(writable, geometry, addresses),
            FileSystemIds.Sos => new SosVolumeWriter().Create(writable,
                geometry with { FormatId = MediaImageFormatIds.AppleIIProDos140 }, addresses, blankImage.FormatId),
            FileSystemIds.CommodoreDos => new CommodoreDosVolumeWriter().Create(writable, blankImage.FormatId),
            _ => throw new ArgumentException($"Unsupported migration file system '{plan.TargetFileSystemId}'.", nameof(plan))
        };

        if (!filled.FormatId.Equals(blankImage.FormatId, StringComparison.OrdinalIgnoreCase) ||
            filled.BlockSize != blankImage.BlockSize ||
            filled.Cylinders != blankImage.Cylinders || filled.Heads != blankImage.Heads ||
            filled.SectorsPerTrack != blankImage.SectorsPerTrack ||
            filled.AvailableBlocks.Count != blankImage.BlockCount)
            throw new InvalidOperationException("The filled image does not match the blank target image.");
        foreach (var block in filled.AvailableBlocks)
        {
            if (!blankImage.TryGetBlock(block.LogicalBlock, out var blank) ||
                block.Address.Cylinder != blank.Cylinder || block.Address.Head != blank.Head ||
                block.Address.Number != blank.PhysicalSectorNumber)
                throw new InvalidOperationException("The filled image changed a target sector address.");
        }
        var injectedBlocks = filled.AvailableBlocks.Select(block =>
        {
            blankImage.TryGetBlock(block.LogicalBlock, out var blank);
            return new MediaSectorWriteBlock(block.LogicalBlock,
                new MediaSectorAddress(blank.Cylinder, blank.Head, blank.PhysicalSectorNumber), block.Data);
        });
        var result = new MediaSectorWritePlan(blankImage.FormatId, blankImage.BlockSize,
            blankImage.Cylinders, blankImage.Heads, blankImage.SectorsPerTrack,
            injectedBlocks, blankImage.Capacity, blankImage.BlockCount);
        return (result, report);
    }

    private static MigrationTargetCapabilities ResolveCapabilities(MigrationPlan plan, IMediaSectorImage blankImage)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(blankImage);
        var capabilities = FileSystemMigrationCapabilityCatalog.For(plan.TargetFileSystemId, blankImage);
        return plan.SourceFileSystemId.Equals(plan.TargetFileSystemId, StringComparison.OrdinalIgnoreCase) &&
            plan.TargetFileSystemId is FileSystemIds.AppleDos or FileSystemIds.CommodoreDos
            ? capabilities with { SupportsRawAttributes = true } : capabilities;
    }

    private static void RequireBlankImage(IMediaSectorImage image)
    {
        if (image.AvailableBlocks.Count != image.BlockCount ||
            image.AvailableBlocks.Any(block => block.Data.Count != image.BlockSize || block.Data.Any(value => value != 0)))
            throw new ArgumentException("The target image must contain all sectors initialized to zero.", nameof(image));
    }
}
