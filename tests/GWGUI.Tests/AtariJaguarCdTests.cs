using GWGUI.Emulation;
using GWGUI.Emulation.Atari;
using GWGUI.Emulation.Atari.Cores;
using System.IO;

namespace GWGUI.Tests;

[Collection(AtariNativeCoreTestConstants.CollectionName)]
public sealed class AtariJaguarCdTests
{
    private const string CompleteDiscFileName = "game.cue";
    private const string TrackFileName = "track.bin";

    [Fact]
    [Trait("Category", "LocalAssets")]
    public void InstalledCoreReportsExactlyTheSupportedJaguarCdFormats()
    {
        var info = AtariExternalCoreProbe.Inspect(
            Path.Combine(FindRepositoryRoot(), "tmp", "atari-cores", "virtual-jaguar.dll"),
            AtariEmulator.VirtualJaguar);

        Assert.True(AtariJaguarCdFunctions.IsSupported(info.Extensions));
        Assert.All(AtariJaguarCdConstants.CompleteDiscExtensions,
            extension => Assert.Contains(extension, info.Extensions));
    }

    [Fact]
    public void CdSlotIsDisabledOnJaguarAndAvailableOnJaguarCd()
    {
        var jaguar = AtariCompatibilityCatalog.Get(AtariMachineModel.Jaguar).Media.Single(
            item => item.Category == AtariMediaCategory.CompactDisc);
        var jaguarCd = AtariCompatibilityCatalog.Get(AtariMachineModel.JaguarCd).Media.Single(
            item => item.Category == AtariMediaCategory.CompactDisc);

        Assert.Equal(AtariMediaAvailability.Unavailable, jaguar.Availability);
        Assert.Equal(AtariCompatibilityConstants.JaguarStandardNoCdResource,
            jaguar.ExplanationResourceKey);
        Assert.Equal(AtariMediaAvailability.Available, jaguarCd.Availability);
        Assert.Contains(EmulationMediaSlot.Cd0, jaguarCd.Slots);
    }

    [Fact]
    public void CompleteCueImageIsPreparedWhenEveryTrackExists()
    {
        var root = CreateRoot();
        var cue = Path.Combine(root, CompleteDiscFileName);
        var track = Path.Combine(root, TrackFileName);
        File.WriteAllBytes(track, []);
        File.WriteAllText(cue, $"FILE \"{TrackFileName}\" BINARY");
        try
        {
            var prepared = AtariJaguarCdFunctions.Prepare(
                new AtariMachineConfiguration(AtariMachineModel.JaguarCd),
                CompactDisc(cue), false, AtariJaguarCdConstants.CompleteDiscExtensions);

            Assert.Equal(Path.GetFullPath(cue), prepared.RuntimePath);
            Assert.True(prepared.NeedsFullPath);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void CueWithMissingTrackIsRejected()
    {
        var root = CreateRoot();
        var cue = Path.Combine(root, CompleteDiscFileName);
        File.WriteAllText(cue, $"FILE \"{TrackFileName}\" BINARY");
        try
        {
            var exception = Assert.Throws<AtariEmulationException>(() => AtariJaguarCdFunctions.Prepare(
                new AtariMachineConfiguration(AtariMachineModel.JaguarCd), CompactDisc(cue), false,
                AtariJaguarCdConstants.CompleteDiscExtensions));

            Assert.Equal(AtariJaguarCdErrors.MissingCueTrack, exception.Message);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void EmptyCueIsRejected()
    {
        var root = CreateRoot();
        var cue = Path.Combine(root, CompleteDiscFileName);
        File.WriteAllText(cue, string.Empty);
        try
        {
            var exception = Assert.Throws<AtariEmulationException>(() => AtariJaguarCdFunctions.Prepare(
                new AtariMachineConfiguration(AtariMachineModel.JaguarCd), CompactDisc(cue), false,
                AtariJaguarCdConstants.CompleteDiscExtensions));

            Assert.Equal(AtariJaguarCdErrors.EmptyCue, exception.Message);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Theory]
    [InlineData(TrackFileName)]
    [InlineData("disc.iso")]
    public void IndividualTrackAndUnsupportedImageAreRejected(string fileName)
    {
        var root = CreateRoot();
        var path = Path.Combine(root, fileName);
        File.WriteAllBytes(path, []);
        try
        {
            var exception = Assert.Throws<AtariEmulationException>(() => AtariJaguarCdFunctions.Prepare(
                new AtariMachineConfiguration(AtariMachineModel.JaguarCd), CompactDisc(path), false,
                new HashSet<string>(StringComparer.OrdinalIgnoreCase) { Path.GetExtension(path).TrimStart('.') }));
            Assert.Equal(AtariErrorCode.ContentUnsupported, exception.Code);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void StandardJaguarRejectsCdAndJaguarCdOwnsItsBios()
    {
        Assert.Throws<ArgumentException>(() => AtariJaguarCdFunctions.RejectForStandardJaguar(
            AtariMachineModel.Jaguar, CompactDisc(CompleteDiscFileName)));
        AtariJaguarCdFunctions.RejectForStandardJaguar(
            AtariMachineModel.JaguarCd, CompactDisc(CompleteDiscFileName));

        Assert.DoesNotContain(AtariFirmwareCategory.JaguarCdBios,
            AtariCompatibilityCatalog.Get(AtariMachineModel.Jaguar).Firmware);
        Assert.Contains(AtariFirmwareCategory.JaguarCdBios,
            AtariCompatibilityCatalog.Get(AtariMachineModel.JaguarCd).Firmware);
    }

    [Fact]
    [Trait("Category", "LocalAssets")]
    public void CoreReportsThatHotEjectionIsUnavailable()
    {
        using var core = new AtariExternalCore(
            Path.Combine(FindRepositoryRoot(), "tmp", "atari-cores", "virtual-jaguar.dll"),
            AtariEmulator.VirtualJaguar);

        var exception = Assert.Throws<NotSupportedException>(
            () => core.EjectMedia(EmulationMediaSlot.Cd0));

        Assert.Equal(AtariJaguarCdErrors.EjectionUnsupported, exception.Message);
    }

    [Fact]
    public void LockedCdiImageIsRejected()
    {
        var root = CreateRoot();
        var path = Path.Combine(root, "disc.cdi");
        File.WriteAllBytes(path, []);
        try
        {
            using var locked = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

            var exception = Assert.Throws<AtariEmulationException>(() => AtariJaguarCdFunctions.Prepare(
                new AtariMachineConfiguration(AtariMachineModel.JaguarCd), CompactDisc(path), false,
                AtariJaguarCdConstants.CompleteDiscExtensions));

            Assert.Equal(AtariJaguarCdErrors.FileUnreadable, exception.Message);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    private static AtariMediaConfiguration CompactDisc(string path) =>
        new(path, AtariMediaCategory.CompactDisc, EmulationMediaSlot.Cd0, IsReadOnly: true);

    private static string CreateRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "GWGUI-Atari-Jaguar-CD", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null && !File.Exists(Path.Combine(current.FullName, "GWGUI.sln")))
            current = current.Parent;
        return current?.FullName ?? throw new DirectoryNotFoundException("Repository root not found.");
    }
}
