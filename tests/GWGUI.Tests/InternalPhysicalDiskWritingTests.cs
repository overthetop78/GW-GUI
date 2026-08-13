using GWGUI.App.Services.PhysicalDiskWriting;
using GWGUI.App.ViewModels;
using GWGUI.Infrastructure.Hardware.Greaseweazle;
using GWGUI.Domain.Settings;

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
    public void InternalWriterChoiceIsStoredInGlobalEngineSettings()
    {
        var settings = new AppSettings();
        settings.Engines.PhysicalWrite = OperationEngine.Internal;

        Assert.Equal(OperationEngine.Internal, settings.Engines.PhysicalWrite);
        Assert.DoesNotContain(new WriteOperationViewModel().BuildOptions(), option => option.Argument == "--internal-writer");
    }
}
