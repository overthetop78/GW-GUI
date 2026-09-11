using GWGUI.App.Services.Emulation;
using GWGUI.Emulation.HardDisks;
using GWGUI.Emulation.HardDisks.Containers;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class HardDiskDeletionTests
{
    [Fact]
    public void CompleteVmdkSetIsResolvedAndDeletedCollectively()
    {
        using var temporary = new TemporaryDirectory();
        var entryPoint = CreateVmdkSet(temporary.Path);
        var members = HardDiskImageSetResolver.Resolve(entryPoint);
        Assert.Equal(2, members.Count);
        var handles = HardDiskDeletionFile.Open(members);
        HardDiskDeletionFile.Delete(handles, members);
        Assert.All(members, path => Assert.False(File.Exists(path)));
    }

    [Fact]
    public void MissingVmdkMemberStopsBeforeDeletingTheDescriptor()
    {
        using var temporary = new TemporaryDirectory();
        var entryPoint = CreateVmdkSet(temporary.Path);
        var extent = Path.Combine(Path.GetDirectoryName(entryPoint)!, "disk-flat.vmdk");
        File.Delete(extent);
        Assert.Throws<InvalidDataException>(() => HardDiskImageSetResolver.Resolve(entryPoint));
        Assert.True(File.Exists(entryPoint));
    }

    [Fact]
    public void RefusingAfterTheCollectiveLockPreservesEveryMember()
    {
        using var temporary = new TemporaryDirectory();
        var entryPoint = CreateVmdkSet(temporary.Path);
        var members = HardDiskImageSetResolver.Resolve(entryPoint);
        var handles = HardDiskDeletionFile.Open(members);
        foreach (var handle in handles) handle.Dispose();
        Assert.All(members, path => Assert.True(File.Exists(path)));
    }

    [Fact]
    public void CollectiveOpenFailureReleasesEarlierHandlesAndPreservesFiles()
    {
        using var temporary = new TemporaryDirectory();
        var first = Path.Combine(temporary.Path, "first.img");
        File.WriteAllBytes(first, [1, 2, 3]);
        var missing = Path.Combine(temporary.Path, "missing.img");
        Assert.ThrowsAny<Exception>(() => HardDiskDeletionFile.Open([first, missing]));
        using var exclusive = new FileStream(first, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        Assert.Equal(3, exclusive.Length);
    }

    [Fact]
    public void MountedDescriptorProtectsItsSetMemberAsAnActiveDependency()
    {
        using var temporary = new TemporaryDirectory();
        var entryPoint = CreateVmdkSet(temporary.Path);
        var extent = Path.Combine(Path.GetDirectoryName(entryPoint)!, "disk-flat.vmdk");
        var index = new DiskImageDependencyIndex(path => new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));
        Assert.True(index.Uses(entryPoint, extent));
    }

    [Fact]
    public void CompleteSparseBundleRemovesFilesAndItsOwnedDirectories()
    {
        using var temporary = new TemporaryDirectory();
        var bundle = Path.Combine(temporary.Path, "disk.sparsebundle");
        var entryPoint = DiskImageSetPublication.CreateTree(bundle, "Info.plist",
            emit => SparseBundleImageWriter.Write(65536, emit));
        var members = HardDiskImageSetResolver.Resolve(entryPoint);
        var handles = HardDiskDeletionFile.Open(members);
        HardDiskDeletionFile.Delete(handles, members);
        Assert.False(Directory.Exists(bundle));
    }

    private static string CreateVmdkSet(string parent) =>
        DiskImageSetPublication.Create(Path.Combine(parent, "disk-set"), "disk.vmdk",
            emit => VmdkFlatImageSetWriter.Write(65536, "disk", emit));

    private sealed class TemporaryDirectory : IDisposable
    {
        internal TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "gwgui-hdd-delete-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        internal string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path)) Directory.Delete(Path, recursive: true);
        }
    }
}
