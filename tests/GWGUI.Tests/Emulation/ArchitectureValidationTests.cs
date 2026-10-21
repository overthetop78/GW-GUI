using GWGUI.Emulation;
using System.IO;
using System.Reflection;
using GWGUI.Emulation.Interfaces;

namespace GWGUI.Tests.Emulation;

public sealed class ArchitectureValidationTests
{
    [Theory]
    [InlineData(nameof(EmulationSdk.AudioProcessing), typeof(IEmulationAudioProcessing))]
    [InlineData(nameof(EmulationSdk.SessionDirectories), typeof(IEmulationSessionDirectories))]
    public void SharedProcessingApisExposePublicInterfacesAndInternalImplementations(string serviceName, Type contract)
    {
        var entry = typeof(EmulationSdk).GetProperty(serviceName)!;
        Assert.Equal(contract, entry.PropertyType);
        Assert.True(contract.IsInterface);
        Assert.True(contract.IsPublic);
        Assert.False(entry.GetValue(null)!.GetType().IsVisible);
    }

    [Fact]
    public void ReleaseApiExposesAnInterfaceAndKeepsItsImplementationInternal()
    {
        var entry = typeof(EmulationSdk)
            .GetProperty(nameof(EmulationSdk.CoreReleases))!;
        Assert.Equal(typeof(IEmulationCoreReleases), entry.PropertyType);
        Assert.True(entry.PropertyType.IsPublic);
        var implementation = EmulationSdk.CoreReleases.GetType();
        Assert.False(implementation.IsVisible);
        Assert.Null(implementation.GetMethod("ReadJsonAsync", BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance));
    }

    [Fact]
    public void DiagnosticApiExposesAnInterfaceAndKeepsItsImplementationInternal()
    {
        var entry = typeof(EmulationSdk)
            .GetProperty(nameof(EmulationSdk.CoreDiagnostics))!;
        Assert.Equal(typeof(IEmulationCoreDiagnostics), entry.PropertyType);
        Assert.True(entry.PropertyType.IsPublic);
        var implementation = EmulationSdk.CoreDiagnostics.GetType();
        Assert.False(implementation.IsVisible);
        foreach (var name in new[] { "ReadArchitecture", "ReadDeclaredVersion", "ReadExports" })
            Assert.Null(implementation.GetMethod(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance));
    }

    [Theory]
    [InlineData("x64", true)]
    [InlineData("X64", false)]
    [InlineData("x64 ", false)]
    [InlineData("arm64", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void ReadsTheErrorOnlyOnceAndOnlyAfterOrdinalRejection(string? actual, bool accepted)
    {
        var reads = 0;
        string Message()
        {
            reads++;
            return "architecture refusée";
        }

        if (accepted)
        {
            EmulationSdk.CoreDiagnostics.RequireArchitecture(actual, "x64", Message);
            Assert.Equal(0, reads);
        }
        else
        {
            var error = Assert.Throws<InvalidDataException>(() =>
                EmulationSdk.CoreDiagnostics.RequireArchitecture(actual, "x64", Message));
            Assert.Equal("architecture refusée", error.Message);
            Assert.Equal(1, reads);
        }
    }

    [Fact]
    public void PreservesTheMessageProviderFailureAfterRejection()
    {
        var expected = new InvalidOperationException("message unavailable");
        string Message() => throw expected;

        EmulationSdk.CoreDiagnostics.RequireArchitecture("x64", "x64", Message);
        Assert.Same(expected, Assert.Throws<InvalidOperationException>(() =>
            EmulationSdk.CoreDiagnostics.RequireArchitecture("unknown", "x64", Message)));
    }
}
