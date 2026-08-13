using GWGUI.MediaEngine.Conversion.Fat12;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Migration;
using GWGUI.MediaEngine.SectorImages;
using System.Buffers.Binary;

namespace GWGUI.MediaEngine.FileSystems.Fat12;

public sealed class Fat12VolumeWriter
{
    public SectorImage Create(MigrationPlan plan, string formatId)
    {
        ArgumentNullException.ThrowIfNull(plan);
        if (!Fat12TargetGeometryCatalog.TryResolve(formatId, out var geometry)) throw Fat12VolumeWriterExceptions.UnsupportedGeometry(formatId);
        if (!new Fat12VolumeNamePolicy().IsValid(plan.VolumeName)) throw Fat12VolumeWriterExceptions.InvalidEntry("/");
        return new Builder(plan, geometry).Build();
    }

    private sealed class Builder(MigrationPlan plan, Fat12TargetGeometry geometry)
    {
        private readonly Fat12ShortNamePolicy _namePolicy = new();
        private readonly byte[][] _sectors = Enumerable.Range(0, geometry.TotalSectors).Select(_ => new byte[FatBootSectorLayout.SectorSize]).ToArray();
        private readonly Fat12WritableLayout _layout = CreateLayout(geometry.TotalSectors);
        private int _nextCluster = Fat12Table.FirstDataCluster;

        public SectorImage Build()
        {
            var nodes = plan.Entries.Select(entry => CreateNode(entry, 0)).ToArray();
            WriteBootSector();
            var fat = new byte[_layout.SectorsPerFat * FatBootSectorLayout.SectorSize];
            fat[0] = _layout.MediaDescriptor;
            fat[1] = Fat12Table.ReservedEntryByte;
            fat[2] = Fat12Table.ReservedEntryByte;
            foreach (var node in nodes) WriteNode(node, fat);
            WriteRootDirectory(nodes);
            for (var copy = 0; copy < _layout.FatCount; copy++) for (var sector = 0; sector < _layout.SectorsPerFat; sector++) fat.AsSpan(sector * FatBootSectorLayout.SectorSize, FatBootSectorLayout.SectorSize).CopyTo(_sectors[FatBootSectorLayout.FirstFatLogicalBlock + copy * _layout.SectorsPerFat + sector]);
            var blocks = Enumerable.Range(0, geometry.TotalSectors).Select(logicalBlock =>
            {
                var track = logicalBlock / geometry.SectorsPerTrack;
                return new SectorBlock(logicalBlock, new(track / geometry.Heads, track % geometry.Heads, logicalBlock % geometry.SectorsPerTrack + FatBootSectorLayout.FirstSectorNumber), _sectors[logicalBlock]);
            });
            return new(geometry.FormatId, geometry.SectorSize, geometry.Cylinders, geometry.Heads, geometry.SectorsPerTrack, blocks);
        }

        private Node CreateNode(MigrationEntry entry, int parentCluster)
        {
            if (!_namePolicy.IsValid(entry.TargetName) || entry.Kind is not (FileSystemEntryKind.Directory or FileSystemEntryKind.File) || entry.Kind == FileSystemEntryKind.File && entry.Content is null) throw Fat12VolumeWriterExceptions.InvalidEntry(entry.SourcePath);
            var clusterCount = entry.Kind == FileSystemEntryKind.Directory ? Math.Max(1, DivideRoundUp((entry.Children.Count + 2) * FatDirectoryLayout.EntrySize, _layout.ClusterByteCount)) : DivideRoundUp(entry.Content!.Count, _layout.ClusterByteCount);
            var clusters = AllocateClusters(clusterCount);
            var node = new Node(entry, parentCluster, clusters, []);
            node.Children.AddRange(entry.Children.Select(child => CreateNode(child, clusters.Count == 0 ? 0 : clusters[0])));
            return node;
        }

        private void WriteNode(Node node, Span<byte> fat)
        {
            WriteClusterChain(fat, node.Clusters);
            if (node.Entry.Kind == FileSystemEntryKind.File)
            {
                var content = node.Entry.Content!.ToArray();
                for (var index = 0; index < node.Clusters.Count; index++) WriteClusterData(node.Clusters[index], content.AsSpan(index * _layout.ClusterByteCount, Math.Min(_layout.ClusterByteCount, content.Length - index * _layout.ClusterByteCount)));
                return;
            }
            var directory = new byte[node.Clusters.Count * _layout.ClusterByteCount];
            WriteSpecialDirectoryEntry(directory.AsSpan(0, FatDirectoryLayout.EntrySize), FatDirectoryLayout.CurrentDirectoryName, node.Clusters[0]);
            WriteSpecialDirectoryEntry(directory.AsSpan(FatDirectoryLayout.EntrySize, FatDirectoryLayout.EntrySize), FatDirectoryLayout.ParentDirectoryName, node.ParentCluster);
            for (var index = 0; index < node.Children.Count; index++) WriteDirectoryEntry(directory.AsSpan((index + 2) * FatDirectoryLayout.EntrySize, FatDirectoryLayout.EntrySize), node.Children[index]);
            for (var index = 0; index < node.Clusters.Count; index++) WriteClusterData(node.Clusters[index], directory.AsSpan(index * _layout.ClusterByteCount, _layout.ClusterByteCount));
            foreach (var child in node.Children) WriteNode(child, fat);
        }

        private void WriteRootDirectory(IReadOnlyList<Node> nodes)
        {
            if (nodes.Count + 1 > _layout.RootEntries) throw Fat12VolumeWriterExceptions.DiskFull();
            var root = new byte[_layout.RootSectors * FatBootSectorLayout.SectorSize];
            WriteVolumeLabel(root.AsSpan(0, FatDirectoryLayout.EntrySize));
            for (var index = 0; index < nodes.Count; index++) WriteDirectoryEntry(root.AsSpan((index + 1) * FatDirectoryLayout.EntrySize, FatDirectoryLayout.EntrySize), nodes[index]);
            for (var sector = 0; sector < _layout.RootSectors; sector++) root.AsSpan(sector * FatBootSectorLayout.SectorSize, FatBootSectorLayout.SectorSize).CopyTo(_sectors[_layout.RootStart + sector]);
        }

        private void WriteBootSector()
        {
            var boot = _sectors[FatBootSectorLayout.BootLogicalBlock];
            boot[0] = FatBootSectorLayout.ShortJumpOpcode;
            boot[1] = 0x3c;
            boot[2] = 0x90;
            System.Text.Encoding.ASCII.GetBytes("GWGUI   ").CopyTo(boot, FatBootSectorLayout.OemOffset);
            BinaryPrimitives.WriteUInt16LittleEndian(boot.AsSpan(FatBootSectorLayout.BytesPerSectorOffset), FatBootSectorLayout.SectorSize);
            boot[FatBootSectorLayout.SectorsPerClusterOffset] = checked((byte)_layout.SectorsPerCluster);
            BinaryPrimitives.WriteUInt16LittleEndian(boot.AsSpan(FatBootSectorLayout.ReservedSectorCountOffset), 1);
            boot[FatBootSectorLayout.FatCountOffset] = checked((byte)_layout.FatCount);
            BinaryPrimitives.WriteUInt16LittleEndian(boot.AsSpan(FatBootSectorLayout.RootEntryCountOffset), checked((ushort)_layout.RootEntries));
            BinaryPrimitives.WriteUInt16LittleEndian(boot.AsSpan(FatBootSectorLayout.TotalSectors16Offset), checked((ushort)_layout.TotalSectors));
            boot[FatBootSectorLayout.MediaDescriptorOffset] = _layout.MediaDescriptor;
            BinaryPrimitives.WriteUInt16LittleEndian(boot.AsSpan(FatBootSectorLayout.SectorsPerFatOffset), checked((ushort)_layout.SectorsPerFat));
            BinaryPrimitives.WriteUInt16LittleEndian(boot.AsSpan(FatBootSectorLayout.SectorsPerTrackOffset), checked((ushort)geometry.SectorsPerTrack));
            BinaryPrimitives.WriteUInt16LittleEndian(boot.AsSpan(FatBootSectorLayout.HeadCountOffset), checked((ushort)geometry.Heads));
            WritePaddedAscii(boot.AsSpan(FatBootSectorLayout.VolumeLabelOffset, FatBootSectorLayout.VolumeLabelLength), plan.VolumeName.ToUpperInvariant());
            boot[^2] = 0x55;
            boot[^1] = 0xaa;
        }

        private void WriteVolumeLabel(Span<byte> entry)
        {
            WritePaddedAscii(entry[..11], plan.VolumeName.ToUpperInvariant());
            entry[FatDirectoryLayout.AttributesOffset] = (byte)FatDirectoryAttributes.VolumeLabel;
        }

        private static void WriteDirectoryEntry(Span<byte> target, Node node)
        {
            WriteShortName(target, node.Entry.TargetName);
            target[FatDirectoryLayout.AttributesOffset] = (byte)(node.Entry.Kind == FileSystemEntryKind.Directory ? FatDirectoryAttributes.Directory : FatDirectoryAttributes.Archive);
            WriteTimestamp(target, node.Entry.Modified);
            BinaryPrimitives.WriteUInt16LittleEndian(target[FatDirectoryLayout.FirstClusterOffset..], checked((ushort)(node.Clusters.Count == 0 ? 0 : node.Clusters[0])));
            if (node.Entry.Kind == FileSystemEntryKind.File) BinaryPrimitives.WriteUInt32LittleEndian(target[FatDirectoryLayout.FileSizeOffset..], checked((uint)node.Entry.Content!.Count));
        }

        private static void WriteSpecialDirectoryEntry(Span<byte> target, string name, int cluster)
        {
            target.Fill((byte)' ');
            System.Text.Encoding.ASCII.GetBytes(name).CopyTo(target);
            target[FatDirectoryLayout.AttributesOffset] = (byte)FatDirectoryAttributes.Directory;
            BinaryPrimitives.WriteUInt16LittleEndian(target[FatDirectoryLayout.FirstClusterOffset..], checked((ushort)cluster));
        }

        private static void WriteShortName(Span<byte> target, string name)
        {
            target[..11].Fill((byte)' ');
            var parts = name.Split(FatDirectoryLayout.ExtensionSeparator);
            System.Text.Encoding.ASCII.GetBytes(parts[0]).CopyTo(target);
            if (parts.Length == 2) System.Text.Encoding.ASCII.GetBytes(parts[1]).CopyTo(target[FatDirectoryLayout.ExtensionOffset..]);
        }

        private static void WriteTimestamp(Span<byte> target, DateTimeOffset? value)
        {
            if (value is null) return;
            var local = value.Value.DateTime;
            var year = Math.Clamp(local.Year, 1980, 2107);
            var date = (ushort)((year - 1980) << 9 | local.Month << 5 | local.Day);
            var time = (ushort)(local.Hour << 11 | local.Minute << 5 | local.Second / 2);
            BinaryPrimitives.WriteUInt16LittleEndian(target[FatDirectoryLayout.ModifiedTimeOffset..], time);
            BinaryPrimitives.WriteUInt16LittleEndian(target[FatDirectoryLayout.ModifiedDateOffset..], date);
        }

        private List<int> AllocateClusters(int count)
        {
            if (count == 0) return [];
            if (_nextCluster + count > Fat12Table.FirstDataCluster + _layout.ClusterCount) throw Fat12VolumeWriterExceptions.DiskFull();
            var clusters = Enumerable.Range(_nextCluster, count).ToList();
            _nextCluster += count;
            return clusters;
        }

        private void WriteClusterChain(Span<byte> fat, IReadOnlyList<int> clusters)
        {
            for (var index = 0; index < clusters.Count; index++) WriteFatEntry(fat, clusters[index], index + 1 < clusters.Count ? clusters[index + 1] : Fat12Table.LastEndOfChain);
        }

        private void WriteClusterData(int cluster, ReadOnlySpan<byte> data)
        {
            var firstSector = _layout.DataStart + (cluster - Fat12Table.FirstDataCluster) * _layout.SectorsPerCluster;
            for (var sector = 0; sector < _layout.SectorsPerCluster; sector++)
            {
                var offset = sector * FatBootSectorLayout.SectorSize;
                if (offset >= data.Length) break;
                data.Slice(offset, Math.Min(FatBootSectorLayout.SectorSize, data.Length - offset)).CopyTo(_sectors[firstSector + sector]);
            }
        }

        private static void WriteFatEntry(Span<byte> fat, int cluster, int value)
        {
            var offset = cluster + cluster / 2;
            if ((cluster & 1) == 0)
            {
                fat[offset] = (byte)value;
                fat[offset + 1] = (byte)((fat[offset + 1] & 0xf0) | value >> 8 & 0x0f);
            }
            else
            {
                fat[offset] = (byte)((fat[offset] & 0x0f) | value << 4);
                fat[offset + 1] = (byte)(value >> 4);
            }
        }

        private static Fat12WritableLayout CreateLayout(int totalSectors)
        {
            var sectorsPerCluster = totalSectors <= 360 ? 1 : totalSectors <= 1440 ? 2 : totalSectors <= 2880 ? 1 : 2;
            var rootEntries = totalSectors <= 360 ? 64 : totalSectors <= 1440 ? 112 : 224;
            const int fatCount = 2;
            var rootSectors = FatBootSectorLayout.RootDirectorySectorCount(rootEntries);
            var sectorsPerFat = 1;
            while (true)
            {
                var dataStart = 1 + fatCount * sectorsPerFat + rootSectors;
                var clusters = (totalSectors - dataStart) / sectorsPerCluster;
                var required = DivideRoundUp((clusters + Fat12Table.FirstDataCluster) * 3, 2 * FatBootSectorLayout.SectorSize);
                if (required == sectorsPerFat) return new(totalSectors, sectorsPerCluster, fatCount, sectorsPerFat, rootEntries, 1 + fatCount * sectorsPerFat, rootSectors, dataStart, clusters, totalSectors >= 2880 ? (byte)0xf0 : (byte)0xf9);
                sectorsPerFat = required;
            }
        }

        private static void WritePaddedAscii(Span<byte> target, string value)
        {
            target.Fill((byte)' ');
            var encoded = System.Text.Encoding.ASCII.GetBytes(value);
            encoded.AsSpan(0, Math.Min(target.Length, encoded.Length)).CopyTo(target);
        }

        private static int DivideRoundUp(int value, int divisor) => checked((value + divisor - 1) / divisor);

        private sealed record Node(MigrationEntry Entry, int ParentCluster, List<int> Clusters, List<Node> Children);
    }
}
