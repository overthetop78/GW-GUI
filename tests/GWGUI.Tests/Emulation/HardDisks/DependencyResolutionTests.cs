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
    public void HashBasedParentsAreExplicitlyUnresolvedInsteadOfTreatedAsIndependent()
    {
        using var chd = new MemoryStream(); ChdImageWriter.Write(chd, 65536);
        chd.Position = 104; chd.WriteByte(1);
        Assert.Throws<NotSupportedException>(() => DiskImageDependencyReader.Read(chd, ImagePath("child.chd")));
        using var vdi = new MemoryStream(); VdiImageWriter.Write(vdi, 65536);
        vdi.Position = 76; vdi.Write(new byte[] { 4, 0, 0, 0 });
        Assert.Throws<NotSupportedException>(() => DiskImageDependencyReader.Read(vdi, ImagePath("child.vdi")));
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
