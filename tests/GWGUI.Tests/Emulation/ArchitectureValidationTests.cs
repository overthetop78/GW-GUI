using System.IO;
using System.Reflection;
using GWGUI.Emulation.Interfaces;

namespace GWGUI.Tests.Emulation;

public sealed class ArchitectureValidationTests
{
    [Fact]
    public void DiagnosticApiExposesAnInterfaceAndKeepsItsImplementationInternal()
    {
        var entry = typeof(global::GWGUI.Emulation.EmulationSdk)
            .GetProperty(nameof(global::GWGUI.Emulation.EmulationSdk.CoreDiagnostics))!;
        Assert.Equal(typeof(IEmulationCoreDiagnostics), entry.PropertyType);
        Assert.True(entry.PropertyType.IsPublic);
        var implementation = global::GWGUI.Emulation.EmulationSdk.CoreDiagnostics.GetType();
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
            global::GWGUI.Emulation.EmulationSdk.CoreDiagnostics.RequireArchitecture(actual, "x64", Message);
            Assert.Equal(0, reads);
        }
        else
        {
            var error = Assert.Throws<InvalidDataException>(() =>
                global::GWGUI.Emulation.EmulationSdk.CoreDiagnostics.RequireArchitecture(actual, "x64", Message));
            Assert.Equal("architecture refusée", error.Message);
            Assert.Equal(1, reads);
        }
    }

    [Fact]
    public void PreservesTheMessageProviderFailureAfterRejection()
    {
        var expected = new InvalidOperationException("message unavailable");
        string Message() => throw expected;

        global::GWGUI.Emulation.EmulationSdk.CoreDiagnostics.RequireArchitecture("x64", "x64", Message);
        Assert.Same(expected, Assert.Throws<InvalidOperationException>(() =>
            global::GWGUI.Emulation.EmulationSdk.CoreDiagnostics.RequireArchitecture("unknown", "x64", Message)));
    }
}
