using GWGUI.MediaEngine.FileSystems.Fat12;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Conversion.Fat12;

public static class Fat12ReinterpretationPolicy
{
    public static Fat12TargetGeometry Validate(SectorImage source, string targetFormatId, bool sourceIsHybrid = false)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetFormatId);
        if (sourceIsHybrid) throw Fat12ReinterpretationExceptions.HybridSource();
        if (!Fat12TargetGeometryCatalog.TryResolve(targetFormatId, out var target)) throw Fat12ReinterpretationExceptions.UnsupportedTarget(targetFormatId);
        if (source.MissingBlocks.Count != 0 || source.AvailableBlocks.Any(block => block.Data.Count != source.BlockSize)) throw Fat12ReinterpretationExceptions.MissingSectors(source.FormatId);
        if (source.BlockSize != target.SectorSize || source.Cylinders != target.Cylinders || source.Heads != target.Heads || source.SectorsPerTrack != target.SectorsPerTrack || source.BlockCount != target.TotalSectors || source.Capacity != target.Capacity)
            throw Fat12ReinterpretationExceptions.IncompatibleGeometry(source.FormatId, targetFormatId);
        if (!source.TryGetBlock(FatBootSectorLayout.BootLogicalBlock, out var boot)) throw Fat12ReinterpretationExceptions.InvalidBpb(source.FormatId);
        var bootBytes = boot.Data.ToArray();
        if (!FatBpbGeometryDetector.TryDetect(bootBytes, target.Capacity, out var bpb) || bpb.SectorSize != target.SectorSize || bpb.TotalSectors != target.TotalSectors || bpb.Cylinders != target.Cylinders || bpb.Heads != target.Heads || bpb.SectorsPerTrack != target.SectorsPerTrack || !Fat12LayoutReader.TryRead(bootBytes, source.BlockCount, source.FormatId, out _))
            throw Fat12ReinterpretationExceptions.InvalidBpb(source.FormatId);
        return target;
    }
}
