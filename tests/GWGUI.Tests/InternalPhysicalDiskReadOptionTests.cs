using GWGUI.App.Services.PhysicalDiskReading;
using GWGUI.App.ViewModels;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.Domain.Settings;

namespace GWGUI.Tests;

public sealed class InternalPhysicalDiskReadOptionTests
{
    [Fact]
    public void TrackParserExpandsCylinderAndHeadRangesInPhysicalOrder()
    {
        var tracks = PhysicalDiskTrackSelectionParser.Parse("c=0-4/2:h=0-1");

        Assert.Equal(
            [(0, 0), (0, 1), (2, 0), (2, 1), (4, 0), (4, 1)],
            tracks.Select(track => (track.Cylinder, track.Head)));
    }

    [Fact]
    public void IndexPeriodParserAcceptsRpmAndDurations()
    {
        Assert.Equal(TimeSpan.FromMilliseconds(200), PhysicalDiskIndexPeriodParser.Parse("300rpm"));
        Assert.Equal(TimeSpan.FromMilliseconds(250), PhysicalDiskIndexPeriodParser.Parse("250ms"));
        Assert.Throws<ArgumentException>(() => PhysicalDiskIndexPeriodParser.Parse("40scp"));
    }

    [Theory]
    [InlineData("DD", ScpDiskType.Other720)]
    [InlineData("HD", ScpDiskType.Other1440)]
    [InlineData("Unknown", ScpDiskType.Other720)]
    public void CaptureTypeUsesConfiguredDensityWithoutClaimingAFileSystem(string density, ScpDiskType expected)
    {
        Assert.Equal(expected, ScpCaptureDiskTypePolicy.Resolve(density));
    }

    [Fact]
    public void InternalReaderChoiceIsStoredInGlobalEngineSettings()
    {
        var settings = new AppSettings();
        settings.Engines.PhysicalRead = OperationEngine.Internal;

        Assert.Equal(OperationEngine.Internal, settings.Engines.PhysicalRead);
        Assert.DoesNotContain(new ReadOperationViewModel().BuildOptions(), option => option.Argument == "--internal-reader");
    }
}
