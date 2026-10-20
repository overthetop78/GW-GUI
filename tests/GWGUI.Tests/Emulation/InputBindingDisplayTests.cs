using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation;

public sealed class InputBindingDisplayTests
{
    [Theory]
    [InlineData("Plain", null, "Plain")]
    [InlineData("Emulation.Controller.Up", null, null)]
    [InlineData("", null, "")]
    [InlineData(".", null, null)]
    [InlineData("Échap", null, "Échap")]
    [InlineData("Plain", "", "")]
    [InlineData("Emulation.Controller.Up", "Override", "Override")]
    [InlineData(null, "Override", "Override")]
    [InlineData(null, "", "")]
    public void ExplicitLabelsShortCircuitResourceFallback(string? resourceKey, string? invariant, string? expected)
    {
        Assert.Equal(expected, EmulationInputMappingFunctions.BindingDisplayValue(resourceKey!, invariant));
    }

    [Fact]
    public void MissingResourceWithoutOverrideKeepsOriginalFailure()
    {
        Assert.Throws<NullReferenceException>(() => EmulationInputMappingFunctions.BindingDisplayValue(null!));
    }
}
