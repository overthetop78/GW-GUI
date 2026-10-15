using System.Reflection;

namespace GWGUI.Tests.Emulation.EmulationContracts;

public sealed class NativeCleanupTests
{
    private static readonly (string Family, string Adapter)[] Modules =
    [
        ("Acorn", "Common"), ("Apple", "Common"), ("Atari", "Common"), ("Bandai", "Common"),
        ("Coleco", "Common"), ("Epoch", "Common"), ("Fairchild", "Common"), ("Mattel", "Common"),
        ("Philips", "Common"), ("Sharp", "Common"), ("Sinclair", "Common"), ("Snk", "Common"),
        ("Thomson", "Theodore"), ("Watara", "Potator"), ("Wenquxing", "Wqxemu"), ("Panasonic", "Opera")
    ];

    public static IEnumerable<object[]> CleanupCases()
    {
        foreach (var (family, adapter) in Modules)
        {
            foreach (var failure in new[] { "none", "unload", "deinitialize", "callbacks", "library" })
                yield return [family, adapter, true, true, failure];
            yield return [family, adapter, false, true, "none"];
            yield return [family, adapter, false, false, "none"];
            yield return [family, adapter, false, true, "deinitialize"];
            yield return [family, adapter, false, false, "callbacks"];
        }
    }

    [Theory]
    [MemberData(nameof(CleanupCases))]
    public void CleanupAttemptsEveryStageAfterEarlierFailure(string family, string adapter, bool loaded, bool initialized, string failure)
    {
        var assembly = Assembly.Load($"gwgui.emulation.{family.ToLowerInvariant()}");
        var prefix = $"GWGUI.Emulation.{family}.Emulators.{adapter}.Interop";
        var exportsType = assembly.GetType(prefix + ".Contracts.ExternalCoreExports", throwOnError: true)!;
        var cleanupType = assembly.GetType(prefix + ".Functions.CoreLifecycleFunctions", throwOnError: true)!;
        var cleanup = cleanupType.GetMethod("Cleanup", BindingFlags.NonPublic | BindingFlags.Static)!;
        var calls = new List<string>();
        var injected = new InvalidOperationException("Injected cleanup failure");

        void Step(string name)
        {
            calls.Add(name);
            if (failure == name) throw injected;
        }

        Action unload = () => Step("unload");
        Action deinitialize = () => Step("deinitialize");
        var constructor = exportsType.GetConstructors().Single(ctor => ctor.GetParameters().Length > 2);
        var arguments = constructor.GetParameters().Select(parameter =>
        {
            var action = parameter.Name switch { "UnloadGame" => unload, "Deinitialize" => deinitialize, _ => null };
            return action is null ? null : (object)Delegate.CreateDelegate(parameter.ParameterType, action.Target, action.Method);
        }).ToArray();
        var exports = constructor.Invoke(arguments);
        Action callbacks = () => Step("callbacks");
        Action library = () => Step("library");
        var expected = new List<string>();
        if (loaded) expected.Add("unload");
        if (initialized) expected.Add("deinitialize");
        expected.AddRange(["callbacks", "library"]);

        if (failure == "none") cleanup.Invoke(null, [exports, loaded, initialized, callbacks, library]);
        else
        {
            var error = Assert.Throws<TargetInvocationException>(() => cleanup.Invoke(null, [exports, loaded, initialized, callbacks, library]));
            Assert.Same(injected, error.InnerException);
        }
        Assert.Equal(expected, calls);
    }
}
