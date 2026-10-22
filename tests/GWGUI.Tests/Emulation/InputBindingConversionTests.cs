using GWGUI.Emulation;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation;

public sealed class InputBindingConversionTests
{
    private enum MouseAction : short { Left = 1, Right = 2, None = 17 }
    private enum OtherMouseAction : byte { None, Left }

    [Fact]
    public void MissingAndEmptyMappingsDoNotInvokeTheSourcePolicy()
    {
        var ids = new HashSet<string>(StringComparer.Ordinal) { "Fire" };
        Assert.False(EmulationSdk.InputProcessing.HasInvalidBindings(null, ids, _ => throw new InvalidOperationException()));
        Assert.False(EmulationSdk.InputProcessing.HasInvalidBindings(new Dictionary<string, string>(), ids,
            _ => throw new InvalidOperationException()));
    }

    [Fact]
    public void UnknownCommandsStopBeforeSourceValidation()
    {
        var values = new Dictionary<string, string> { ["unknown"] = "Keyboard:A" };
        Assert.True(EmulationSdk.InputProcessing.HasInvalidBindings(values, new HashSet<string> { "Fire" },
            _ => throw new InvalidOperationException()));
    }

    [Fact]
    public void MappingValidationPreservesOrderAndStopsAtTheFirstRejectedSource()
    {
        var values = new Dictionary<string, string> { ["Fire"] = "first", ["Jump"] = "rejected", ["After"] = "unreached" };
        var visited = new List<string>();
        Assert.True(EmulationSdk.InputProcessing.HasInvalidBindings(values, values.Keys.ToHashSet(StringComparer.Ordinal), source =>
        {
            visited.Add(source);
            return source != "rejected";
        }));
        Assert.Equal(new[] { "first", "rejected" }, visited);
        Assert.Equal(3, values.Count);
    }

    [Fact]
    public void MappingValidationKeepsTheCommandSetComparerAndPropagatesPolicyErrors()
    {
        var values = new Dictionary<string, string> { ["fire"] = "Keyboard:A" };
        Assert.True(EmulationSdk.InputProcessing.HasInvalidBindings(values,
            new HashSet<string>(StringComparer.Ordinal) { "Fire" }, EmulationKeyboardBindingFunctions.IsValid));
        Assert.False(EmulationSdk.InputProcessing.HasInvalidBindings(values,
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Fire" }, EmulationKeyboardBindingFunctions.IsValid));
        var failure = new InvalidOperationException("source policy");
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => EmulationSdk.InputProcessing.HasInvalidBindings(values,
            new HashSet<string> { "fire" }, _ => throw failure)));
    }

    [Fact]
    public void NullSourcesKeepTheirDistinctSyntaxAndWataraConfigurationPolicies()
    {
        var values = new Dictionary<string, string> { ["Fire"] = null! };
        var ids = new HashSet<string> { "Fire" };
        Assert.False(EmulationSdk.InputProcessing.HasInvalidBindings(values, ids, EmulationKeyboardBindingFunctions.IsValid));
        Assert.True(EmulationSdk.InputProcessing.HasInvalidBindings(values, ids, static source => source is not null));
    }

    [Theory]
    [InlineData(null, true, true)]
    [InlineData("", true, true)]
    [InlineData(" ", false, true)]
    [InlineData("Keyboard:DefinitelyNotAKey", true, false)]
    [InlineData("Mouse:X+", true, false)]
    [InlineData("LeftControl", false, true)]
    [InlineData("controller:", true, false)]
    [InlineData("mouse:left", true, true)]
    [InlineData("unexpected", false, false)]
    public void PrefixOnlyAdaptersKeepTheirOriginalPolicyDistinctFromFullSyntax(string? source, bool prefix, bool syntax)
    {
        Assert.Equal(prefix, EmulationKeyboardBindingFunctions.IsValidSourcePrefix(source));
        Assert.Equal(syntax, EmulationKeyboardBindingFunctions.IsValid(source));
    }

    [Fact]
    public void DefinitionDefaultsProduceIndependentOrdinalResults()
    {
        InputBindingDefinition[] definitions = [new("A", "unused", "Keyboard:A"), new("a", "unused", "Keyboard:B")];
        var first = EmulationSdk.InputProcessing.ResolveBindings(definitions, null);
        var second = EmulationSdk.InputProcessing.ResolveBindings(definitions, null);

        Assert.Equal("Keyboard:A", first["A"]);
        Assert.Equal("Keyboard:B", first["a"]);
        Assert.Same(StringComparer.Ordinal, first.Comparer);
        first.Clear();
        Assert.Equal(2, second.Count);
    }

    [Fact]
    public void ConfiguredDefinitionsKeepEmptySourcesAndFallbackOnlyForMissingOrNullValues()
    {
        InputBindingDefinition[] definitions =
        [
            new("A", "unused", "default-A"), new("a", "unused", "default-a"),
            new("null", "unused", "default-null"), new("missing", "unused", "default-missing"),
            new("blank", "unused", "default-blank")
        ];
        var values = new Dictionary<string, string>
        {
            ["A"] = "Keyboard:Z", ["a"] = "", ["null"] = null!, ["blank"] = " ", ["unknown"] = "ignored"
        };
        var result = EmulationSdk.InputProcessing.ResolveBindings(definitions, values);

        Assert.Equal(5, result.Count);
        Assert.Equal("Keyboard:Z", result["A"]);
        Assert.Equal("", result["a"]);
        Assert.Equal("default-null", result["null"]);
        Assert.Equal("default-missing", result["missing"]);
        Assert.Equal(" ", result["blank"]);
        Assert.False(result.ContainsKey("unknown"));
        values.Clear();
        Assert.Equal("Keyboard:Z", result["A"]);
    }

    [Fact]
    public void DefinitionLookupUsesTheConfiguredComparerWhileResultIdsRemainOrdinal()
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["UP"] = "" };
        var result = EmulationSdk.InputProcessing.ResolveBindings([new("Up", "unused", "Keyboard:A")], values);

        Assert.Equal("", result["Up"]);
        Assert.False(result.ContainsKey("UP"));
        Assert.Same(StringComparer.Ordinal, result.Comparer);
    }

    [Fact]
    public void DuplicateDefinitionIdsRemainErrors()
    {
        InputBindingDefinition[] definitions = [new("A", "unused", "first"), new("A", "unused", "second")];
        Assert.Throws<ArgumentException>(() => EmulationSdk.InputProcessing.ResolveBindings(definitions, null));
    }

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
        var result = EmulationSdk.InputProcessing.MouseBindings(source, MouseAction.None);
        Assert.Equal(2, result.Count);
        Assert.Equal("left input", result["Left"]);
        Assert.Equal("zero input", result["0"]);
        Assert.Same(StringComparer.Ordinal, result.Comparer);
        source.Clear();
        Assert.Equal(2, result.Count);
        var other = EmulationSdk.InputProcessing.MouseBindings(
            new Dictionary<string, OtherMouseAction> { ["left"] = OtherMouseAction.Left }, OtherMouseAction.None);
        Assert.Equal("left", other["Left"]);
    }

    [Fact]
    public void NullForwardMouseMapsProduceIndependentEmptyDictionaries()
    {
        var first = EmulationSdk.InputProcessing.MouseBindings<MouseAction>(null, MouseAction.None);
        var second = EmulationSdk.InputProcessing.MouseBindings<MouseAction>(null, MouseAction.None);
        first.Add("Left", "A");
        Assert.Empty(second);
    }

    [Fact]
    public void TwoPhysicalSourcesForTheSameMouseActionStillThrow()
    {
        var source = new Dictionary<string, MouseAction> { ["A"] = MouseAction.Left, ["B"] = MouseAction.Left };
        Assert.Throws<ArgumentException>(() => EmulationSdk.InputProcessing.MouseBindings(source, MouseAction.None));
        Assert.Equal(2, source.Count);
    }

    [Fact]
    public void ReverseMouseMappingsKeepNullDistinctFromAnEmptyConfiguredSet()
    {
        Assert.Null(EmulationSdk.InputProcessing.CreateMouseMappings<MouseAction>(null, MouseAction.None));
        var result = EmulationSdk.InputProcessing.CreateMouseMappings(Bindings(new Dictionary<string, string>()), MouseAction.None);
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
        var result = EmulationSdk.InputProcessing.CreateMouseMappings(Bindings(source), MouseAction.None)!;
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
        Assert.Throws<ArgumentException>(() => EmulationSdk.InputProcessing.CreateMouseMappings(Bindings(source), MouseAction.None));
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
        var result = EmulationSdk.InputProcessing.CopyNonEmptyBindings(source);
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
        Assert.Equal("source", Assert.Throws<ArgumentNullException>(() => EmulationSdk.InputProcessing.CopyNonEmptyBindings(null!)).ParamName);
    }

    private static EmulationInputBindingSet Bindings(IReadOnlyDictionary<string, string> values) =>
        new([], values, EmulationInputSource.Mouse);
}
