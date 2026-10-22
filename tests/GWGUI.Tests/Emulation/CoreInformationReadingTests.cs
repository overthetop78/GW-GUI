using System.Runtime.InteropServices;
using GWGUI.Emulation.Interop;
using GWGUI.Emulation;

namespace GWGUI.Tests.Emulation;

public sealed class CoreInformationReadingTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void CopiesUtf8FieldsAndFlagsBeforeBorrowedMemoryIsReleased(bool fullPath, bool blockExtract)
    {
        var extensions = new HashSet<string> { "bin", "rom" };
        var calls = new List<string>();
        (Profile Profile, string Name, string Version, IReadOnlySet<string> Extensions, bool FullPath, bool BlockExtract) result = default;
        WithInfo("CORE", "version-é", "bin|rom", info =>
        {
            info.NeedFullPath = fullPath;
            info.BlockExtract = blockExtract;
            result = EmulationSdk.NativeResources.ReadInfo(info, Profile.Sample,
                profile => { Assert.Equal(Profile.Sample, profile); calls.Add("expected"); return "core"; },
                pointer => { Assert.Equal(info.ValidExtensions, pointer); calls.Add("extensions"); return extensions; },
                (_, _) => throw new InvalidOperationException("wrong identity"),
                (profile, name, version, parsed, full, blocked) =>
                {
                    calls.Add("result");
                    return (profile, name, version, parsed, full, blocked);
                });
        });

        Assert.Equal(Profile.Sample, result.Profile);
        Assert.Equal("CORE", result.Name);
        Assert.Equal("version-é", result.Version);
        Assert.Same(extensions, result.Extensions);
        Assert.Equal(fullPath, result.FullPath);
        Assert.Equal(blockExtract, result.BlockExtract);
        Assert.Equal(["expected", "extensions", "result"], calls);
    }

    [Fact]
    public void CopiesAMissingVersionAsEmptyAndPassesTheNullExtensionPointer()
    {
        WithInfo("core", null, null, info =>
        {
            var result = EmulationSdk.NativeResources.ReadInfo(info, Profile.Sample,
                static _ => "core",
                pointer => { Assert.Equal(nint.Zero, pointer); return new HashSet<string>(); },
                static (_, _) => throw new InvalidOperationException("wrong identity"),
                static (_, name, version, extensions, _, _) => (name, version, extensions));
            Assert.Equal("core", result.name);
            Assert.Empty(result.version);
            Assert.Empty(result.extensions);
        });
    }

    [Theory]
    [InlineData("wrong", "wrong")]
    [InlineData(null, "")]
    public void RejectsIdentityBeforeParsingExtensionsOrCreatingTheResult(string? nativeName, string actualName)
    {
        var expected = new InvalidOperationException("identity rejected");
        var calls = new List<string>();
        WithInfo(nativeName, "version", "extensions", info =>
        {
            var observed = Assert.Throws<InvalidOperationException>(() =>
            {
                _ = EmulationSdk.NativeResources.ReadInfo<Profile, object>(info, Profile.Sample,
                    _ => { calls.Add("expected"); return "core"; },
                    _ => throw new InvalidOperationException("extensions read too early"),
                    (name, actual) =>
                    {
                        Assert.Equal("core", name);
                        Assert.Equal(actualName, actual);
                        calls.Add("reject");
                        return expected;
                    },
                    static (_, _, _, _, _, _) => throw new InvalidOperationException("result created too early"));
            });
            Assert.Same(expected, observed);
        });
        Assert.Equal(["expected", "reject"], calls);
    }

    [Theory]
    [InlineData("expected")]
    [InlineData("extensions")]
    [InlineData("result")]
    public void PreservesProviderErrorsAndTheirOrdering(string failure)
    {
        var expected = new InvalidOperationException("provider failed");
        var calls = new List<string>();
        void Visit(string step)
        {
            calls.Add(step);
            if (step == failure) throw expected;
        }
        WithInfo("core", "version", "extensions", info =>
        {
            var observed = Assert.Throws<InvalidOperationException>(() =>
            {
                _ = EmulationSdk.NativeResources.ReadInfo<Profile, object>(info, Profile.Sample,
                    _ => { Visit("expected"); return "core"; },
                    _ => { Visit("extensions"); return new HashSet<string>(); },
                    static (_, _) => throw new InvalidOperationException("wrong identity"),
                    (_, _, _, _, _, _) => { Visit("result"); return new object(); });
            });
            Assert.Same(expected, observed);
        });
        var steps = new[] { "expected", "extensions", "result" };
        Assert.Equal(steps.Take(Array.IndexOf(steps, failure) + 1), calls);
    }

    [Theory]
    [InlineData(null, new string[] { })]
    [InlineData("", new string[] { })]
    [InlineData(" \t\u2003", new string[] { })]
    [InlineData(" | || ", new string[] { })]
    [InlineData(".bin|ROM|..Bin| rom | .iso", new[] { "bin", "ROM", "iso" })]
    [InlineData(".", new[] { "" })]
    [InlineData("...|..|.", new[] { "" })]
    [InlineData(". bin| .rom ", new[] { " bin", "rom" })]
    [InlineData(".é|É", new[] { "é" })]
    public void CopiesNormalizedExtensionsAndPreservesOrdinalCaseComparison(string? source, string[] expected)
    {
        HashSet<string>? result = null;
        WithInfo(null, null, source, info => result = EmulationSdk.NativeResources.ParseExtensions(info.ValidExtensions));

        Assert.NotNull(result);
        Assert.Equal(expected.Length, result.Count);
        Assert.True(result.SetEquals(expected));
        Assert.Same(StringComparer.OrdinalIgnoreCase, result.Comparer);
        foreach (var extension in expected)
            Assert.Contains(extension.ToUpperInvariant(), result);
    }

    [Fact]
    public void ReturnsAnIndependentExtensionSetForEachRead()
    {
        HashSet<string>? first = null;
        HashSet<string>? second = null;
        WithInfo(null, null, null, info => first = EmulationSdk.NativeResources.ParseExtensions(info.ValidExtensions));
        WithInfo(null, null, null, info => second = EmulationSdk.NativeResources.ParseExtensions(info.ValidExtensions));

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.NotSame(first, second);
        first.Add("bin");
        Assert.Empty(second);
    }

    private static void WithInfo(string? name, string? version, string? extensions, Action<ExternalCoreApi.SystemInfo> action)
    {
        var info = new ExternalCoreApi.SystemInfo();
        try
        {
            if (name is not null) info.LibraryName = Marshal.StringToCoTaskMemUTF8(name);
            if (version is not null) info.LibraryVersion = Marshal.StringToCoTaskMemUTF8(version);
            if (extensions is not null) info.ValidExtensions = Marshal.StringToCoTaskMemUTF8(extensions);
            action(info);
        }
        finally
        {
            try { if (info.LibraryName != 0) Marshal.FreeCoTaskMem(info.LibraryName); }
            finally
            {
                try { if (info.LibraryVersion != 0) Marshal.FreeCoTaskMem(info.LibraryVersion); }
                finally { if (info.ValidExtensions != 0) Marshal.FreeCoTaskMem(info.ValidExtensions); }
            }
        }
    }

    private enum Profile { Sample }
}
