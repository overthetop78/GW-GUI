using System.IO;
using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation;

public sealed class ArchitectureValidationTests
{
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
            EmulationCoreDiagnosticFunctions.RequireArchitecture(actual, "x64", Message);
            Assert.Equal(0, reads);
        }
        else
        {
            var error = Assert.Throws<InvalidDataException>(() =>
                EmulationCoreDiagnosticFunctions.RequireArchitecture(actual, "x64", Message));
            Assert.Equal("architecture refusée", error.Message);
            Assert.Equal(1, reads);
        }
    }

    [Fact]
    public void PreservesTheMessageProviderFailureAfterRejection()
    {
        var expected = new InvalidOperationException("message unavailable");
        string Message() => throw expected;

        EmulationCoreDiagnosticFunctions.RequireArchitecture("x64", "x64", Message);
        Assert.Same(expected, Assert.Throws<InvalidOperationException>(() =>
            EmulationCoreDiagnosticFunctions.RequireArchitecture("unknown", "x64", Message)));
    }
}
