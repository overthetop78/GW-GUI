using System.Globalization;
using System.Text;
using DiscUtils.Streams;
using DiscUtils.Vmdk;

namespace GWGUI.Emulation.HardDisks.Containers;

/// <summary>Common serialization for hosted VMDK sets with external descriptors.</summary>
internal static class VmdkDescriptorImageSetWriter
{
    internal static void Write(long capacity, string baseName, bool sparse, Action<string, Stream> emit,
        Action<Stream>? initialize, DiskAdapterType adapter, long extentBytes, bool split)
    {
        ArgumentNullException.ThrowIfNull(emit);
        if (capacity < 512 || capacity % 512 != 0 || capacity > 1L << 40)
            throw new ArgumentOutOfRangeException(nameof(capacity));
        if (split && (extentBytes < 65536 || extentBytes > 2L << 30 || extentBytes % 65536 != 0 ||
            (capacity + extentBytes - 1) / extentBytes > 1024))
            throw new ArgumentOutOfRangeException(nameof(extentBytes));
        if (!split) extentBytes = capacity;
        if (string.IsNullOrWhiteSpace(baseName) || baseName.Length > 100 ||
            baseName.Any(c => !(char.IsAsciiLetterOrDigit(c) || c is '-' or '_')))
            throw new ArgumentException("Invalid VMDK image set name.", nameof(baseName));
        var adapterName = adapter switch
        {
            DiskAdapterType.Ide => "ide", DiskAdapterType.BusLogicScsi => "buslogic",
            DiskAdapterType.LsiLogicScsi => "lsilogic", DiskAdapterType.LegacyEsx => "legacyESX",
            _ => throw new ArgumentOutOfRangeException(nameof(adapter))
        };
        using var content = SparseImageContent.Create(capacity, initialize);
        var descriptor = new StringBuilder("# Disk DescriptorFile\nversion=1\nencoding=\"UTF-8\"\n");
        descriptor.Append("CID=").Append(Guid.NewGuid().ToString("N")[..8]).Append("\nparentCID=ffffffff\n");
        descriptor.Append("createType=\"").Append(!split ? "monolithicFlat" : sparse ? "twoGbMaxExtentSparse" : "twoGbMaxExtentFlat").Append("\"\n\n");
        var index = 0;
        for (long offset = 0; offset < capacity; offset += extentBytes)
        {
            var length = Math.Min(extentBytes, capacity - offset);
            var name = !split ? baseName + "-flat.vmdk" :
                baseName + (sparse ? "-s" : "-f") + (++index).ToString("D3", CultureInfo.InvariantCulture) + ".vmdk";
            descriptor.Append(FormattableString.Invariant($"RW {length / 512} {(sparse ? "SPARSE" : "FLAT")} \"{name}\""));
            descriptor.Append(sparse ? "\n" : " 0\n");
            using var slice = new SubStream(content, Ownership.None, offset, length);
            if (!sparse) { emit(name, slice); continue; }
            var builder = new DiscUtils.Vmdk.DiskBuilder
            { Content = slice, DiskType = DiskCreateType.MonolithicSparse, AdapterType = adapter };
            using var built = builder.Build(Path.GetFileNameWithoutExtension(name)).Single().OpenStream();
            using var extent = new SparseMemoryStream();
            built.CopyTo(extent);
            // Hosted sparse extents are described by the external set descriptor, not an embedded descriptor.
            extent.Position = 28; extent.Write(new byte[16]);
            extent.Position = 0; emit(name, extent);
        }
        var geometry = DiscUtils.Geometry.FromCapacity(capacity);
        descriptor.Append("\nddb.adapterType = \"").Append(adapterName).Append("\"\n");
        descriptor.Append(FormattableString.Invariant($"ddb.geometry.cylinders = \"{geometry.Cylinders}\"\nddb.geometry.heads = \"{geometry.HeadsPerCylinder}\"\nddb.geometry.sectors = \"{geometry.SectorsPerTrack}\"\n"));
        descriptor.Append("ddb.virtualHWVersion = \"4\"\n");
        using var text = new MemoryStream(Encoding.UTF8.GetBytes(descriptor.ToString()), writable: false);
        emit(baseName + ".vmdk", text);
    }
}
