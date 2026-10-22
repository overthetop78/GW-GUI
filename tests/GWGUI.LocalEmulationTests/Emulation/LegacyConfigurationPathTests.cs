using GWGUI.Emulation;
using System.IO;
using System.Reflection;
using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation;

public sealed class LegacyConfigurationPathTests
{
    private const string BasePath = @"C:\Audit";

    public static IEnumerable<object[]> Stores()
    {
        yield return [typeof(GWGUI.Emulation.Amstrad.Common.Services.ConfigurationStore)];
        yield return [typeof(GWGUI.Emulation.Commodore.Common.Services.ConfigurationStore)];
        yield return [typeof(GWGUI.Emulation.Microsoft.Common.Services.ConfigurationStore)];
        yield return [typeof(GWGUI.Emulation.Nec.Common.Services.ConfigurationStore)];
        yield return [typeof(GWGUI.Emulation.Nintendo.Common.Services.ConfigurationStore)];
        yield return [typeof(GWGUI.Emulation.Sega.Common.Services.ConfigurationStore)];
        yield return [typeof(GWGUI.Emulation.Sony.Common.Services.ConfigurationStore)];
    }

    [Theory]
    [MemberData(nameof(Stores))]
    public void BlankHistoricalPathsRemainNull(Type type)
    {
        var store = Create(type);
        foreach (var path in new string?[] { null, "", " ", "\t\r\n" })
        {
            Assert.Null(Invoke(store, "StorePath", path));
            Assert.Null(Invoke(store, "ResolvePath", path));
        }
    }

    [Theory]
    [MemberData(nameof(Stores))]
    public void StoredPathsRespectTheBaseBoundaryAndCanonicalSeparator(Type type)
    {
        var store = Create(type);
        foreach (var (path, expected) in new[]
        {
            (@"C:\Audit\Media\Disk.adf", "Media/Disk.adf"),
            (@"C:\Audit", "."),
            (@"C:\Audit\Media\..\Disk.adf", "Disk.adf"),
            (@"C:\Audit2\Disk.adf", @"C:\Audit2\Disk.adf"),
            (@"C:\", @"C:\"),
            (@"C:\Disk.adf", @"C:\Disk.adf"),
            (@"D:\Disk.adf", @"D:\Disk.adf"),
            (@"\\server\share\Disk.adf", @"\\server\share\Disk.adf")
        })
            Assert.Equal(expected, Invoke(store, "StorePath", path));
    }

    [Theory]
    [MemberData(nameof(Stores))]
    public void ResolutionRetainsAbsolutePathsAndResolvesRelativePaths(Type type)
    {
        var store = Create(type);
        foreach (var (path, expected) in new[]
        {
            ("Media/Disk.adf", @"C:\Audit\Media\Disk.adf"),
            (@"Media\Disk.adf", @"C:\Audit\Media\Disk.adf"),
            (".", @"C:\Audit"),
            ("../Disk.adf", @"C:\Disk.adf"),
            ("./Media/../Disk.adf", @"C:\Audit\Disk.adf"),
            ("C:/Else/Disk.adf", "C:/Else/Disk.adf"),
            (@"D:\Disk.adf", @"D:\Disk.adf"),
            (@"\\server\share\Disk.adf", @"\\server\share\Disk.adf")
        })
            Assert.Equal(expected, Invoke(store, "ResolvePath", path));
    }

    [Theory]
    [MemberData(nameof(Stores))]
    public void InvalidNonblankPathsKeepTheirException(Type type)
    {
        var store = Create(type);
        foreach (var operation in new[] { "StorePath", "ResolvePath" })
        {
            var error = Assert.Throws<TargetInvocationException>(() => Invoke(store, operation, "bad\0path"));
            Assert.IsAssignableFrom<ArgumentException>(error.InnerException);
        }
    }

    [Fact]
    public void NativeSdkWhitespacePolicyIsUnchanged()
    {
        Assert.Equal(" ", EmulationSdk.ConfigurationFiles.StorePath(" ", BasePath));
        Assert.Equal(" ", EmulationSdk.ConfigurationFiles.ResolvePath(" ", BasePath));
    }

    private static object Create(Type type)
        => Activator.CreateInstance(type, [Path.GetFullPath(BasePath), BasePath])!;

    private static string? Invoke(object store, string operation, string? path)
        => (string?)store.GetType().GetMethod(operation, BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(store, [path]);
}
