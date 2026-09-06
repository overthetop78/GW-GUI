using System.Reflection;
using DiscUtils.Fat;
using DiscUtils.Streams;
using GWGUI.Emulation.HardDisks;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class ImageSetCompositionTests
{
    [Theory]
    [InlineData("sparsebundle")]
    [InlineData("vmdk-vmfs-flat")] [InlineData("vmdk-vmfs-sparse")]
    [InlineData("vmdk-split-flat")] [InlineData("vmdk-split-sparse")] [InlineData("vmdk-monolithic-flat")]
    public void EverySetUsesTheCommonPartitionAndFilesystemPlan(string container)
    {
        var members = new Dictionary<string, byte[]>();
        var plan = new DiskImagePlan(8L << 20, container, "mbr", [new(1L << 20, 4L << 20, "fat12", "DATA")]);
        var consumer = new DiskConsumerProfile("storage", [new(container, "mbr", ["fat12"])], 8L << 20);
        DiskImageBuilder.WriteSet(plan, (name, source) =>
        {
            using var copy = new MemoryStream(); source.CopyTo(copy); members.Add(name, copy.ToArray());
        }, consumer: consumer);
        if (container == "sparsebundle")
        {
            using var raw = SparseBundleTests.Reassemble(members); Check(raw);
        }
        else
        {
            using var layer = (DiscUtils.Vmdk.DiskImageFile)Activator.CreateInstance(typeof(DiscUtils.Vmdk.DiskImageFile),
                BindingFlags.Instance | BindingFlags.NonPublic, null,
                [new VmdkImageSetTests.MemoryLocator(members), "disk.vmdk", FileAccess.Read], null)!;
            using var logical = layer.OpenContent(null!, Ownership.None); Check(logical);
        }
        static void Check(Stream logical)
        {
            logical.Position = 510; Assert.Equal(0x55, logical.ReadByte()); Assert.Equal(0xaa, logical.ReadByte());
            using var volume = new SubStream(logical, Ownership.None, 1L << 20, 4L << 20);
            using var fs = new FatFileSystem(volume); Assert.Equal("DATA", fs.VolumeLabel.Trim());
        }
    }

    [Fact]
    public void ConsumerAndSectorRestrictionsRunBeforeAnyMemberIsEmitted()
    {
        var count = 0;
        void Emit(string name, Stream source) => count++;
        var plan = new DiskImagePlan(8L << 20, "sparsebundle", "none", []);
        var consumer = new DiskConsumerProfile("storage", [new("raw", "none", ["none"])], 8L << 20);
        Assert.Throws<NotSupportedException>(() => DiskImageBuilder.WriteSet(plan, Emit, consumer: consumer));
        Assert.Throws<ArgumentException>(() => DiskImageBuilder.WriteSet(plan with { PartitionTableId = "gpt-4096" }, Emit));
        Assert.Throws<ArgumentException>(() => DiskImageBuilder.WriteSet(plan with
            { Volumes = [new(0, 4L << 20), new(512, 4L << 20)] }, Emit));
        Assert.Throws<NotSupportedException>(() => DiskImageBuilder.WriteSet(plan with { FixedSize = true }, Emit));
        Assert.Equal(0, count);
    }

    [Fact]
    public void SetAndSingleFileIdentifiersCannotShadowEachOther()
    {
        var registry = DiskFormatRegistry.CreateDefault();
        Assert.Equal(6, registry.ImageSets.Count);
        Assert.All(registry.ImageSets, set => Assert.Equal(DiskFormatOperations.Create, set.Operations));
        Assert.Throws<ArgumentException>(() => registry.Register(new DiskFormatRegistry.Container("sparsebundle", (_, _) => { }, (_, _, _, _) => { })));
        Assert.Throws<ArgumentException>(() => registry.Register(registry.ImageSets[0] with { Id = "raw" }));
    }
}
