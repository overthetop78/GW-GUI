using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Geometries.Amiga;
using GWGUI.MediaEngine.Migration;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Reconstruction;
using GWGUI.MediaEngine.SectorImages;
using System.Buffers.Binary;
using System.Text;

namespace GWGUI.MediaEngine.FileSystems.Amiga;

public sealed class AmigaDosVolumeWriter
{
    public SectorImage Create(MigrationPlan plan, AmigaDosVariant variant, string formatId = DiskImageFormatIds.AmigaDos)
    {
        ArgumentNullException.ThrowIfNull(plan);
        if (variant is not (AmigaDosVariant.Ofs or AmigaDosVariant.Ffs)) throw AmigaDosVolumeWriterExceptions.UnsupportedVariant(variant);
        if (!new AmigaDosNamePolicy().IsValid(plan.VolumeName)) throw AmigaDosVolumeWriterExceptions.InvalidEntry("/");
        var geometry = formatId.Equals(DiskImageFormatIds.AmigaDos, StringComparison.OrdinalIgnoreCase) ? AmigaAdfGeometry.DoubleDensity : formatId.Equals(DiskImageFormatIds.AmigaDosHighDensity, StringComparison.OrdinalIgnoreCase) ? AmigaAdfGeometry.HighDensity : throw AmigaDosVolumeWriterExceptions.UnsupportedGeometry(formatId);
        return new Builder(plan, variant, geometry).Build();
    }

    private sealed class Builder(MigrationPlan plan, AmigaDosVariant variant, RegularSectorGeometry geometry)
    {
        private readonly Dictionary<int, byte[]> _blocks = [];
        private readonly HashSet<int> _allocated = [.. Enumerable.Range(0, AmigaDosLayout.BootBlockCount)];
        private readonly HashSet<int> _normalChecksumBlocks = [];
        private readonly int _rootBlock = geometry.BlockCount / 2;
        private readonly int _bitmapBlock = geometry.BlockCount / 2 + 1;

        public SectorImage Build()
        {
            _allocated.Add(_rootBlock);
            _allocated.Add(_bitmapBlock);
            var boot = CreateBootBlocks();
            _blocks[AmigaDosLayout.BootBlock] = boot[..AmigaDosLayout.BlockSize];
            _blocks[AmigaDosLayout.BootBlock + 1] = boot[AmigaDosLayout.BlockSize..];
            var root = CreateMetadataBlock(AmigaDosLayout.RootSecondaryType, 0, 0, plan.VolumeName, null, 0);
            WriteInt32(root, AmigaDosLayout.HashTableSizeOffset, AmigaDosLayout.RootHashTableEntryCount);
            WriteInt32(root, AmigaDosLayout.BitmapValidityOffset, -1);
            WriteInt32(root, AmigaDosLayout.BitmapPointersOffset, _bitmapBlock);
            _blocks[_rootBlock] = root;
            _normalChecksumBlocks.Add(_rootBlock);
            AddEntries(root, _rootBlock, plan.Entries);
            _blocks[_bitmapBlock] = CreateBitmapBlock();
            foreach (var blockNumber in _normalChecksumBlocks) SetChecksum(_blocks[blockNumber], AmigaDosLayout.ChecksumOffset);
            SetChecksum(_blocks[_bitmapBlock], 0);
            var sectors = Enumerable.Range(0, geometry.BlockCount).Select(logicalBlock =>
            {
                var track = logicalBlock / geometry.SectorsPerTrack;
                var data = _blocks.TryGetValue(logicalBlock, out var block) ? block : new byte[AmigaDosLayout.BlockSize];
                return new SectorBlock(logicalBlock, new(track / geometry.Heads, track % geometry.Heads, logicalBlock % geometry.SectorsPerTrack), data);
            });
            return new(geometry.FormatId, AmigaDosLayout.BlockSize, geometry.Cylinders, geometry.Heads, geometry.SectorsPerTrack, sectors);
        }

        private byte[] CreateBootBlocks()
        {
            var boot = new byte[AmigaDosLayout.BlockSize * AmigaDosLayout.BootBlockCount];
            boot[0] = AmigaDosLayout.DosSignatureD;
            boot[1] = AmigaDosLayout.DosSignatureO;
            boot[2] = AmigaDosLayout.DosSignatureS;
            boot[AmigaDosLayout.DosVariantOffset] = (byte)variant;
            WriteInt32(boot, AmigaDosLayout.BootRootPointerOffset, _rootBlock);
            uint sum = 0;
            for (var offset = 0; offset < boot.Length; offset += AmigaDosLayout.WordSize)
            {
                if (offset == AmigaDosLayout.WordSize) continue;
                var value = BinaryPrimitives.ReadUInt32BigEndian(boot.AsSpan(offset));
                var previous = sum;
                sum = unchecked(sum + value);
                if (sum < previous) sum++;
            }
            WriteUInt32(boot, AmigaDosLayout.WordSize, ~sum);
            return boot;
        }

        private void AddEntries(byte[] parent, int parentBlock, IReadOnlyList<MigrationEntry> entries)
        {
            foreach (var entry in entries)
            {
                if (entry.Kind is not (FileSystemEntryKind.Directory or FileSystemEntryKind.File) || entry.TargetName.Length is 0 or > AmigaDosLayout.OrdinaryNameMaximumLength) throw AmigaDosVolumeWriterExceptions.InvalidEntry(entry.SourcePath);
                var entryBlock = Allocate();
                var block = CreateMetadataBlock(entry.Kind == FileSystemEntryKind.Directory ? AmigaDosLayout.DirectorySecondaryType : AmigaDosLayout.FileSecondaryType, entryBlock, parentBlock, entry.TargetName, entry.Comment, entry.RawAttributes, entry.Modified);
                _blocks[entryBlock] = block;
                _normalChecksumBlocks.Add(entryBlock);
                InsertIntoHashTable(parent, entry.TargetName, entryBlock);
                if (entry.Kind == FileSystemEntryKind.Directory) AddEntries(block, entryBlock, entry.Children);
                else WriteFile(block, entryBlock, entry);
            }
        }

        private void WriteFile(byte[] header, int headerBlock, MigrationEntry entry)
        {
            if (entry.Content is null) throw AmigaDosVolumeWriterExceptions.InvalidEntry(entry.SourcePath);
            var content = entry.Content.ToArray();
            WriteUInt32(header, AmigaDosLayout.FileSizeOffset, checked((uint)content.Length));
            var payloadSize = variant.IsFastFileSystem() ? AmigaDosLayout.BlockSize : AmigaDosLayout.OfsDataMaximumLength;
            var dataBlocks = new List<int>();
            for (var offset = 0; offset < content.Length; offset += payloadSize)
            {
                var dataBlock = Allocate();
                var length = Math.Min(payloadSize, content.Length - offset);
                var data = new byte[AmigaDosLayout.BlockSize];
                if (variant.IsFastFileSystem()) content.AsSpan(offset, length).CopyTo(data);
                else
                {
                    WriteInt32(data, AmigaDosLayout.PrimaryTypeOffset, AmigaDosLayout.OfsDataPrimaryType);
                    WriteInt32(data, AmigaDosLayout.HeaderKeyOffset, headerBlock);
                    WriteInt32(data, AmigaDosLayout.HighSequenceOffset, dataBlocks.Count + 1);
                    WriteInt32(data, AmigaDosLayout.HashTableSizeOffset, length);
                    content.AsSpan(offset, length).CopyTo(data.AsSpan(AmigaDosLayout.OfsDataHeaderLength));
                }
                _blocks[dataBlock] = data;
                if (!variant.IsFastFileSystem()) _normalChecksumBlocks.Add(dataBlock);
                dataBlocks.Add(dataBlock);
            }
            if (!variant.IsFastFileSystem())
            {
                for (var index = 0; index < dataBlocks.Count; index++)
                {
                    WriteInt32(_blocks[dataBlocks[index]], AmigaDosLayout.FirstReservedOffset, index + 1 < dataBlocks.Count ? dataBlocks[index + 1] : 0);
                }
            }
            WriteDataPointers(header, headerBlock, dataBlocks);
        }

        private void WriteDataPointers(byte[] header, int headerBlock, IReadOnlyList<int> dataBlocks)
        {
            var metadata = header;
            for (var start = 0; start < dataBlocks.Count; start += AmigaDosLayout.RootHashTableEntryCount)
            {
                var count = Math.Min(AmigaDosLayout.RootHashTableEntryCount, dataBlocks.Count - start);
                WriteInt32(metadata, AmigaDosLayout.HighSequenceOffset, count);
                WriteInt32(metadata, AmigaDosLayout.FirstReservedOffset, dataBlocks[start]);
                for (var index = 0; index < count; index++) WriteInt32(metadata, AmigaDosLayout.DataPointersOffset + (AmigaDosLayout.RootHashTableEntryCount - 1 - index) * AmigaDosLayout.WordSize, dataBlocks[start + index]);
                if (start + count >= dataBlocks.Count) break;
                var extensionBlock = Allocate();
                WriteInt32(metadata, AmigaDosLayout.ExtensionBlockOffset, extensionBlock);
                var extension = new byte[AmigaDosLayout.BlockSize];
                WriteInt32(extension, AmigaDosLayout.PrimaryTypeOffset, AmigaDosLayout.FileExtensionPrimaryType);
                WriteInt32(extension, AmigaDosLayout.HeaderKeyOffset, extensionBlock);
                WriteInt32(extension, AmigaDosLayout.ParentBlockOffset, headerBlock);
                WriteInt32(extension, AmigaDosLayout.SecondaryTypeOffset, AmigaDosLayout.FileSecondaryType);
                _blocks[extensionBlock] = extension;
                _normalChecksumBlocks.Add(extensionBlock);
                metadata = extension;
            }
        }

        private byte[] CreateMetadataBlock(int secondaryType, int blockNumber, int parentBlock, string name, string? comment, uint attributes, DateTimeOffset? modified = null)
        {
            var block = new byte[AmigaDosLayout.BlockSize];
            WriteInt32(block, AmigaDosLayout.PrimaryTypeOffset, AmigaDosLayout.HeaderPrimaryType);
            WriteInt32(block, AmigaDosLayout.HeaderKeyOffset, blockNumber);
            WriteUInt32(block, AmigaDosLayout.ProtectionOffset, attributes);
            WriteString(block, AmigaDosLayout.OrdinaryNameOffset, AmigaDosLayout.OrdinaryNameMaximumLength, name);
            WriteString(block, AmigaDosLayout.LongNameOffset, AmigaDosLayout.CommentMaximumLength, comment ?? string.Empty);
            WriteDate(block, AmigaDosLayout.DateOffset, modified ?? DateTimeOffset.UtcNow);
            if (secondaryType == AmigaDosLayout.RootSecondaryType) WriteDate(block, AmigaDosLayout.VolumeModifiedDateOffset, modified ?? DateTimeOffset.UtcNow);
            WriteInt32(block, AmigaDosLayout.ParentBlockOffset, parentBlock);
            WriteInt32(block, AmigaDosLayout.SecondaryTypeOffset, secondaryType);
            return block;
        }

        private byte[] CreateBitmapBlock()
        {
            var bitmap = new byte[AmigaDosLayout.BlockSize];
            for (var block = AmigaDosLayout.BootBlockCount; block < geometry.BlockCount; block++)
            {
                if (_allocated.Contains(block)) continue;
                var bit = block - AmigaDosLayout.BootBlockCount;
                var offset = AmigaDosLayout.BitmapDataOffset + bit / 32 * AmigaDosLayout.WordSize;
                var mask = 1u << (bit % 32);
                WriteUInt32(bitmap, offset, BigEndianInt32.ReadUnsigned(bitmap, offset) | mask);
            }
            return bitmap;
        }

        private void InsertIntoHashTable(byte[] parent, string name, int entryBlock)
        {
            var hash = Hash(name);
            var offset = AmigaDosLayout.DataPointersOffset + hash * AmigaDosLayout.WordSize;
            var current = BigEndianInt32.Read(parent, offset);
            if (current == 0) WriteInt32(parent, offset, entryBlock);
            else
            {
                while (BigEndianInt32.Read(_blocks[current], AmigaDosLayout.HashChainOffset) != 0) current = BigEndianInt32.Read(_blocks[current], AmigaDosLayout.HashChainOffset);
                WriteInt32(_blocks[current], AmigaDosLayout.HashChainOffset, entryBlock);
            }
        }

        private int Allocate()
        {
            for (var block = AmigaDosLayout.BootBlockCount; block < geometry.BlockCount; block++) if (_allocated.Add(block)) return block;
            throw AmigaDosVolumeWriterExceptions.DiskFull();
        }

        private static int Hash(string name)
        {
            uint hash = (uint)name.Length;
            foreach (var value in System.Text.Encoding.Latin1.GetBytes(name)) hash = (hash * 13 + (byte)char.ToUpperInvariant((char)value)) & 0x7ff;
            return (int)(hash % AmigaDosLayout.RootHashTableEntryCount);
        }

        private static void WriteString(Span<byte> block, int offset, int maximum, string value)
        {
            var encoded = System.Text.Encoding.Latin1.GetBytes(value);
            var length = Math.Min(maximum, encoded.Length);
            block[offset] = (byte)length;
            encoded.AsSpan(0, length).CopyTo(block[(offset + 1)..]);
        }

        private static void WriteDate(Span<byte> block, int offset, DateTimeOffset value)
        {
            var elapsed = value.ToUniversalTime() - AmigaDosLayout.Epoch;
            if (elapsed < TimeSpan.Zero) elapsed = TimeSpan.Zero;
            var days = (int)elapsed.TotalDays;
            var withinDay = elapsed - TimeSpan.FromDays(days);
            var minutes = (int)withinDay.TotalMinutes;
            var ticks = (int)((withinDay - TimeSpan.FromMinutes(minutes)).TotalSeconds * AmigaDosLayout.TicksPerSecond);
            WriteInt32(block, offset + AmigaDosTime.DaysOffset, days);
            WriteInt32(block, offset + AmigaDosTime.MinutesOffset, minutes);
            WriteInt32(block, offset + AmigaDosTime.TicksOffset, ticks);
        }

        private static void SetChecksum(Span<byte> block, int offset)
        {
            WriteUInt32(block, offset, 0);
            uint sum = 0;
            for (var index = 0; index < block.Length; index += AmigaDosLayout.WordSize) sum = unchecked(sum + BinaryPrimitives.ReadUInt32BigEndian(block[index..]));
            WriteUInt32(block, offset, unchecked(0u - sum));
        }

        private static void WriteInt32(Span<byte> block, int offset, int value) => BinaryPrimitives.WriteInt32BigEndian(block[offset..], value);

        private static void WriteUInt32(Span<byte> block, int offset, uint value) => BinaryPrimitives.WriteUInt32BigEndian(block[offset..], value);
    }
}
