using System.Collections.Frozen;
using GWGUI.Emulation.HardDisks.Containers;
using GWGUI.Emulation.HardDisks.FileSystems;
using GWGUI.Emulation.HardDisks.Partitioning;

namespace GWGUI.Emulation.HardDisks;

/// <summary>Only registered implementations are advertised as constructible. Registries are instance-owned.</summary>
public sealed class DiskFormatRegistry
{
    public sealed record ImageSet(string Id, string EntryPoint, Action<long, bool> Validate,
        Action<long, Action<string, Stream>, Action<Stream>> Write, DiskFormatIdentity Identity)
    {
        public IReadOnlySet<int>? LogicalSectorSizes { get; init; }
        public DiskFormatOperations Operations => DiskFormatOperations.Create;
    }
    public sealed record Container(string Id, Action<long, bool> Validate,
        Action<Stream, long, bool, Action<Stream>> Write)
    {
        public DiskFormatIdentity? Identity { get; init; }
        public IReadOnlySet<int>? LogicalSectorSizes { get; init; }
        public DiskFormatOperations Operations => DiskFormatOperations.Create;
    }
    public sealed record FileSystem(string Id, Action<DiskVolumePlan> Validate,
        Action<Stream, DiskVolumePlan> Format, IReadOnlySet<int>? SectorSizes = null)
    {
        public DiskFormatIdentity? Identity { get; init; }
        public DiskFormatOperations Operations => DiskFormatOperations.Create;
    }
    public sealed record PartitionTable(string Id, Action<long, IReadOnlyList<DiskVolumePlan>> Validate,
        Action<Stream, IReadOnlyList<DiskVolumePlan>> Write)
    {
        public DiskFormatIdentity? Identity { get; init; }
        public int LogicalSectorBytes { get; init; } = 512;
        public bool SupportsMbrLogical { get; init; }
        public DiskChsGeometry? BiosGeometry { get; init; }
        public DiskFormatOperations Operations => DiskFormatOperations.Create;
    }

    private readonly Dictionary<string, Container> containers = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, ImageSet> imageSets = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, FileSystem> fileSystems = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, PartitionTable> partitionTables = new(StringComparer.OrdinalIgnoreCase);
    public IReadOnlyList<string> ContainerIds => containers.Keys.Order().ToArray();
    public IReadOnlyList<string> FileSystemIds => fileSystems.Keys.Order().ToArray();
    public IReadOnlyList<string> PartitionTableIds => partitionTables.Keys.Order().ToArray();
    public IReadOnlyList<Container> Containers => containers.Values.OrderBy(value => value.Id).ToArray();
    public IReadOnlyList<ImageSet> ImageSets => imageSets.Values.OrderBy(value => value.Id).ToArray();
    public IReadOnlyList<FileSystem> FileSystems => fileSystems.Values.OrderBy(value => value.Id).ToArray();
    public IReadOnlyList<PartitionTable> PartitionTables => partitionTables.Values.OrderBy(value => value.Id).ToArray();

    /// <summary>Returns candidates only. A suffix cannot establish the container or its version.</summary>
    public IReadOnlyList<Container> FindContainersByExtension(string extension)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(extension);
        return Containers.Where(value => value.Identity?.Extensions.Contains(extension, StringComparer.OrdinalIgnoreCase) == true).ToArray();
    }
    public void Register(Container value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value.Id);
        if (imageSets.ContainsKey(value.Id)) throw new ArgumentException("An image set already uses this container identifier.");
        containers.Add(value.Id, value with { LogicalSectorSizes = value.LogicalSectorSizes?.ToFrozenSet() });
    }
    public void Register(ImageSet value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value.Id);
        DiskImageSetPublication.ValidateMemberPath(value.EntryPoint);
        if (containers.ContainsKey(value.Id)) throw new ArgumentException("A single-file container already uses this identifier.");
        imageSets.Add(value.Id, value with { LogicalSectorSizes = value.LogicalSectorSizes?.ToFrozenSet() });
    }
    public void Register(FileSystem value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value.Id);
        fileSystems.Add(value.Id, value with { SectorSizes = value.SectorSizes?.ToFrozenSet() });
    }
    public void Register(PartitionTable value) { ArgumentException.ThrowIfNullOrWhiteSpace(value.Id); partitionTables.Add(value.Id, value); }
    internal Container GetContainer(string id) => containers.TryGetValue(id, out var value) ? value : throw new NotSupportedException($"Unknown container: {id}");
    internal ImageSet GetImageSet(string id) => imageSets.TryGetValue(id, out var value) ? value : throw new NotSupportedException($"Unknown image set: {id}");
    internal FileSystem GetFileSystem(string id) => fileSystems.TryGetValue(id, out var value) ? value : throw new NotSupportedException($"Unknown filesystem: {id}");
    internal PartitionTable GetPartitionTable(string id) => partitionTables.TryGetValue(id, out var value) ? value : throw new NotSupportedException($"Unknown partition table: {id}");

    public static DiskFormatRegistry CreateDefault()
    {
        var registry = new DiskFormatRegistry();
        registry.Register(new ImageSet("sparsebundle", "Info.plist", (size, fixedSize) =>
        {
            SparseBundleImageWriter.Validate(size);
            if (fixedSize) throw new NotSupportedException("This sparsebundle profile does not provide fixed allocation.");
        }, (size, emit, initialize) => SparseBundleImageWriter.Write(size, emit, initialize),
            new("sparsebundle", "Unencrypted v1, 8 MiB bands", [".sparsebundle"])));
        foreach (var kind in Enum.GetValues<VmdkImageSetKind>())
        {
            var id = kind switch
            {
                VmdkImageSetKind.VmfsFlat => "vmdk-vmfs-flat", VmdkImageSetKind.VmfsSparse => "vmdk-vmfs-sparse",
                VmdkImageSetKind.SplitFlat => "vmdk-split-flat", VmdkImageSetKind.SplitSparse => "vmdk-split-sparse",
                _ => "vmdk-monolithic-flat"
            };
            registry.Register(new ImageSet(id, "disk.vmdk", (size, fixedSize) =>
            {
                if (size < 512 || size % 512 != 0 || size > 1L << 40) throw new ArgumentOutOfRangeException(nameof(size));
                if (fixedSize && kind is VmdkImageSetKind.VmfsSparse or VmdkImageSetKind.SplitSparse)
                    throw new NotSupportedException("This image set does not provide fixed allocation.");
            }, (size, emit, initialize) => VmdkImageSetWriter.Write(size, "disk", kind, emit, initialize),
                new("VMDK", kind.ToString(), [".vmdk"], "vmdk")));
        }
        foreach (var kind in Enum.GetValues<DiskContainerKind>())
            registry.Register(new Container(kind.ToString().ToLowerInvariant(), (size, fixedSize) =>
            {
                if (size < 512 || size % 512 != 0) throw new ArgumentOutOfRangeException(nameof(size));
                if (kind == DiskContainerKind.Vhd) VhdImageWriter.Validate(size);
                if (kind == DiskContainerKind.Vhdx) VhdxImageWriter.Validate(size);
                if (kind == DiskContainerKind.Hdi) HdiImageWriter.Validate(size);
                if (kind == DiskContainerKind.Nhd) NhdImageWriter.Validate(size);
                if (kind == DiskContainerKind.Thd) ThdImageWriter.Validate(size);
                if (kind == DiskContainerKind.Qcow2 && size > 1L << 40) throw new ArgumentOutOfRangeException(nameof(size));
                if (kind == DiskContainerKind.TwoImg && size > int.MaxValue - 64) throw new ArgumentOutOfRangeException(nameof(size));
                if (kind == DiskContainerKind.Qed && size > 1L << 42) throw new ArgumentOutOfRangeException(nameof(size));
                if (kind == DiskContainerKind.Chd && size > ChdImageWriter.MaximumCapacity) throw new ArgumentOutOfRangeException(nameof(size));
                if ((kind is DiskContainerKind.Parallels or DiskContainerKind.Qcow) && size > 1L << 40) throw new ArgumentOutOfRangeException(nameof(size));
                if (fixedSize && kind is DiskContainerKind.Gzip or DiskContainerKind.Vmdk or DiskContainerKind.Qcow2 or DiskContainerKind.Qed or DiskContainerKind.Chd or DiskContainerKind.Parallels or DiskContainerKind.Qcow)
                    throw new NotSupportedException("This container writer does not provide fixed allocation.");
            }, (stream, size, fixedSize, initialize) => DiskContainerWriter.Write(stream, size, kind, initialize, fixedSize)));

        registry.Register(new Container("sparseimage", (size, fixedSize) =>
        {
            SparseImageWriter.Validate(size);
            if (fixedSize) throw new NotSupportedException("This sparseimage profile does not provide fixed allocation.");
        }, (stream, size, _, initialize) => SparseImageWriter.Write(stream, size, initialize))
        { Identity = new("sparseimage", "Unencrypted v3, one index header, 8 MiB bands", [".sparseimage"], "sparseimage") });
        registry.Register(new Container("vmdk-stream", (size, fixedSize) =>
        {
            VmdkStreamImageWriter.Validate(size);
            if (fixedSize) throw new NotSupportedException("streamOptimized does not provide fixed allocation.");
        }, (stream, size, _, initialize) => VmdkStreamImageWriter.Write(stream, size, initialize))
        { Identity = new("VMDK", "Autonomous streamOptimized, 64 KiB zlib grains", [".vmdk"], "vmdk") });
        registry.Register(new Container("vhdx-4kn", (size, _) => VhdxImageWriter.Validate(size, 4096),
            (stream, size, fixedSize, initialize) => VhdxImageWriter.Write(stream, size, fixedSize, initialize, 4096))
        {
            Identity = new("VHDX", "Autonomous fixed or dynamic, 4096-byte logical sectors", [".vhdx"], "vhdx"),
            LogicalSectorSizes = new HashSet<int> { 4096 }
        });
        registry.Register(new Container("qcow2-v2", (size, fixedSize) =>
        {
            Qcow2ImageWriter.Validate(size, version: 2);
            if (fixedSize) throw new NotSupportedException("QCOW2 fixed allocation is not implemented.");
        }, (stream, size, _, initialize) => Qcow2ImageWriter.Write(stream, size, initialize, version: 2)));
        foreach (var compressed in new[] { false, true })
            registry.Register(new Container(compressed ? "udif-zlib" : "udif", (size, fixedSize) =>
            {
                UdifImageWriter.Validate(size);
                if (fixedSize) throw new NotSupportedException("UDIF fixed allocation is not implemented.");
            }, (stream, size, _, initialize) => UdifImageWriter.Write(stream, size, initialize, compressed)));
        foreach (var version in new[] { 1, 2 })
            registry.Register(new Container($"bochs-v{version}", (size, fixedSize) =>
            {
                BochsImageWriter.Validate(size, version);
                if (fixedSize) throw new NotSupportedException("Growing redolog does not provide fixed allocation.");
            }, (stream, size, _, initialize) => BochsImageWriter.Write(stream, size, initialize, version)));
        registry.Register(new Container("cloop-v2", (size, fixedSize) =>
        {
            CompressedLoopImageWriter.Validate(size);
            if (fixedSize) throw new NotSupportedException("Compressed loop does not provide fixed allocation.");
        }, (stream, size, _, initialize) => CompressedLoopImageWriter.Write(stream, size, initialize)));
        registry.Register(new FileSystem("none", _ => { }, (_, _) => { }));
        foreach (var pageBytes in new[] { 4096, 8192, 16384, 32768, 65536 })
        foreach (var bigEndian in new[] { false, true })
        {
            var id = $"linux-swap-{pageBytes / 1024}k" + (bigEndian ? "-be" : "");
            registry.Register(new FileSystem(id, volume => LinuxSwapVolumeFormatter.Validate(volume.LengthBytes, volume.Label, pageBytes),
                (stream, volume) => LinuxSwapVolumeFormatter.Format(stream, volume.Label, pageBytes, bigEndian))
            { Identity = new("Linux swap", $"SWAPSPACE2 v1, {pageBytes}-byte pages, {(bigEndian ? "big" : "little")}-endian integers") });
        }
        registry.Register(new FileSystem("bfs", volume => BfsVolumeFormatter.Validate(volume.LengthBytes, volume.Label),
            (stream, volume) => BfsVolumeFormatter.Format(stream, volume.Label)));
        registry.Register(new FileSystem("pfs3", volume => Pfs3VolumeFormatter.Validate(volume.LengthBytes, volume.Label),
            (stream, volume) => Pfs3VolumeFormatter.Format(stream, volume.Label))
        { Identity = new("PFS3", "512-byte sectors, 1024-byte reserved blocks, super-index supported") });
        foreach (var fast in new[] { false, true })
            registry.Register(new FileSystem(fast ? "ffs-longnames" : "ofs-longnames",
                volume => LongNameDosVolumeFormatter.Validate(volume.LengthBytes, volume.Label),
                (stream, volume) => LongNameDosVolumeFormatter.Format(stream, volume.Label, fast))
            { Identity = new("OFS / FFS", fast ? "DOS7, long names, 512-byte blocks" : "DOS6, long names, 512-byte blocks") });
        registry.Register(new FileSystem("d90-dos", volume => D90VolumeFormatter.Validate(volume.LengthBytes, volume.Label),
            (stream, volume) => D90VolumeFormatter.Format(stream, volume.Label), new HashSet<int> { 256 }));
        foreach (var (id, order) in new[] { ("v7-le", StorageByteOrder.LittleEndian), ("v7-be", StorageByteOrder.BigEndian), ("v7-pdp", StorageByteOrder.PdpEndian) })
            registry.Register(new FileSystem(id, volume =>
            {
                if (!string.IsNullOrEmpty(volume.Label)) throw new ArgumentException("This V7 profile has no volume label field.");
                V7VolumeFormatter.Validate(volume.LengthBytes, order);
            }, (stream, _) => V7VolumeFormatter.Format(stream, order)));
        registry.Register(new FileSystem("fatx-be",volume=>
        {
            if(!string.IsNullOrEmpty(volume.Label))throw new ArgumentException("This FATX formatter does not install a volume label.");
            FatxVolumeFormatter.Validate(volume.LengthBytes,volume.SectorsPerCluster==0?32:volume.SectorsPerCluster,true);
        },(stream,volume)=>FatxVolumeFormatter.Format(stream,volume.SectorsPerCluster==0?32:volume.SectorsPerCluster,true)));
        foreach(var version in new[]{1,2,3})
        foreach (var order in Enum.GetValues<MinixStorageOrder>())
        foreach (var nameLength in version == 3 ? new[] { 60 } : new[] { 30, 14 })
        {
            var suffix = order switch { MinixStorageOrder.BigEndian16 => "-be16", MinixStorageOrder.BigEndian32 => "-be32",
                MinixStorageOrder.BigEndian64 => "-be64", _ => "" };
            var id = $"minix{version}{suffix}{(nameLength == 14 ? "-n14" : "")}";
            registry.Register(new FileSystem(id,volume=>
            {
                if(!string.IsNullOrEmpty(volume.Label)) throw new ArgumentException("Minix has no volume label field.");
                MinixVolumeFormatter.Validate(volume.LengthBytes,version,order:order,nameLength:nameLength);
            },(stream,_)=>MinixVolumeFormatter.Format(stream,version,order:order,nameLength:nameLength))
            { Identity = new("Minix", $"Version {version}, {order}, {nameLength}-byte names, 1 KiB blocks") });
        }
        registry.Register(new FileSystem("hfs", volume => ClassicHfsVolumeFormatter.Validate(volume.LengthBytes, volume.Label),
            (stream, volume) => ClassicHfsVolumeFormatter.Format(stream, volume.Label)));
        registry.Register(new FileSystem("mfs", volume => MfsVolumeFormatter.Validate(volume.LengthBytes, volume.Label),
            (stream, volume) => MfsVolumeFormatter.Format(stream, volume.Label)));
        registry.Register(new FileSystem("pascal", volume => PascalVolumeFormatter.Validate(volume.LengthBytes, volume.Label),
            (stream, volume) => PascalVolumeFormatter.Format(stream, volume.Label)));
        registry.Register(new FileSystem("hfsplus", volume => HfsPlusVolumeFormatter.Validate(volume.LengthBytes, volume.Label),
            (stream, volume) => HfsPlusVolumeFormatter.Format(stream, volume.Label)));
        registry.Register(new FileSystem("hfsx", volume => HfsxVolumeFormatter.Validate(volume.LengthBytes, volume.Label),
            (stream, volume) => HfsxVolumeFormatter.Format(stream, volume.Label)));
        registry.Register(new FileSystem("fatx", volume =>
        {
            if (!string.IsNullOrEmpty(volume.Label)) throw new ArgumentException("This FATX formatter does not install a volume label.");
            FatxVolumeFormatter.Validate(volume.LengthBytes, volume.SectorsPerCluster == 0 ? 32 : volume.SectorsPerCluster);
        }, (stream, volume) => FatxVolumeFormatter.Format(stream, volume.SectorsPerCluster == 0 ? 32 : volume.SectorsPerCluster)));
        registry.Register(new FileSystem("ext2", volume => Ext2VolumeFormatter.Validate(volume.LengthBytes, volume.Label),
            (stream, volume) => Ext2VolumeFormatter.Format(stream, volume.Label)));
        registry.Register(new FileSystem("ext3", volume => Ext3VolumeFormatter.Validate(volume.LengthBytes, volume.Label),
            (stream, volume) => Ext3VolumeFormatter.Format(stream, volume.Label)));
        registry.Register(new FileSystem("ext4", volume => Ext4VolumeFormatter.Validate(volume.LengthBytes, volume.Label),
            (stream, volume) => Ext4VolumeFormatter.Format(stream, volume.Label)));
        foreach (var variant in Enum.GetValues<FatVariant>())
            registry.Register(new FileSystem(variant.ToString().ToLowerInvariant(),
                volume => ExplicitFatVolumeFormatter.Validate(volume.LengthBytes, volume.Label, variant, volume.SectorsPerCluster, volume.OffsetBytes / volume.SectorBytes,volume.SectorBytes),
                (stream, volume) => ExplicitFatVolumeFormatter.Format(stream, volume.Label, variant, volume.SectorsPerCluster, volume.OffsetBytes / volume.SectorBytes,volume.SectorBytes, volume.BiosGeometry),
                new HashSet<int>{512,1024,2048,4096}));
        registry.Register(new FileSystem("exfat", volume => ExFatVolumeFormatter.Validate(volume.LengthBytes, volume.Label),
            (stream, volume) => ExFatVolumeFormatter.Format(stream, volume.Label, volume.OffsetBytes / 512)));
        registry.Register(new FileSystem("prodos", volume => ProDosVolumeFormatter.Validate(volume.LengthBytes, volume.Label),
            (stream, volume) => ProDosVolumeFormatter.Format(stream, volume.Label)));
        registry.Register(new FileSystem("cpm22", volume => CpmVolumeFormatter.Validate(volume.LengthBytes,
            volume.CpmOptions ?? throw new ArgumentException("CP/M requires explicit disk parameters.")),
            (stream, volume) => CpmVolumeFormatter.Format(stream, volume.CpmOptions!)));
        registry.Register(new FileSystem("fat", volume => ValidateVolume(volume, 8L << 20, (long)int.MaxValue * 512),
            (stream, volume) => FatVolumeFormatter.Format(stream, volume.Label, volume.OffsetBytes / 512, volume.BiosGeometry)));
        registry.Register(new FileSystem("ntfs", volume => ValidateVolume(volume, 16L << 20, (long)int.MaxValue * 512),
            (stream, volume) => NtfsVolumeFormatter.Format(stream, volume.Label, volume.OffsetBytes / 512, volume.BiosGeometry)));
        registry.Register(new FileSystem("fat16-adapted", volume => ValidateVolume(volume, 8L << 20, 1L << 30),
            (stream, _) => AtariFat16VolumeFormatter.Format(stream)));
        foreach (var fast in new[] { false, true })
            registry.Register(new FileSystem(fast ? "ffs" : "ofs", volume =>
            {
                ValidateVolume(volume, 1L << 20, (2L << 30) - 512);
                if (volume.Label.Length is < 1 or > 30 || volume.Label.Any(c => c < 32 || c > 255 || c is ':' or '/'))
                    throw new ArgumentException("Invalid volume label.");
            }, (stream, volume) => AmigaDosVolumeFormatter.Format(stream, volume.Label, fast)));
        foreach (var (id, variant) in new (string,byte)[] { ("ofs-intl",2),("ffs-intl",3),("ofs-dircache",4),("ffs-dircache",5) })
            registry.Register(new FileSystem(id, volume => AmigaDosVolumeFormatter.Validate(volume.LengthBytes,volume.Label,variant),
                (stream,volume)=>AmigaDosVolumeFormatter.Format(stream,volume.Label,variant)));
        registry.Register(new PartitionTable("none", (size, volumes) =>
        {
            if (volumes.Count > 1 || volumes.Any(v => v.OffsetBytes != 0 || v.LengthBytes != size))
                throw new ArgumentException("A direct volume must occupy the entire disk.");
        }, (_, _) => { }) { LogicalSectorBytes = 0 });
        registry.Register(new PartitionTable("mbr", ComposedPartitionWriter.ValidateMbr, ComposedPartitionWriter.WriteMbr)
        { SupportsMbrLogical = true });
        registry.Register(new PartitionTable("mbr-chs", (size, volumes) => ChsMbrPartitionWriter.Validate(size, volumes),
            (stream, volumes) => ChsMbrPartitionWriter.Write(stream, volumes))
        { SupportsMbrLogical = true, BiosGeometry = new(), Identity = new("MBR", "CHS 16 heads, 63 sectors per track, extended type 05") });
        registry.Register(new PartitionTable("gpt", ComposedPartitionWriter.ValidateGpt, ComposedPartitionWriter.WriteGpt));
        foreach (var sectorBytes in new[] { 1024, 2048, 4096 })
            registry.Register(GptPartitionWriter.Describe($"gpt-{sectorBytes}", sectorBytes));
        registry.Register(new PartitionTable("apm", ApmPartitionWriter.Validate, ApmPartitionWriter.Write));
        registry.Register(new PartitionTable("ahdi", AhdiPartitionWriter.Validate, AhdiPartitionWriter.Write));
        registry.Register(new PartitionTable("icd", IcdPartitionWriter.Validate, IcdPartitionWriter.Write));
        registry.Register(new PartitionTable("bsd-disklabel", BsdDisklabelWriter.Validate, BsdDisklabelWriter.Write));
        registry.Register(new PartitionTable("bsd-disklabel-be", BsdDisklabelWriter.Validate,
            (stream,volumes)=>BsdDisklabelWriter.Write(stream,volumes,true)));
        registry.Register(new PartitionTable("rdb", RdbPartitionWriter.Validate, RdbPartitionWriter.Write));
        registry.Register(new PartitionTable("sun-vtoc", SunDisklabelWriter.Validate, SunDisklabelWriter.Write));
        registry.Register(SunDisklabelWriter.Describe("sun-vtoc-backup", new(), new()));
        registry.Register(new PartitionTable("sgi", SgiDisklabelWriter.Validate, SgiDisklabelWriter.Write));
        foreach (var value in registry.Containers)
            registry.containers[value.Id] = value with
            {
                Identity = value.Identity ?? DefaultDiskFormatIdentities.Container(value.Id),
                LogicalSectorSizes = value.LogicalSectorSizes ?? (value.Id is "raw" or "gzip" ? new[] { 128, 256, 512, 1024, 2048, 4096 }.ToFrozenSet() : null)
            };
        foreach (var value in registry.FileSystems)
            registry.fileSystems[value.Id] = value with { Identity = value.Identity ?? DefaultDiskFormatIdentities.FileSystem(value.Id) };
        foreach (var value in registry.PartitionTables)
            registry.partitionTables[value.Id] = value with { Identity = value.Identity ?? DefaultDiskFormatIdentities.PartitionTable(value.Id) };
        return registry;
    }

    private static void ValidateVolume(DiskVolumePlan volume, long minimum, long maximum)
    {
        if (volume.LengthBytes < minimum || volume.LengthBytes > maximum || volume.LengthBytes % 512 != 0)
            throw new ArgumentOutOfRangeException(nameof(volume));
    }
}
