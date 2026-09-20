using System.Reflection;
using GWGUI.App.Views.Windows.Shell;
using GWGUI.MediaEngine.Enums;
using GWGUI.Emulation.Amiga.Modules;
using GWGUI.Emulation.Atari.Modules;
using GWGUI.MediaEngine.Composition;
using GWGUI.MediaEngine.Exploration.Sequential;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaFileSystems.Definitions;
using GWGUI.MediaFileSystems.Exploration;
using GWGUI.MediaAnalysis.Enums;

namespace GWGUI.Tests.Architecture;

public sealed class MediaEngineProjectBoundaryTests
{
    [Fact]
    public void LoadedAssembliesStayWithinMediaLibraryBoundaries()
    {
        AssertGwguiReferences(typeof(MediaKind).Assembly, ["gwgui.mediafilesystems"]);
        AssertGwguiReferences(typeof(MediaContentCategory).Assembly, []);
        AssertGwguiReferences(typeof(MediaVolumeDetectorRegistry).Assembly, []);
        AssertGwguiReferences(typeof(MediaEngineComposition).Assembly, ["gwgui.mediafilesystems"]);
        AssertGwguiReferences(typeof(GWGUI.Infrastructure.Processes.GreaseweazleRunner).Assembly, ["gwgui.mediaengine"]);
        AssertGwguiReferences(typeof(AmigaEmulationModule).Assembly, ["gwgui.emulation", "gwgui.mediaengine"]);
        AssertGwguiReferences(typeof(AtariEmulationModule).Assembly, ["gwgui.emulation", "gwgui.mediaengine"]);

        var appReferences = References(typeof(MainWindow).Assembly);
        Assert.DoesNotContain("gwgui.domain", appReferences);
        Assert.Contains("gwgui.infrastructure", appReferences);
        Assert.Contains("gwgui.mediaengine", appReferences);
        Assert.Contains("gwgui.emulation", appReferences);
        Assert.DoesNotContain("gwgui.mediafilesystems", appReferences);
        Assert.DoesNotContain("gwgui.mediaanalysis", appReferences);

        var readers = FileSystemReaderCatalog.CreateDefault();
        foreach (var id in new[]
        {
            FileSystemIds.AcornAdfs,
            FileSystemIds.AcornDfs,
            FileSystemIds.AmigaDos,
            FileSystemIds.AmigaFlatResourceArchive,
            FileSystemIds.AppleDos,
            FileSystemIds.CommodoreDos,
            FileSystemIds.Fat12
        })
            Assert.IsType<MediaFileSystemsReaderAdapter>(Assert.Single(readers, reader => reader.Id == id));

        var exploration = MediaExplorationComposition.CreateDefault(SequentialMediaComposition.CreateDefault());
        Assert.IsType<SequentialContentDecoderAdapter>(
            Assert.Single(exploration.FileSystems.MediaReaders, reader => reader.Id == FileSystemIds.SequentialContent));
    }

    private static void AssertGwguiReferences(Assembly assembly, IEnumerable<string> expected)
    {
        var actual = References(assembly)
            .Where(name => name.StartsWith("gwgui.", StringComparison.OrdinalIgnoreCase))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        Assert.Equal(expected.Order(StringComparer.OrdinalIgnoreCase), actual.Order(StringComparer.OrdinalIgnoreCase));
    }

    private static IReadOnlySet<string> References(Assembly assembly) => assembly
        .GetReferencedAssemblies()
        .Select(reference => reference.Name ?? string.Empty)
        .Where(name => name.Length > 0)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);
}
