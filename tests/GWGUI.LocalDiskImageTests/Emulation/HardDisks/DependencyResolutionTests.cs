using System.Buffers.Binary;
using System.Text;
using GWGUI.Emulation.HardDisks;
using GWGUI.Emulation.HardDisks.Containers;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class DependencyResolutionTests
{
    private static string ImagePath(string name) => Path.GetFullPath(Path.Combine("dependency-memory", name));

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void DifferentialParentsResolveRelativeToTheChildAndPreserveInput(bool vhdx)
    {
        var extension = vhdx ? ".vhdx" : ".vhd";
        var parentPath = ImagePath("parent" + extension);
        var childPath = ImagePath("child" + extension);
        using var parent = new MemoryStream(); using var child = new MemoryStream();
        if (vhdx)
        {
            VhdxImageWriter.Write(parent, 4L << 20);
            VhdxDifferencingImageWriter.Write(child, parent, parentPath, "parent" + extension, DateTime.UnixEpoch.AddYears(40));
        }
        else
        {
            VhdImageWriter.Write(parent, 4L << 20);
            VhdDifferencingImageWriter.Write(child, parent, parentPath, "parent" + extension, DateTime.UnixEpoch.AddYears(40));
        }
        var original = child.ToArray(); child.Position = 17;
        Assert.Equal([parentPath], DiskImageDependencyReader.Read(child, childPath));
        Assert.Equal(17, child.Position); Assert.Equal(original, child.ToArray()); Assert.True(child.CanWrite);
        var files = new Dictionary<string, byte[]> { [parentPath] = parent.ToArray(), [childPath] = child.ToArray() };
        var index = new DiskImageDependencyIndex(path => new MemoryStream(files[path], writable: false));
        Assert.True(index.Uses(childPath, parentPath));
        Assert.False(index.Uses(childPath, ImagePath("unrelated.img")));
    }

    [Theory]
    [InlineData(VmdkImageSetKind.VmfsFlat)] [InlineData(VmdkImageSetKind.VmfsSparse)]
    [InlineData(VmdkImageSetKind.SplitFlat)] [InlineData(VmdkImageSetKind.SplitSparse)]
    [InlineData(VmdkImageSetKind.MonolithicFlat)]
    public void EveryEmittedVmdkExtentIsProtectedByTheDescriptor(VmdkImageSetKind kind)
    {
        var files = new Dictionary<string, byte[]>();
        VmdkImageSetWriter.Write(65536, "disk", kind, (name, stream) =>
        {
            using var copy = new MemoryStream(); stream.CopyTo(copy); files.Add(ImagePath(name), copy.ToArray());
        });
        var index = new DiskImageDependencyIndex(path => new MemoryStream(files[path], writable: false));
        var descriptor = ImagePath("disk.vmdk");
        foreach (var member in files.Keys.Where(path => path != descriptor)) Assert.True(index.Uses(descriptor, member));
        Assert.False(index.Uses(descriptor, ImagePath("unrelated.img")));
    }

    [Fact]
    public void MonolithicSparseSelfExtentDoesNotCreateADependencyCycle()
    {
        using var image = new MemoryStream(); VmdkImageWriter.Write(image, 65536);
        var bytes = image.ToArray();
        var index = new DiskImageDependencyIndex(_ => new MemoryStream(bytes, writable: false));
        Assert.False(index.Uses(ImagePath("disk.vmdk"), ImagePath("unrelated.img")));
        Assert.False(index.Uses(ImagePath("renamed.vmdk"), ImagePath("unrelated.img")));
    }

    [Fact]
    public void ParentChainsCyclesAndMissingReferencesAreHandledExplicitly()
    {
        byte[] Descriptor(string parent) => Encoding.UTF8.GetBytes(
            $"# Disk DescriptorFile\nversion=1\nparentCID=12345678\nparentFileNameHint=\"{parent}\"\nRW 128 ZERO\n");
        var files = new Dictionary<string, byte[]>
        {
            [ImagePath("child.vmdk")] = Descriptor("parent.vmdk"),
            [ImagePath("parent.vmdk")] = Descriptor("root.vmdk"),
            [ImagePath("root.vmdk")] = new byte[65536]
        };
        var index = new DiskImageDependencyIndex(path => new MemoryStream(files[path], writable: false));
        Assert.True(index.Uses(ImagePath("child.vmdk"), ImagePath("root.vmdk")));
        files[ImagePath("root.vmdk")] = Descriptor("child.vmdk");
        index = new(path => new MemoryStream(files[path], writable: false));
        Assert.Throws<InvalidDataException>(() => index.Uses(ImagePath("child.vmdk"), ImagePath("unrelated.img")));
        using var invalid = new MemoryStream(Encoding.ASCII.GetBytes("# Disk DescriptorFile\nversion=1\nparentCID=12345678\n"));
        Assert.Throws<InvalidDataException>(() => DiskImageDependencyReader.Read(invalid, ImagePath("missing.vmdk")));
    }

    [Fact]
    public void BackingReferencesAreBoundedAndResolvedWithoutOpeningTheParent()
    {
        var bytes = new byte[512]; new byte[] { 0x51, 0x46, 0x49, 0xfb }.CopyTo(bytes, 0);
        BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(4), 3);
        BinaryPrimitives.WriteUInt64BigEndian(bytes.AsSpan(8), 128);
        var name = Encoding.UTF8.GetBytes("base.img");
        BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(16), (uint)name.Length);
        name.CopyTo(bytes, 128);
        using var image = new MemoryStream(bytes);
        Assert.Equal([ImagePath("base.img")], DiskImageDependencyReader.Read(image, ImagePath("child.qcow2")));
        BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(16), 1000000);
        Assert.Throws<InvalidDataException>(() => DiskImageDependencyReader.Read(image, ImagePath("child.qcow2")));
    }

    [Fact]
    public void ADirectReferenceBlocksDeletionWithoutOpeningTheCandidate()
    {
        var index = new DiskImageDependencyIndex(_ => throw new InvalidOperationException("No read should occur"));
        Assert.True(index.Uses(ImagePath("disk.img"), ImagePath("disk.img")));
        Assert.Throws<InvalidDataException>(() => DiskImageDependencyReader.Resolve(ImagePath("disk.vmdk"), "\\\\.\\PhysicalDrive0"));
        Assert.Throws<InvalidDataException>(() => DiskImageDependencyReader.Resolve(ImagePath("disk.vmdk"), "disk.img:stream"));
    }

    [Theory]
    [InlineData(DiskContainerKind.Raw)] [InlineData(DiskContainerKind.Gzip)]
    [InlineData(DiskContainerKind.Vhd)] [InlineData(DiskContainerKind.Vhdx)] [InlineData(DiskContainerKind.Vdi)]
    [InlineData(DiskContainerKind.Qcow)] [InlineData(DiskContainerKind.Qcow2)] [InlineData(DiskContainerKind.Qed)]
    [InlineData(DiskContainerKind.Chd)] [InlineData(DiskContainerKind.TwoImg)] [InlineData(DiskContainerKind.Parallels)]
    public void AutonomousConstructorsDoNotInventParentReferences(DiskContainerKind kind)
    {
        using var image = new MemoryStream(); DiskContainerWriter.Write(image, 4L << 20, kind);
        Assert.Empty(DiskImageDependencyReader.Read(image, ImagePath("disk.img")));
    }

    [Fact]
    public void VdiUuidAndChdSha1ReferencesAreReadWithTheirOwnIdentities()
    {
        var parentUuid = Guid.NewGuid();
        using var vdiParent = new MemoryStream(); VdiImageWriter.Write(vdiParent, 65536);
        var vdiParentBytes = vdiParent.ToArray(); parentUuid.TryWriteBytes(vdiParentBytes.AsSpan(0x188, 16));
        using var vdiChild = new MemoryStream(); VdiImageWriter.Write(vdiChild, 65536);
        var vdiChildBytes = vdiChild.ToArray();
        BinaryPrimitives.WriteUInt32LittleEndian(vdiChildBytes.AsSpan(0x4C, 4), 4);
        parentUuid.TryWriteBytes(vdiChildBytes.AsSpan(0x1A8, 16));
        using var vdiParentInput = new MemoryStream(vdiParentBytes);
        using var vdiChildInput = new MemoryStream(vdiChildBytes);
        Assert.Equal([DiskImageDependencyReference.FromUuid(parentUuid)],
            DiskImageDependencyReader.ReadIdentities(vdiParentInput));
        Assert.Equal([DiskImageDependencyReference.FromUuid(parentUuid)],
            DiskImageDependencyReader.ReadReferences(vdiChildInput, ImagePath("child.vdi")));

        var parentSha1 = Enumerable.Range(1, 20).Select(value => (byte)value).ToArray();
        using var chdParent = new MemoryStream(); ChdImageWriter.Write(chdParent, 65536);
        var chdParentBytes = chdParent.ToArray(); parentSha1.CopyTo(chdParentBytes, 84);
        using var chd = new MemoryStream(); ChdImageWriter.Write(chd, 65536);
        var chdChildBytes = chd.ToArray(); parentSha1.CopyTo(chdChildBytes, 104);
        using var chdParentInput = new MemoryStream(chdParentBytes);
        using var chdChildInput = new MemoryStream(chdChildBytes);
        Assert.Equal([DiskImageDependencyReference.FromSha1(parentSha1)],
            DiskImageDependencyReader.ReadIdentities(chdParentInput));
        Assert.Equal([DiskImageDependencyReference.FromSha1(parentSha1)],
            DiskImageDependencyReader.ReadReferences(chdChildInput, ImagePath("child.chd")));
        Assert.Throws<NotSupportedException>(() =>
            DiskImageDependencyReader.Read(new MemoryStream(chdChildBytes), ImagePath("child.chd")));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void IdentifierInventoriesResolveParentsAndRejectMissingOrWrongEntries(bool chd)
    {
        var parentPath = ImagePath(chd ? "parent.chd" : "parent.vdi");
        var childPath = ImagePath(chd ? "child.chd" : "child.vdi");
        var unrelatedPath = ImagePath(chd ? "unrelated.chd" : "unrelated.vdi");
        var files = new Dictionary<string, byte[]>();
        DiskImageDependencyReference reference;
        if (chd)
        {
            var sha1 = Enumerable.Range(1, 20).Select(value => (byte)value).ToArray();
            using var parent = new MemoryStream(); ChdImageWriter.Write(parent, 65536);
            var parentBytes = parent.ToArray(); sha1.CopyTo(parentBytes, 84); files[parentPath] = parentBytes;
            using var child = new MemoryStream(); ChdImageWriter.Write(child, 65536);
            var childBytes = child.ToArray(); sha1.CopyTo(childBytes, 104); files[childPath] = childBytes;
            using var unrelated = new MemoryStream(); ChdImageWriter.Write(unrelated, 65536); files[unrelatedPath] = unrelated.ToArray();
            reference = DiskImageDependencyReference.FromSha1(sha1);
        }
        else
        {
            var uuid = Guid.NewGuid();
            using var parent = new MemoryStream(); VdiImageWriter.Write(parent, 65536);
            var parentBytes = parent.ToArray(); uuid.TryWriteBytes(parentBytes.AsSpan(0x188, 16)); files[parentPath] = parentBytes;
            using var child = new MemoryStream(); VdiImageWriter.Write(child, 65536);
            var childBytes = child.ToArray();
            BinaryPrimitives.WriteUInt32LittleEndian(childBytes.AsSpan(0x4C, 4), 4);
            uuid.TryWriteBytes(childBytes.AsSpan(0x1A8, 16)); files[childPath] = childBytes;
            using var unrelated = new MemoryStream(); VdiImageWriter.Write(unrelated, 65536); files[unrelatedPath] = unrelated.ToArray();
            reference = DiskImageDependencyReference.FromUuid(uuid);
        }
        Stream Open(string path) => new MemoryStream(files[path], writable: false);
        var index = new DiskImageDependencyIndex(Open, resolveReference: (_, value) => value == reference ? parentPath : null);
        Assert.True(index.Uses(childPath, parentPath));
        Assert.False(index.Uses(childPath, unrelatedPath));
        var absent = new DiskImageDependencyIndex(Open);
        Assert.Throws<InvalidDataException>(() => absent.Uses(childPath, unrelatedPath));
        var wrong = new DiskImageDependencyIndex(Open, resolveReference: (_, _) => unrelatedPath);
        Assert.Throws<InvalidDataException>(() => wrong.Uses(childPath, unrelatedPath));
    }

    [Fact]
    public void UuidParentCyclesAndSegmentAliasesRemainOneLogicalDependency()
    {
        var firstPath = ImagePath("first.vdi");
        var secondPath = ImagePath("second.vdi");
        var unrelatedPath = ImagePath("unrelated.vdi");
        var firstUuid = Guid.NewGuid();
        var secondUuid = Guid.NewGuid();
        byte[] Differential(Guid ownUuid, Guid parentUuid)
        {
            using var image = new MemoryStream(); VdiImageWriter.Write(image, 65536);
            var bytes = image.ToArray();
            BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(0x4C, 4), 4);
            ownUuid.TryWriteBytes(bytes.AsSpan(0x188, 16));
            parentUuid.TryWriteBytes(bytes.AsSpan(0x1A8, 16));
            return bytes;
        }
        using var unrelated = new MemoryStream(); VdiImageWriter.Write(unrelated, 65536);
        var files = new Dictionary<string, byte[]>
        {
            [firstPath] = Differential(firstUuid, secondUuid),
            [secondPath] = Differential(secondUuid, firstUuid),
            [unrelatedPath] = unrelated.ToArray()
        };
        string? ResolveReference(string _, DiskImageDependencyReference reference) =>
            reference.Uuid == firstUuid ? firstPath : reference.Uuid == secondUuid ? secondPath : null;
        var index = new DiskImageDependencyIndex(path => new MemoryStream(files[path], writable: false),
            resolveReference: ResolveReference);
        Assert.Throws<InvalidDataException>(() => index.Uses(firstPath, unrelatedPath));

        var descriptor = ImagePath("set/disk.vmdk");
        var extent = ImagePath("set/disk-flat.vmdk");
        using var setIndexImage = new MemoryStream(Encoding.UTF8.GetBytes(
            "# Disk DescriptorFile\nversion=1\nparentCID=ffffffff\nRW 128 FLAT \"disk-flat.vmdk\" 0\n"));
        var setBytes = setIndexImage.ToArray();
        var setIndex = new DiskImageDependencyIndex(
            path => new MemoryStream(path == descriptor ? setBytes : new byte[65536], writable: false),
            path => path == extent ? descriptor : path);
        Assert.True(setIndex.Uses(descriptor, extent));
    }

    [Fact]
    public void QedBackingFlagControlsItsExplicitFileReference()
    {
        using var qed = new MemoryStream(); QedImageWriter.Write(qed, 65536);
        var bytes = qed.ToArray();
        BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(16), 1);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(56), 128);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(60), 8);
        "base.img"u8.CopyTo(bytes.AsSpan(128));
        using var child = new MemoryStream(bytes);
        Assert.Equal([ImagePath("base.img")], DiskImageDependencyReader.Read(child, ImagePath("child.qed")));
    }

    [Fact]
    public void AliasesUseResolvedIdentityWhileRelativeParentsKeepTheirOpeningDirectory()
    {
        var realChild = ImagePath("real/child.vmdk");
        var aliasChild = ImagePath("alias/child.vmdk");
        var realParent = ImagePath("real/base.img");
        var aliasParent = ImagePath("alias/base.img");
        var linkedParent = ImagePath("other/base.img");
        var descriptor = Encoding.UTF8.GetBytes("# Disk DescriptorFile\nversion=1\nparentCID=12345678\nparentFileNameHint=\"base.img\"\nRW 128 ZERO\n");
        string Resolve(string path) => path == aliasChild ? realChild : path == aliasParent ? linkedParent : path;
        var index = new DiskImageDependencyIndex(path => new MemoryStream(
            Resolve(path) == realChild ? descriptor : new byte[65536], writable: false), Resolve);
        Assert.True(index.Uses(aliasChild, realChild));
        Assert.True(index.Uses(aliasChild, linkedParent));
        Assert.False(index.Uses(aliasChild, realParent));
        Assert.True(index.Uses(realChild, realParent));
    }
}
