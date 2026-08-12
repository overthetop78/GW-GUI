using GWGUI.MediaEngine.Definitions;
using System.Buffers.Binary;
using System.Text;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.FileSystems.Macintosh;


namespace GWGUI.MediaEngine.FileSystems.Readers;

public sealed class MacHfsFileSystemReader : IFileSystemReader
{
    private static readonly DateTimeOffset MacEpoch = new(1904, 1, 1, 0, 0, 0, TimeSpan.Zero);
    public string Id => "mac-hfs";
    public IReadOnlySet<string> CatalogFormatIds { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { DiskImageFormatIds.AppleMacHfs, DiskImageFormatIds.Mac400, DiskImageFormatIds.Mac800, DiskImageFormatIds.Mac1440 };

    public bool CanRead(SectorImage image) => image.BlockSize == MacintoshVolumeSignatures.BlockSize && image.TryGetBlock(MacintoshVolumeSignatures.MasterDirectoryBlock, out var mdb) && mdb.Data.Count >= 162 && BinaryPrimitives.ReadUInt16BigEndian(mdb.Data.ToArray()) == MacintoshVolumeSignatures.Hfs;

    public FileSystemVolume Read(SectorImage image)
    {
        if (!CanRead(image)) throw new InvalidDataException("The image does not contain a Macintosh HFS volume.");
        var mdb = image.GetBlock(2).Span; var allocationCount = U16(mdb, 18); var allocationSize = U32(mdb, 20);
        var allocationStart = U16(mdb, 28); var free = U16(mdb, 34); var name = Pascal(mdb, 36, 27);
        var catalogSize = U32(mdb, 146); var warnings = new List<string>();
        var catalog = ReadExtents(image, mdb.Slice(150, 12), allocationStart, allocationSize, catalogSize, warnings, "catalog");
        var records = ParseCatalog(catalog, image, allocationStart, allocationSize, warnings);
        var entries = BuildChildren(2, records, new HashSet<uint>(), warnings);
        return new(name, "Macintosh HFS", (long)allocationCount * allocationSize, (long)free * allocationSize,
            MacDate(U32(mdb, 2)), MacDate(U32(mdb, 6)), entries, warnings);
    }

    private static List<CatalogRecord> ParseCatalog(byte[] catalog, SectorImage image, int allocationStart, uint allocationSize, List<string> warnings)
    {
        if (catalog.Length < 64) throw new InvalidDataException("The HFS catalog file is truncated.");
        var nodeSize = U16(catalog, 32); if (nodeSize is < 256 or > 32768 || catalog.Length < nodeSize) nodeSize = 512;
        var result = new List<CatalogRecord>();
        for (var nodeOffset = 0; nodeOffset + nodeSize <= catalog.Length; nodeOffset += nodeSize)
        {
            var node = catalog.AsSpan(nodeOffset, nodeSize); if ((sbyte)node[8] != -1) continue;
            var count = U16(node, 10); if (count > 512) continue;
            for (var index = 0; index < count; index++)
            {
                var start = U16(node, nodeSize - 2 * (index + 1)); var end = U16(node, nodeSize - 2 * (index + 2));
                if (start < 14 || end <= start || end > nodeSize) continue;
                var keyLength = node[start]; if (keyLength < 6 || start + 1 + keyLength > end) continue;
                var key = start + 1; var parent = U32(node, key + 1); var nameLength = node[key + 5];
                if (nameLength > 31 || key + 6 + nameLength > end) continue;
                var name = DecodeMac(node.Slice(key + 6, nameLength)); var data = start + 1 + keyLength; if ((data & 1) != 0) data++;
                if (data >= end) continue; var type = node[data];
                if (type == 1 && data + 70 <= end)
                {
                    result.Add(new(parent, U32(node, data + 6), name, true, 0, MacDate(U32(node, data + 14)), "Directory", []));
                }
                else if (type == 2 && data + 102 <= end)
                {
                    var dataLength = U32(node, data + 26);
                    var resourceLength = U32(node, data + 36);
                    var fileType = System.Text.Encoding.ASCII.GetString(node.Slice(data + 4, 4)).Trim('\0', ' ');
                    var dataFork = ReadExtents(image, node.Slice(data + 74, 12), allocationStart, allocationSize, dataLength, warnings, name);
                    var resourceFork = ReadExtents(image, node.Slice(data + 86, 12), allocationStart, allocationSize, resourceLength, warnings, $"{name} (resource fork)");
                    var content = dataFork.Length > 0 ? dataFork : resourceFork;
                    result.Add(new(parent, U32(node, data + 20), name, false, (long)dataLength + resourceLength,
                        MacDate(U32(node, data + 48)), string.IsNullOrWhiteSpace(fileType) ? "Macintosh file" : fileType, content));
                }
            }
        }
        if (result.Count == 0) warnings.Add("The HFS catalog contains no readable leaf records.");
        return result;
    }

    private static IReadOnlyList<FileSystemEntry> BuildChildren(uint parent, List<CatalogRecord> records, HashSet<uint> path, List<string> warnings)
    {
        if (!path.Add(parent)) { warnings.Add($"Cyclic HFS directory #{parent}."); return []; }
        var entries = records.Where(record => record.Parent == parent).Select(record => new FileSystemEntry(record.Name,
            record.IsDirectory ? FileSystemEntryKind.Directory : FileSystemEntryKind.File, record.Size, record.Modified, record.Type, 0,
            checked((int)Math.Min(record.Id, int.MaxValue)), true,
            record.IsDirectory ? BuildChildren(record.Id, records, new HashSet<uint>(path), warnings) : [], record.IsDirectory ? null : record.Content)).ToArray();
        return entries.OrderBy(entry => entry.Kind != FileSystemEntryKind.Directory).ThenBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static byte[] ReadExtents(SectorImage image, ReadOnlySpan<byte> extents, int allocationStart, uint allocationSize, uint logicalLength, List<string> warnings, string name)
    {
        using var output = new MemoryStream(); var blocksPerAllocation = checked((int)(allocationSize / 512));
        for (var extent = 0; extent < 3 && output.Length < logicalLength; extent++)
        {
            var start = U16(extents, extent * 4); var count = U16(extents, extent * 4 + 2); if (count == 0) continue;
            for (var allocation = 0; allocation < count && output.Length < logicalLength; allocation++)
                for (var block = 0; block < blocksPerAllocation && output.Length < logicalLength; block++)
                {
                    var logical = allocationStart + (start + allocation) * blocksPerAllocation + block;
                    if (!image.TryGetBlock(logical, out var sector)) { warnings.Add($"{name}: HFS block {logical} is missing."); output.Write(new byte[512]); }
                    else output.Write(sector.Data.ToArray());
                }
        }
        if (output.Length < logicalLength) warnings.Add($"{name}: additional HFS extents are required or data is missing.");
        return output.ToArray().Take(checked((int)Math.Min(logicalLength, int.MaxValue))).ToArray();
    }

    private static ushort U16(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadUInt16BigEndian(data.Slice(offset, 2));
    private static uint U32(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadUInt32BigEndian(data.Slice(offset, 4));
    private static string Pascal(ReadOnlySpan<byte> data, int offset, int max) { var length = Math.Min(data[offset], max); return DecodeMac(data.Slice(offset + 1, length)); }
    private static string DecodeMac(ReadOnlySpan<byte> value) => System.Text.Encoding.Latin1.GetString(value).Replace(':', '/');
    private static DateTimeOffset? MacDate(uint seconds) { try { return seconds == 0 ? null : MacEpoch.AddSeconds(seconds); } catch { return null; } }
    private sealed record CatalogRecord(uint Parent, uint Id, string Name, bool IsDirectory, long Size, DateTimeOffset? Modified, string Type, IReadOnlyList<byte> Content);
}
