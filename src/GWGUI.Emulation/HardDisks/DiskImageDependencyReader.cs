using System.Buffers.Binary;
using System.Text;
using System.Text.RegularExpressions;
using DiscUtils.Streams;
using GWGUI.Emulation.HardDisks.Containers;

namespace GWGUI.Emulation.HardDisks;

/// <summary>Reads file dependencies from supported container metadata without opening their contents.</summary>
public static partial class DiskImageDependencyReader
{
    private const int MaximumMetadataBytes = 1 << 20;
    private const int VdiHeaderWithParentIdentifiersSize = 0x1C8;
    private const uint VdiDynamicImageType = 1;
    private const uint VdiFixedImageType = 2;
    private const uint VdiDifferencingImageType = 4;
    private const uint ChdVersion5 = 5;

    public static IReadOnlyList<string> Read(Stream image, string imagePath)
    {
        var references = ReadReferences(image, imagePath);
        if (references.Any(reference => reference.Path is null))
            throw new NotSupportedException("Identifier-based image dependencies require a dependency inventory.");
        return references.Select(reference => reference.Path!).Distinct(PathComparer).ToArray();
    }

    public static IReadOnlyList<DiskImageDependencyReference> ReadReferences(Stream image, string imagePath)
    {
        if (!image.CanRead || !image.CanSeek) throw new ArgumentException("A readable seekable image is required.");
        using var input = new DifferencingImageStreams.ReadOnlyParent(image);
        var family = DiskContainerSignatures.Identify(input);
        IEnumerable<DiskImageDependencyReference> dependencies;
        switch (family)
        {
            // The pinned library's public stream constructors have no locator. Its explicit-base overload
            // is required here; the implementation expects a directory despite the XML parameter wording.
#pragma warning disable CS0618
            case "vhd":
                using (var disk = new DiscUtils.Vhd.DiskImageFile(input, Ownership.None))
                    dependencies = Paths(disk.NeedsParent ? disk.GetParentLocations(Path.GetDirectoryName(Path.GetFullPath(imagePath))!) : []);
                break;
            case "vhdx":
                using (var disk = new DiscUtils.Vhdx.DiskImageFile(input, Ownership.None))
                    dependencies = Paths(disk.NeedsParent ? disk.GetParentLocations(Path.GetDirectoryName(Path.GetFullPath(imagePath))!) : []);
                break;
#pragma warning restore CS0618
            case "vmdk": dependencies = Paths(ReadVmdk(input, imagePath).Dependencies); break;
            case "qcow":
                var qcow = ReadBytes(input, 0, 20);
                dependencies = Paths(ReadBackingName(input, BinaryPrimitives.ReadUInt64BigEndian(qcow.AsSpan(8)),
                    BinaryPrimitives.ReadUInt32BigEndian(qcow.AsSpan(16))));
                break;
            case "qed":
                var qed = ReadBytes(input, 0, 64);
                dependencies = (BinaryPrimitives.ReadUInt64LittleEndian(qed.AsSpan(16)) & 1) == 0 ? [] : Paths(
                    ReadBackingName(input, BinaryPrimitives.ReadUInt32LittleEndian(qed.AsSpan(56)),
                        BinaryPrimitives.ReadUInt32LittleEndian(qed.AsSpan(60))));
                break;
            case "vdi":
                var vdi = ReadBytes(input, 0, VdiHeaderWithParentIdentifiersSize);
                var imageType = BinaryPrimitives.ReadUInt32LittleEndian(vdi.AsSpan(0x4C, sizeof(uint)));
                dependencies = imageType switch
                {
                    VdiDynamicImageType or VdiFixedImageType => [],
                    VdiDifferencingImageType => ReadVdiParent(vdi),
                    _ => throw new NotSupportedException($"VDI image type {imageType} is not supported for dependency resolution.")
                };
                break;
            case "chd":
                var chd = ReadBytes(input, 0, 124);
                if (BinaryPrimitives.ReadUInt32BigEndian(chd.AsSpan(12)) != ChdVersion5)
                    throw new NotSupportedException("Only CHD V5 dependencies are supported.");
                dependencies = chd.AsSpan(104, 20).ContainsAnyExcept((byte)0)
                    ? [DiskImageDependencyReference.FromSha1(chd.AsSpan(104, 20))]
                    : [];
                break;
            case "udif":
                var footer = ReadBytes(input, (ulong)(input.Length - 512), 512);
                if (BinaryPrimitives.ReadUInt32BigEndian(footer.AsSpan(60)) != 1)
                    throw new NotSupportedException("Segmented UDIF dependencies are not resolved by this reader.");
                dependencies = [];
                break;
            case "bochs":
                var bochs = ReadBytes(input, 0, 64);
                if (!bochs.AsSpan(48).StartsWith("Growing\0"u8))
                    throw new NotSupportedException("This redolog subtype requires external dependency information.");
                dependencies = [];
                break;
            default: dependencies = []; break;
        }
        return DistinctReferences(dependencies.Select(reference => reference.Path is null
                ? reference
                : DiskImageDependencyReference.FromPath(Resolve(imagePath, reference.Path))));
    }

    public static IReadOnlyList<DiskImageDependencyReference> ReadIdentities(Stream image)
    {
        if (!image.CanRead || !image.CanSeek) throw new ArgumentException("A readable seekable image is required.");
        using var input = new DifferencingImageStreams.ReadOnlyParent(image);
        return DiskContainerSignatures.Identify(input) switch
        {
            "vdi" => ReadVdiIdentity(input),
            "chd" => ReadChdIdentity(input),
            _ => []
        };
    }

    public static IReadOnlyList<string> ReadMembers(Stream image, string imagePath)
    {
        if (!image.CanRead || !image.CanSeek) throw new ArgumentException("A readable seekable image is required.");
        using var input = new DifferencingImageStreams.ReadOnlyParent(image);
        return DiskContainerSignatures.Identify(input) == "vmdk"
            ? ReadVmdk(input, imagePath).Members.Select(path => Resolve(imagePath, path)).Distinct(PathComparer).ToArray()
            : [];
    }

    internal static StringComparer PathComparer => OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

    internal static string Resolve(string imagePath, string dependency)
    {
        if (string.IsNullOrWhiteSpace(dependency) || dependency.Contains('\0') || dependency.StartsWith("\\\\.\\", StringComparison.Ordinal) ||
            dependency.StartsWith("\\\\?\\", StringComparison.Ordinal) || dependency.AsSpan(Math.Min(2, dependency.Length)).Contains(':'))
            throw new InvalidDataException("An image dependency does not identify a regular file path.");
        if (dependency.Length > 1 && dependency[1] == ':' && !Path.IsPathFullyQualified(dependency))
            throw new InvalidDataException("Drive-relative image dependencies are ambiguous.");
        return Path.GetFullPath(dependency, Path.GetDirectoryName(Path.GetFullPath(imagePath))!);
    }

    private static string[] ReadBackingName(Stream image, ulong offset, uint count)
    {
        if (offset == 0 && count == 0) return [];
        if (offset == 0 || count == 0) throw new InvalidDataException("The backing image reference is incomplete.");
        return [new UTF8Encoding(false, true).GetString(ReadBytes(image, offset, count))];
    }

    private static IEnumerable<DiskImageDependencyReference> Paths(IEnumerable<string> paths) =>
        paths.Select(DiskImageDependencyReference.FromPath);

    private static IReadOnlyList<DiskImageDependencyReference> DistinctReferences(
        IEnumerable<DiskImageDependencyReference> references)
    {
        var paths = new HashSet<string>(PathComparer);
        var uuids = new HashSet<Guid>();
        var hashes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        return references.Where(reference =>
                reference.Path is { } path ? paths.Add(path) :
                reference.Uuid is { } uuid ? uuids.Add(uuid) :
                reference.Sha1 is { } sha1 && hashes.Add(sha1))
            .ToArray();
    }

    private static IReadOnlyList<DiskImageDependencyReference> ReadVdiParent(byte[] header)
    {
        var parentUuid = new Guid(header.AsSpan(0x1A8, 16));
        if (parentUuid == Guid.Empty) throw new InvalidDataException("The differencing VDI parent UUID is empty.");
        return [DiskImageDependencyReference.FromUuid(parentUuid)];
    }

    private static IReadOnlyList<DiskImageDependencyReference> ReadVdiIdentity(Stream image)
    {
        var header = ReadBytes(image, 0, VdiHeaderWithParentIdentifiersSize);
        var uuid = new Guid(header.AsSpan(0x188, 16));
        return uuid == Guid.Empty ? [] : [DiskImageDependencyReference.FromUuid(uuid)];
    }

    private static IReadOnlyList<DiskImageDependencyReference> ReadChdIdentity(Stream image)
    {
        var header = ReadBytes(image, 0, 124);
        if (BinaryPrimitives.ReadUInt32BigEndian(header.AsSpan(12)) != ChdVersion5)
            throw new NotSupportedException("Only CHD V5 identities are supported.");
        return header.AsSpan(84, 20).ContainsAnyExcept((byte)0)
            ? [DiskImageDependencyReference.FromSha1(header.AsSpan(84, 20))]
            : [];
    }

    private static byte[] ReadBytes(Stream image, ulong offset, uint count)
    {
        if (count > MaximumMetadataBytes || offset > (ulong)image.Length || count > (ulong)image.Length - offset)
            throw new InvalidDataException("Image dependency metadata is outside the image or exceeds its supported size.");
        var bytes = new byte[count]; image.Position = (long)offset; image.ReadExactly(bytes); return bytes;
    }

    private static VmdkReferences ReadVmdk(Stream image, string imagePath)
    {
        var header = ReadBytes(image, 0, (uint)Math.Min(512, image.Length));
        var embedded = header.AsSpan().StartsWith("KDMV"u8);
        byte[] bytes;
        if (embedded)
        {
            if (header.Length < 512) throw new InvalidDataException("Truncated VMDK sparse header.");
            var offset = BinaryPrimitives.ReadUInt64LittleEndian(header.AsSpan(28));
            var size = BinaryPrimitives.ReadUInt64LittleEndian(header.AsSpan(36));
            if (offset == 0 && size == 0) return new([], []); // A hosted sparse extent belonging to an external descriptor.
            if (size == 0 || size > MaximumMetadataBytes / 512 || offset > ulong.MaxValue / 512)
                throw new InvalidDataException("Invalid VMDK descriptor extent.");
            bytes = ReadBytes(image, offset * 512, checked((uint)(size * 512)));
        }
        else bytes = ReadBytes(image, 0, checked((uint)Math.Min(image.Length, MaximumMetadataBytes + 1L)));
        var text = new UTF8Encoding(false, true).GetString(bytes).TrimEnd('\0');
        var dependencies = new List<string>();
        var members = new List<string>();
        var extentCount = 0;
        var properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var raw in text.Split('\n'))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line[0] == '#') continue;
            var extent = ExtentLine().Match(line);
            if (extent.Success)
            {
                extentCount++;
                var type = extent.Groups[3].Value;
                if (type == "ZERO") continue;
                if (type is not ("FLAT" or "SPARSE" or "VMFS" or "VMFSSPARSE") || !extent.Groups[4].Success)
                    throw new NotSupportedException("Unsupported VMDK extent dependency.");
                var path = extent.Groups[4].Value;
                if (!embedded && !PathComparer.Equals(Resolve(imagePath, path), Path.GetFullPath(imagePath)))
                {
                    dependencies.Add(path);
                    members.Add(path);
                }
                continue;
            }
            var equals = line.IndexOf('=');
            if (equals <= 0) throw new InvalidDataException("Unrecognized VMDK descriptor line.");
            var key = line[..equals].Trim();
            var value = line[(equals + 1)..].Trim();
            if (value.StartsWith('"'))
            {
                var end = value.IndexOf('"', 1);
                if (end < 0 || value[(end + 1)..].TrimStart() is var tail && tail.Length != 0 && !tail.StartsWith('#'))
                    throw new InvalidDataException("Invalid quoted VMDK property.");
                value = value[1..end];
            }
            else value = value.Split('#')[0].Trim();
            if (!properties.TryAdd(key, value)) throw new InvalidDataException("Duplicate VMDK descriptor property.");
        }
        if (!properties.TryGetValue("version", out var version) || version != "1" ||
            !properties.TryGetValue("parentCID", out var parentCid))
            throw new InvalidDataException("Incomplete VMDK descriptor.");
        if (embedded && (extentCount != 1 || !properties.TryGetValue("createType", out var createType) ||
            createType is not ("monolithicSparse" or "streamOptimized")))
            throw new NotSupportedException("Unsupported embedded VMDK extent arrangement.");
        if (properties.TryGetValue("parentFileNameHint", out var parent)) dependencies.Add(parent);
        else if (!parentCid.Equals("ffffffff", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("The VMDK parent reference is missing.");
        return new(dependencies, members);
    }

    private sealed record VmdkReferences(IReadOnlyList<string> Dependencies, IReadOnlyList<string> Members);

    [GeneratedRegex("^(RW|RDONLY|NOACCESS)\\s+([0-9]+)\\s+([A-Z]+)(?:\\s+\"([^\"\\r\\n]+)\")?(?:\\s+[0-9]+)?\\s*(?:#.*)?$", RegexOptions.CultureInvariant, 100)]
    private static partial Regex ExtentLine();
}
