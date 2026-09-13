using GWGUI.Emulation.HardDisks;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class ConsumerProfileTests
{
    private static DiskImagePlan Plan(string container = "raw", string table = "mbr") =>
        new(16L << 20, container, table, [new(1L << 20, 4L << 20)]);

    [Fact]
    public void SupportedPairingsDoNotAuthorizeUnlistedCombinations()
    {
        var consumer = new DiskConsumerProfile("test-storage",
            [new("raw", "mbr", ["none"]), new("vhd", "gpt", ["none"])], 32L << 20);
        DiskImageBuilder.Validate(Plan(), consumer: consumer);
        DiskImageBuilder.Validate(Plan("vhd", "gpt"), consumer: consumer);
        using var image = new MemoryStream();
        Assert.Throws<NotSupportedException>(() => DiskImageBuilder.Write(image, Plan("raw", "gpt"), consumer: consumer));
        Assert.Equal(0, image.Length);
        Assert.Throws<NotSupportedException>(() => DiskImageBuilder.Validate(Plan("vhd", "mbr"), consumer: consumer));
    }

    [Fact]
    public void ConsumerDiskAndVolumeLimitsRemainIndependent()
    {
        var consumer = new DiskConsumerProfile("limited-storage", [new("raw", "mbr", ["none"])],
            32L << 20, minimumBytes: 8L << 20, capacityAlignment: 1L << 20,
            maximumVolumeBytes: 4L << 20, maximumVolumes: 1);
        DiskImageBuilder.Validate(Plan(), consumer: consumer);
        Assert.Throws<ArgumentOutOfRangeException>(() => DiskImageBuilder.Validate(Plan() with { CapacityBytes = 64L << 20 }, consumer: consumer));
        Assert.Throws<ArgumentOutOfRangeException>(() => DiskImageBuilder.Validate(Plan() with { CapacityBytes = (16L << 20) + 512 }, consumer: consumer));
        Assert.Throws<ArgumentException>(() => DiskImageBuilder.Validate(Plan() with { Volumes = [new(1L << 20, 5L << 20)] }, consumer: consumer));
        Assert.Throws<ArgumentException>(() => DiskImageBuilder.Validate(Plan() with
        { Volumes = [new(1L << 20, 2L << 20), new(4L << 20, 2L << 20)] }, consumer: consumer));
    }

    [Fact]
    public void ConsumerCannotBroadenContainerOrFilesystemSupport()
    {
        var consumer = new DiskConsumerProfile("large-storage", [new("vhd", "none", ["none"])], 64L << 40);
        Assert.Throws<ArgumentOutOfRangeException>(() => DiskImageBuilder.Validate(
            new(4L << 40, "vhd", "none", []), consumer: consumer));
        consumer = new DiskConsumerProfile("format-storage", [new("raw", "none", ["not-implemented"])], 64L << 20);
        Assert.Throws<NotSupportedException>(() => DiskImageBuilder.Validate(
            new(16L << 20, "raw", "none", [new(0, 16L << 20, "not-implemented")]), consumer: consumer));
    }

    [Fact]
    public void SectorRestrictionsApplyEvenWhenTheFormatterSupportsMore()
    {
        var consumer = new DiskConsumerProfile("sector-storage", [new("raw", "none", ["fat16"], [512])], 64L << 20);
        var plan = new DiskImagePlan(32L << 20, "raw", "none", [new(0, 32L << 20, "fat16", SectorBytes: 4096)]);
        DiskImageBuilder.Validate(plan);
        Assert.Throws<NotSupportedException>(() => DiskImageBuilder.Validate(plan, consumer: consumer));
    }

    [Fact]
    public void SpecificRestrictionsRunBeforeAnyOutputAndCannotAlterTheVolumeList()
    {
        var calls = 0;
        var consumer = new DiskConsumerProfile("geometry-storage", [new("raw", "mbr", ["none"])], 32L << 20,
            validateSpecific: plan =>
            {
                calls++;
                Assert.True(((IList<DiskVolumePlan>)plan.Volumes).IsReadOnly);
                throw new ArgumentException("Simulated unsupported controller geometry");
            });
        using var image = new MemoryStream();
        Assert.Throws<ArgumentException>(() => DiskImageBuilder.Write(image, Plan(), consumer: consumer));
        Assert.Equal(1, calls);
        Assert.Equal(0, image.Length);
    }

    [Fact]
    public void CallerCollectionsCannotChangeAnAlreadyDeclaredProfile()
    {
        var formats = new HashSet<string> { "none" };
        int[] sectors = [512];
        DiskLayoutSupport[] layouts = [new("raw", "mbr", formats, sectors)];
        var consumer = new DiskConsumerProfile("immutable-storage", layouts, 32L << 20);
        formats.Clear(); sectors[0] = 4096; layouts[0] = new("vhd", "gpt", []);
        DiskImageBuilder.Validate(Plan(), consumer: consumer);
    }
}
