using GWGUI.Emulation;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation;

public sealed class StrictKeyboardBindingResolutionTests
{
    private static readonly InputBindingDefinition[] Definitions =
    [
        new("First", "", "Keyboard:A"),
        new("Second", "", "Keyboard:B")
    ];

    [Fact]
    public void DefaultsAndOverridesAreIndependentOrdinalCopies()
    {
        var input = new Dictionary<string, string> { ["First"] = "Ctrl+Keyboard:C" };
        var defaults = EmulationSdk.InputProcessing.ResolveKeyboardBindings(Definitions, null);
        var result = EmulationSdk.InputProcessing.ResolveKeyboardBindings(Definitions, input);
        Assert.Equal("Keyboard:A", defaults["First"]);
        Assert.Equal("Ctrl+Keyboard:C", result["First"]);
        Assert.Equal("Keyboard:B", result["Second"]);
        Assert.False(result.ContainsKey("first"));
        result["First"] = "Keyboard:D";
        Assert.Equal("Ctrl+Keyboard:C", input["First"]);
        Assert.Equal("Keyboard:A", defaults["First"]);
        Assert.Equal("Ctrl+Keyboard:C", EmulationSdk.InputProcessing.ResolveKeyboardBindings(Definitions, input)["First"]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Ctrl+Keyboard:C")]
    [InlineData("Mouse:Left")]
    [InlineData("Controller:0:Button1")]
    public void ValidStoredValuesArePreservedWithoutFallbackOrNormalization(string? value)
    {
        var input = new Dictionary<string, string> { ["First"] = value! };
        var result = EmulationSdk.InputProcessing.ResolveKeyboardBindings(Definitions, input);
        Assert.Equal(value, result["First"]);
        Assert.Equal(value, input["First"]);
    }

    [Theory]
    [InlineData("Unknown", "Keyboard:A")]
    [InlineData("first", "Keyboard:A")]
    [InlineData("First", "DefinitelyNotAKey")]
    [InlineData("First", "Mouse:Unknown")]
    [InlineData("First", "Controller:")]
    public void InvalidKeysOrSourcesKeepTheOriginalMessageOverload(string key, string value)
    {
        var input = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { [key] = value };
        var error = Assert.Throws<ArgumentException>(() => EmulationSdk.InputProcessing.ResolveKeyboardBindings(Definitions, input));
        Assert.Equal("input", error.Message);
        Assert.Null(error.ParamName);
        Assert.Equal(value, Assert.Single(input).Value);
    }

    [Fact]
    public void DuplicateDefinitionsAreRejectedAndPermissiveResolutionRemainsSeparate()
    {
        InputBindingDefinition[] duplicates = [Definitions[0], Definitions[0]];
        Assert.Throws<ArgumentException>(() => EmulationSdk.InputProcessing.ResolveKeyboardBindings(duplicates, null));
        var input = new Dictionary<string, string> { ["Unknown"] = "Keyboard:C", ["First"] = null! };
        var permissive = EmulationSdk.InputProcessing.ResolveBindings(Definitions, input);
        Assert.Equal("Keyboard:A", permissive["First"]);
        Assert.False(permissive.ContainsKey("Unknown"));
        Assert.Throws<ArgumentException>(() => EmulationSdk.InputProcessing.ResolveKeyboardBindings(Definitions, input));
    }
}
