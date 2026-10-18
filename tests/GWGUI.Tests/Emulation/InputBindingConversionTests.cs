using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation;

public sealed class InputBindingConversionTests
{
    private enum MouseAction : short { Left = 1, Right = 2, None = 17 }
    private enum OtherMouseAction : byte { None, Left }

    [Fact]
    public void KeyConversionKeepsCaseInsensitiveEnumParsingAndOrdinalCommandNames()
    {
        var source = new Dictionary<string, string>
        {
            ["A"] = "a", ["a"] = "B", ["zero"] = "0", ["numeric"] = "2147483647",
            ["prefixed"] = "Keyboard:A", ["empty"] = "", ["invalid"] = "not a key", ["null"] = null!
        };
        var result = source.ToKeys();
        Assert.Equal(4, result.Count);
        Assert.Equal(EmulationKey.A, result["A"]);
        Assert.Equal(EmulationKey.B, result["a"]);
        Assert.Equal(EmulationKey.Unknown, result["zero"]);
        Assert.Equal((EmulationKey)int.MaxValue, result["numeric"]);
        Assert.Same(StringComparer.Ordinal, Assert.IsType<Dictionary<string, EmulationKey>>(result).Comparer);
        source["A"] = "B";
        Assert.Equal(EmulationKey.A, result["A"]);
    }

    [Fact]
    public void KeyStringsCopyValuesAndPreserveOrdinalCommandNames()
    {
        var source = new Dictionary<string, EmulationKey> { ["A"] = EmulationKey.A, ["a"] = (EmulationKey)int.MaxValue };
        var result = EmulationKeyboardBindingFunctions.ToStrings(source);
        Assert.Equal("A", result["A"]);
        Assert.Equal(int.MaxValue.ToString(), result["a"]);
        Assert.Same(StringComparer.Ordinal, Assert.IsType<Dictionary<string, string>>(result).Comparer);
        source.Clear();
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void NullKeyMapsPreserveBothOriginalEmptyComparerPoliciesAndIndependentResults()
    {
        var first = Assert.IsType<Dictionary<string, string>>(EmulationKeyboardBindingFunctions.ToStrings(null));
        var second = Assert.IsType<Dictionary<string, string>>(EmulationKeyboardBindingFunctions.ToStrings(null));
        var ordinal = Assert.IsType<Dictionary<string, string>>(EmulationKeyboardBindingFunctions.ToStrings(null, StringComparer.Ordinal));
        Assert.Same(new Dictionary<string, string>().Comparer, first.Comparer);
        Assert.Same(StringComparer.Ordinal, ordinal.Comparer);
        first.Add("A", "A");
        Assert.Empty(second);
        Assert.Empty(ordinal);
    }

    [Fact]
    public void ForwardMouseBindingsUseTheModuleNoneValueAndKeepUndefinedNumericActions()
    {
        var source = new Dictionary<string, MouseAction>
        {
            ["left input"] = MouseAction.Left, ["none input"] = MouseAction.None, ["zero input"] = 0
        };
        var result = EmulationInputMappingFunctions.MouseBindings(source, MouseAction.None);
        Assert.Equal(2, result.Count);
        Assert.Equal("left input", result["Left"]);
        Assert.Equal("zero input", result["0"]);
        Assert.Same(StringComparer.Ordinal, result.Comparer);
        source.Clear();
        Assert.Equal(2, result.Count);
        var other = EmulationInputMappingFunctions.MouseBindings(
            new Dictionary<string, OtherMouseAction> { ["left"] = OtherMouseAction.Left }, OtherMouseAction.None);
        Assert.Equal("left", other["Left"]);
    }

    [Fact]
    public void NullForwardMouseMapsProduceIndependentEmptyDictionaries()
    {
        var first = EmulationInputMappingFunctions.MouseBindings<MouseAction>(null, MouseAction.None);
        var second = EmulationInputMappingFunctions.MouseBindings<MouseAction>(null, MouseAction.None);
        first.Add("Left", "A");
        Assert.Empty(second);
    }

    [Fact]
    public void TwoPhysicalSourcesForTheSameMouseActionStillThrow()
    {
        var source = new Dictionary<string, MouseAction> { ["A"] = MouseAction.Left, ["B"] = MouseAction.Left };
        Assert.Throws<ArgumentException>(() => EmulationInputMappingFunctions.MouseBindings(source, MouseAction.None));
        Assert.Equal(2, source.Count);
    }

    [Fact]
    public void ReverseMouseMappingsKeepNullDistinctFromAnEmptyConfiguredSet()
    {
        Assert.Null(EmulationInputMappingFunctions.CreateMouseMappings<MouseAction>(null, MouseAction.None));
        var result = EmulationInputMappingFunctions.CreateMouseMappings(Bindings(new Dictionary<string, string>()), MouseAction.None);
        Assert.NotNull(result);
        Assert.Empty(result);
        Assert.Same(StringComparer.OrdinalIgnoreCase, result.Comparer);
    }

    [Fact]
    public void ReverseMouseMappingsPreserveFilteringNumericEnumsAndUntrimmedSources()
    {
        var source = new Dictionary<string, string>
        {
            ["left"] = " Input A ", ["RIGHT"] = "B", ["None"] = "ignored",
            ["invalid"] = "ignored", ["32767"] = "numeric", ["0"] = "zero", ["1"] = "\t"
        };
        var result = EmulationInputMappingFunctions.CreateMouseMappings(Bindings(source), MouseAction.None)!;
        Assert.Equal(4, result.Count);
        Assert.Equal(MouseAction.Left, result[" input a "]);
        Assert.Equal(MouseAction.Right, result["b"]);
        Assert.Equal((MouseAction)short.MaxValue, result["numeric"]);
        Assert.Equal((MouseAction)0, result["zero"]);
        source.Clear();
        Assert.Equal(4, result.Count);
    }

    [Fact]
    public void ReverseMouseSourceCollisionsRemainCaseInsensitiveErrors()
    {
        var source = new Dictionary<string, string> { ["Left"] = "Source", ["Right"] = "source" };
        Assert.Throws<ArgumentException>(() => EmulationInputMappingFunctions.CreateMouseMappings(Bindings(source), MouseAction.None));
        Assert.Equal(2, source.Count);
    }

    [Fact]
    public void FilteredControllerCopiesSkipOnlyWhitespaceAndPreserveOrdinalKeys()
    {
        var source = new Dictionary<string, string>
        {
            ["A"] = "value", ["a"] = " value ", [""] = "ignored", [" "] = "ignored",
            ["empty"] = "", ["blank"] = "\t", ["null"] = null!
        };
        var result = EmulationInputMappingFunctions.CopyNonEmptyBindings(source);
        Assert.Equal(2, result.Count);
        Assert.Equal("value", result["A"]);
        Assert.Equal(" value ", result["a"]);
        Assert.Same(StringComparer.Ordinal, result.Comparer);
        source.Clear();
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void RequiredKeyAndControllerMapsKeepTheirOriginalNullSourceErrors()
    {
        Assert.Equal("source", Assert.Throws<ArgumentNullException>(() => EmulationKeyboardBindingFunctions.ToKeys(null!)).ParamName);
        Assert.Equal("source", Assert.Throws<ArgumentNullException>(() => EmulationInputMappingFunctions.CopyNonEmptyBindings(null!)).ParamName);
    }

    private static EmulationInputBindingSet Bindings(IReadOnlyDictionary<string, string> values) =>
        new([], values, EmulationInputSource.Mouse);
}
