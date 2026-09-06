using GWGUI.Launcher;
using System.Reflection;
namespace GWGUI.Tests.Application.Launcher;
internal static class LauncherDecisionScenarios
{
    public static void Resolve()
    {
        var root = "virtual"; var language = Path.Combine(root, "Languages", "fr-FR.dll");
        Assert.Equal(language, LauncherPolicy.ResolveAssemblyPath(root, new("gwgui.app.resources") { CultureName = "fr-FR" },
            path => { Assert.Equal(language, path); return true; }, _ => throw new InvalidOperationException(), (_, _) => throw new InvalidOperationException()));
        var library = Path.Combine(root, "lib"); var expected = Path.Combine(library, "nested", "gwgui.domain.dll");
        Assert.Equal(expected, LauncherPolicy.ResolveAssemblyPath(root, new("gwgui.domain"), _ => throw new InvalidOperationException(),
            path => { Assert.Equal(library, path); return true; }, (path, pattern) => { Assert.Equal(library, path); Assert.Equal("gwgui.domain.dll", pattern); return [expected]; }));
        Assert.Null(LauncherPolicy.ResolveAssemblyPath(root, new("absent"), _ => false, _ => false, (_, _) => throw new InvalidOperationException()));
    }
    public static int Entry(string[] args) { Assert.Equal(new[] { "--language", "fr-FR", "virtual path" }, args); return 17; }
    public static void EmptyEntry() { }
    public static void Run()
    {
        var calls = 0;
        Assert.Equal(17, LauncherPolicy.Run("virtual", ["--language", "fr-FR", "virtual path"], path => {
            calls++; Assert.Equal(Path.Combine("virtual", "lib", "gwgui.app.dll"), path); return typeof(LauncherDecisionScenarios).GetMethod(nameof(Entry)); }));
        Assert.Equal(1, calls);
        Assert.Equal(0, LauncherPolicy.Run("virtual", [], _ => typeof(LauncherDecisionScenarios).GetMethod(nameof(EmptyEntry))));
    }
    public static void Failure()
    {
        Assert.Throws<InvalidOperationException>(() => LauncherPolicy.Run("virtual", [], _ => null));
        var error = new FileNotFoundException("synthetic assembly missing");
        Assert.Same(error, Assert.Throws<FileNotFoundException>(() => LauncherPolicy.Run("virtual", [], _ => throw error)));
    }
}
