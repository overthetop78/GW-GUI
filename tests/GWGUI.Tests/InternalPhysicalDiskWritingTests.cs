using GWGUI.App.Services.PhysicalDiskWriting;
using GWGUI.App.ViewModels;
using GWGUI.Infrastructure.Hardware.Greaseweazle;

namespace GWGUI.Tests;

public sealed class InternalPhysicalDiskWritingTests
{
    [Theory]
    [InlineData("A", GreaseweazleBusType.IbmPc, 0)]
    [InlineData("B", GreaseweazleBusType.IbmPc, 1)]
    [InlineData("0", GreaseweazleBusType.Shugart, 0)]
    [InlineData("1", GreaseweazleBusType.Shugart, 1)]
    public void DriveSelectionMapsConfiguredNamesToProtocolAddresses(
        string selection,
        GreaseweazleBusType expectedBus,
        byte expectedUnit)
    {
        var result = GreaseweazleDriveSelectionPolicy.Resolve(selection);

        Assert.Equal(expectedBus, result.BusType);
        Assert.Equal(expectedUnit, result.Unit);
    }

    [Fact]
    public void InternalWriterPersistsInProfilesWithoutLeakingIntoGwArguments()
    {
        var model = new WriteOperationViewModel();
        model.InternalWriter.Enabled = true;

        Assert.Contains("internal-writer", model.CaptureEnabledOptions());
        Assert.DoesNotContain(model.BuildOptions(), option => option.Argument == "--internal-writer");

        var restored = new WriteOperationViewModel();
        restored.ApplyOptions(model.CaptureEnabledOptions(), model.CaptureValues());

        Assert.True(restored.InternalWriter.Enabled);
        Assert.Empty(restored.BuildOptions());
    }
}
