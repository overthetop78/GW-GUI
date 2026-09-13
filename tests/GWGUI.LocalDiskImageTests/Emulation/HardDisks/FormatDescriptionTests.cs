using GWGUI.Emulation.HardDisks;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class FormatDescriptionTests
{
    [Fact]
    public void EveryBuiltInHasAnIdentityButAdvertisesOnlyImplementedConstruction()
    {
        var registry = DiskFormatRegistry.CreateDefault();
        Assert.All(registry.Containers, value =>
        {
            Assert.NotNull(value.Identity);
            Assert.Equal(DiskFormatOperations.Create, value.Operations);
        });
        Assert.All(registry.FileSystems, value =>
        {
            Assert.NotNull(value.Identity);
            Assert.Equal(DiskFormatOperations.Create, value.Operations);
            Assert.Empty(value.Identity.Extensions);
        });
        Assert.All(registry.PartitionTables, value =>
        {
            Assert.NotNull(value.Identity);
            Assert.Equal(DiskFormatOperations.Create, value.Operations);
            Assert.Empty(value.Identity.Extensions);
        });
    }

    [Fact]
    public void SharedExtensionsReturnAllCandidatesRatherThanGuessingAContainer()
    {
        var registry = DiskFormatRegistry.CreateDefault();
        Assert.Equal(["raw", "vhd"], registry.FindContainersByExtension(".VHD").Select(value => value.Id));
        Assert.Equal(["qcow2", "qcow2-v2"], registry.FindContainersByExtension(".qcow2").Select(value => value.Id));
        Assert.Equal(["udif", "udif-zlib"], registry.FindContainersByExtension(".dmg").Select(value => value.Id));
        Assert.Empty(registry.FindContainersByExtension(".unknown"));
        var qcow = registry.Containers.Where(value => value.Identity!.SignatureFamily == "qcow").ToArray();
        Assert.Equal(3, qcow.Length);
        Assert.Equal(3, qcow.Select(value => value.Identity!.Variant).Distinct().Count());
    }

    [Fact]
    public void ExtensionMetadataIsImmutableAndRegistryExtensionsStayLocal()
    {
        string[] extensions = [".testdisk", ".TESTDISK"];
        var identity = new DiskFormatIdentity("Test format", "Version 1", extensions);
        extensions[0] = ".changed";
        Assert.Equal([".testdisk"], identity.Extensions);
        var registry = DiskFormatRegistry.CreateDefault();
        registry.Register(new DiskFormatRegistry.Container("test-disk", (_, _) => { }, (_, _, _, _) => { })
        { Identity = identity });
        Assert.Single(registry.FindContainersByExtension(".testdisk"));
        Assert.Empty(DiskFormatRegistry.CreateDefault().FindContainersByExtension(".testdisk"));
        Assert.Throws<ArgumentException>(() => new DiskFormatIdentity("Test", "V1", ["../image"]));
        Assert.Throws<ArgumentException>(() => new DiskFormatIdentity("Test", "V1", [".img:stream"]));
    }
}
