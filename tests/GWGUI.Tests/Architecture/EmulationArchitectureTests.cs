using System.Reflection;
using GWGUI.App.Views.Windows.Shell;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Amiga.Modules;
using GWGUI.Emulation.Atari.Modules;

namespace GWGUI.Tests.Architecture;

public sealed class EmulationArchitectureTests
{
    [Fact]
    public void AppAndEmulationDoNotReferenceFamilyModules()
    {
        var forbidden = new[] { "gwgui.emulation.amiga", "gwgui.emulation.atari", "gwgui.emulation.amstrad" };
        Assert.DoesNotContain(References(typeof(MainWindow).Assembly), forbidden.Contains);
        Assert.DoesNotContain(References(typeof(IEmulationModule).Assembly), forbidden.Contains);
    }

    [Fact]
    public void FamilyModulesExposeTheSameInternalAdapterNames()
    {
        var atari = CommonAdapter(typeof(AtariEmulationModule).Assembly, "GWGUI.Emulation.Atari");
        var amiga = CommonAdapter(typeof(AmigaEmulationModule).Assembly, "GWGUI.Emulation.Amiga");
        Assert.Equal(MemberNames(atari.Adapter), MemberNames(amiga.Adapter));
        Assert.Equal(MemberNames(atari.Context), MemberNames(amiga.Context));
    }

    private static (Type Adapter, Type Context) CommonAdapter(Assembly assembly, string rootNamespace)
    {
        var adapter = assembly.GetType($"{rootNamespace}.Common.Interfaces.IEmulatorAdapter");
        var context = assembly.GetType($"{rootNamespace}.Common.Contracts.EmulatorCreationContext");
        Assert.NotNull(adapter);
        Assert.NotNull(context);
        return (adapter!, context!);
    }

    private static string[] MemberNames(Type type) => type.GetMembers(BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
        .Select(member => member.Name).Order(StringComparer.Ordinal).ToArray();

    private static IReadOnlySet<string> References(Assembly assembly) => assembly.GetReferencedAssemblies()
        .Select(reference => reference.Name?.ToLowerInvariant() ?? string.Empty)
        .ToHashSet(StringComparer.Ordinal);
}
