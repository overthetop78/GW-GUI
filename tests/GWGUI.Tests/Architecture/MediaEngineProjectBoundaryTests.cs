using System.Reflection;
using GWGUI.App.Views.Windows.Shell;
using GWGUI.Domain.Enums;
using GWGUI.Emulation.Amiga.Modules;
using GWGUI.Emulation.Atari.Modules;
using GWGUI.MediaEngine.Composition;

namespace GWGUI.Tests.Architecture;

public sealed class MediaEngineProjectBoundaryTests
{
    [Fact]
    public void ProjectReferencesFollowTheMediaEngineDependencyDirection()
    {
        AssertGwguiReferences(typeof(MediaKind).Assembly, []);
        AssertGwguiReferences(typeof(MediaEngineComposition).Assembly, ["gwgui.domain"]);
        AssertGwguiReferences(typeof(GWGUI.Infrastructure.Processes.GreaseweazleRunner).Assembly, ["gwgui.domain"]);
        AssertGwguiReferences(typeof(AmigaEmulationModule).Assembly, ["gwgui.emulation", "gwgui.mediaengine"]);
        AssertGwguiReferences(typeof(AtariEmulationModule).Assembly, ["gwgui.emulation", "gwgui.mediaengine"]);

        var appReferences = References(typeof(MainWindow).Assembly);
        Assert.Contains("gwgui.domain", appReferences);
        Assert.Contains("gwgui.infrastructure", appReferences);
        Assert.Contains("gwgui.mediaengine", appReferences);
        Assert.Contains("gwgui.emulation", appReferences);
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
