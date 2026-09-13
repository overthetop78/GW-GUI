using GWGUI.Emulation.HardDisks;
using GWGUI.Emulation.HardDisks.Containers;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class ImageSetPublicationTests
{
    private static string Target => Path.GetFullPath("simulated-image-set");

    [Fact]
    public void CompleteSetIsPublishedOnlyAfterAllMembersHaveBeenProduced()
    {
        var storage = new MemoryStorage();
        var result = DiskImageSetPublication.Create(Target, "disk.vmdk", emit =>
        {
            VmdkFlatImageSetWriter.Write(65536, "disk", (name, source) =>
            {
                emit(name, source);
                Assert.Null(storage.Published);
            });
        }, storage);
        Assert.Equal(Path.Combine(Target, "disk.vmdk"), result);
        Assert.NotNull(storage.Published);
        Assert.Equal(["disk-flat.vmdk", "disk.vmdk"], storage.Published.Keys);
        Assert.True(storage.Disposed);
        Assert.Equal(65536, storage.Published["disk-flat.vmdk"].Length);
    }

    [Fact]
    public void WriteFailureRemovesOnlyTheStagedSet()
    {
        var storage = new MemoryStorage { FailMember = 2 };
        Assert.Throws<IOException>(() => DiskImageSetPublication.Create(Target, "disk.vmdk", emit =>
        {
            emit("data.bin", new MemoryStream([1, 2, 3]));
            emit("disk.vmdk", new MemoryStream([4, 5]));
        }, storage));
        Assert.Null(storage.Published);
        Assert.Empty(storage.Staged);
        Assert.True(storage.Disposed);
    }

    [Fact]
    public void ProducerCannotHideAnEmissionFailureAndPublishAnIncompleteSet()
    {
        var storage = new MemoryStorage { FailMember = 2 };
        Assert.Throws<IOException>(() => DiskImageSetPublication.Create(Target, "disk.vmdk", emit =>
        {
            emit("disk.vmdk", new MemoryStream([1]));
            try { emit("data.bin", new MemoryStream([2])); } catch (IOException) { }
        }, storage));
        Assert.Null(storage.Published);
        Assert.Empty(storage.Staged);
    }

    [Fact]
    public void DestinationCreatedDuringConstructionIsPreserved()
    {
        var storage = new MemoryStorage { CollisionAtCommit = true };
        Assert.Throws<IOException>(() => DiskImageSetPublication.Create(Target, "disk.vmdk",
            emit => emit("disk.vmdk", new MemoryStream([1])), storage));
        Assert.Equal(new byte[] { 99 }, storage.Published!["existing.bin"]);
        Assert.Single(storage.Published);
        Assert.Empty(storage.Staged);
    }

    [Theory]
    [InlineData("../outside.bin")] [InlineData("folder/data.bin")] [InlineData("image:stream")]
    [InlineData("CON.vmdk")] [InlineData("COM¹.bin")] [InlineData("data.")] [InlineData("data ")]
    public void InvalidMemberNamesDoNotReachStorage(string name)
    {
        var storage = new MemoryStorage();
        Assert.Throws<ArgumentException>(() => DiskImageSetPublication.Create(Target, "disk.vmdk",
            emit => emit(name, new MemoryStream([1])), storage));
        Assert.Equal(0, storage.Writes);
        Assert.Null(storage.Published);
    }

    [Fact]
    public void MissingEntryAndCaseInsensitiveDuplicatesAbortPublication()
    {
        var storage = new MemoryStorage();
        Assert.Throws<InvalidDataException>(() => DiskImageSetPublication.Create(Target, "disk.vmdk",
            emit => emit("data.bin", new MemoryStream([1])), storage));
        Assert.Empty(storage.Staged);
        storage = new MemoryStorage();
        Assert.Throws<ArgumentException>(() => DiskImageSetPublication.Create(Target, "disk.vmdk", emit =>
        {
            emit("disk.vmdk", new MemoryStream([1])); emit("DISK.VMDK", new MemoryStream([2]));
        }, storage));
        Assert.Empty(storage.Staged);
    }

    [Fact]
    public void DeferredEmissionCannotChangeAPublishedSet()
    {
        var storage = new MemoryStorage();
        Action<string, Stream>? deferred = null;
        var result = DiskImageSetPublication.Create(Target, "disk.vmdk", emit =>
        {
            deferred = emit; emit("DISK.VMDK", new MemoryStream([1]));
        }, storage);
        Assert.Equal(Path.Combine(Target, "DISK.VMDK"), result);
        Assert.Throws<InvalidOperationException>(() => deferred!("late.bin", new MemoryStream([2])));
        Assert.Single(storage.Published!);
    }

    [Fact]
    public void CleanupFailureDoesNotConcealTheCreationFailure()
    {
        var storage = new MemoryStorage { FailMember = 1, FailCleanup = true };
        var failure = Assert.Throws<AggregateException>(() => DiskImageSetPublication.Create(Target, "disk.vmdk",
            emit => emit("disk.vmdk", new MemoryStream([1])), storage));
        Assert.Equal(2, failure.InnerExceptions.Count);
        Assert.All(failure.InnerExceptions, error => Assert.IsType<IOException>(error));
    }

    [Fact]
    public void BundleSubdirectoriesArePublishedTogetherWithTheirMetadata()
    {
        var storage = new MemoryStorage();
        DiskImageSetPublication.Create(Target, "Info.plist", emit =>
            SparseBundleImageWriter.Write(512, emit), storage, allowSubdirectories: true);
        Assert.Equal(4, storage.Published!.Count);
        Assert.Contains("bands/0", storage.Published.Keys);
    }

    [Theory]
    [InlineData("bands/../outside")] [InlineData("/bands/0")] [InlineData("bands//0")]
    [InlineData("bands/CON")] [InlineData("bands\\0")] [InlineData("bands/0:stream")]
    public void NestedPathsCannotEscapeTheBundleOrIdentifyDevices(string name)
    {
        var storage = new MemoryStorage();
        Assert.ThrowsAny<ArgumentException>(() => DiskImageSetPublication.Create(Target, "Info.plist",
            emit => emit(name, new MemoryStream()), storage, allowSubdirectories: true));
        Assert.Equal(0, storage.Writes);
    }

    [Theory]
    [InlineData("bands", "bands/0")] [InlineData("bands/0", "BANDS")]
    public void FileAndDirectoryCollisionsAbortTheWholeBundle(string first, string second)
    {
        var storage = new MemoryStorage();
        Assert.Throws<ArgumentException>(() => DiskImageSetPublication.Create(Target, "Info.plist", emit =>
        {
            emit(first, new MemoryStream()); emit(second, new MemoryStream());
        }, storage, allowSubdirectories: true));
        Assert.Empty(storage.Staged); Assert.Null(storage.Published);
    }

    private sealed class MemoryStorage : IImageSetStorage, IImageSetTransaction
    {
        public Dictionary<string, byte[]> Staged { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, byte[]>? Published { get; private set; }
        public int FailMember { get; init; }
        public bool CollisionAtCommit { get; init; }
        public bool FailCleanup { get; init; }
        public int Writes { get; private set; }
        public bool Disposed { get; private set; }
        private bool committed;

        public IImageSetTransaction Begin(string targetDirectory)
        {
            Assert.Equal(Target, targetDirectory);
            return this;
        }
        public void WriteNew(string name, Stream source)
        {
            Writes++;
            if (Writes == FailMember) throw new IOException("Simulated member write failure");
            using var copy = new MemoryStream();
            if (source.CanSeek) source.Position = 0;
            source.CopyTo(copy); Staged.Add(name, copy.ToArray());
        }
        public void Commit()
        {
            if (CollisionAtCommit)
            {
                Published = new() { ["existing.bin"] = [99] };
                throw new IOException("Simulated destination collision");
            }
            Published = new(Staged);
            committed = true;
        }
        public void Dispose()
        {
            Disposed = true;
            if (FailCleanup) throw new IOException("Simulated cleanup failure");
            if (!committed) Staged.Clear();
        }
    }
}
