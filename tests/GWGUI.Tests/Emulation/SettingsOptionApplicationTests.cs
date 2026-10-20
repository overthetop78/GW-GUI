using System.Collections.ObjectModel;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation;

public sealed class SettingsOptionApplicationTests
{
    [Fact]
    public void AppliesNullableChangesWithoutNormalizingValuesOrChangingExcludedKeys()
    {
        var options = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["keep"] = "original", ["replace"] = "old", ["remove"] = "old", ["excluded"] = "original"
        };
        var changes = new Dictionary<string, string?>
        {
            ["replace"] = "new", ["remove"] = null, ["added"] = "value",
            ["empty"] = "", ["spaces"] = "  ", ["excluded"] = null, ["missing"] = null
        };
        var before = changes.ToArray();

        EmulationCoreOptionFunctions.ApplyValues(options, changes, key => key == "excluded");

        Assert.Equal("original", options["keep"]);
        Assert.Equal("new", options["replace"]);
        Assert.False(options.ContainsKey("remove"));
        Assert.Equal("value", options["added"]);
        Assert.Equal("", options["empty"]);
        Assert.Equal("  ", options["spaces"]);
        Assert.Equal("original", options["excluded"]);
        Assert.False(options.ContainsKey("missing"));
        Assert.Equal(before, changes.ToArray());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UsesTheSuppliedDictionaryComparer(bool ignoreCase)
    {
        var comparer = ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
        var options = new Dictionary<string, string>(comparer) { ["Option"] = "old" };

        EmulationCoreOptionFunctions.ApplyValues(options, new Dictionary<string, string?> { ["option"] = "new" });

        Assert.Same(comparer, options.Comparer);
        Assert.Equal(ignoreCase ? 1 : 2, options.Count);
        Assert.Equal(ignoreCase ? "new" : "old", options["Option"]);
        Assert.Equal("new", options["option"]);
    }

    [Fact]
    public void EvaluatesEachExclusionBeforeItsMutationAndKeepsInputOrder()
    {
        var options = new Dictionary<string, string> { ["b"] = "old" };
        var changes = new SortedDictionary<string, string?>(StringComparer.Ordinal)
            { ["a"] = "new", ["b"] = null, ["c"] = "last" };
        var observed = new List<(string Key, bool HasA, bool HasB)>();

        EmulationCoreOptionFunctions.ApplyValues(options, changes, key =>
        {
            observed.Add((key, options.ContainsKey("a"), options.ContainsKey("b")));
            return false;
        });

        Assert.Equal(new[] { ("a", false, true), ("b", true, true), ("c", true, false) }, observed);
        Assert.Equal("last", options["c"]);
    }

    [Fact]
    public void PredicateFailureKeepsPriorChangesAndDoesNotApplyTheFailingOrLaterChange()
    {
        var options = new Dictionary<string, string> { ["b"] = "old" };
        var changes = new SortedDictionary<string, string?>(StringComparer.Ordinal)
            { ["a"] = "new", ["b"] = null, ["c"] = "later" };
        var expected = new InvalidOperationException("predicate failed");
        var visited = new List<string>();

        var actual = Assert.Throws<InvalidOperationException>(() =>
            EmulationCoreOptionFunctions.ApplyValues(options, changes, key =>
            {
                visited.Add(key);
                if (key == "b") throw expected;
                return false;
            }));

        Assert.Same(expected, actual);
        Assert.Equal(new[] { "a", "b" }, visited);
        Assert.Equal("new", options["a"]);
        Assert.Equal("old", options["b"]);
        Assert.False(options.ContainsKey("c"));
    }

    [Fact]
    public void ExcludedChangesNeverWriteToTheDictionary()
    {
        var options = new ReadOnlyDictionary<string, string>(new Dictionary<string, string> { ["keep"] = "old" });

        EmulationCoreOptionFunctions.ApplyValues(options,
            new Dictionary<string, string?> { ["keep"] = null, ["new"] = "value" }, _ => true);

        Assert.Equal("old", Assert.Single(options).Value);
    }

    [Fact]
    public void EmptyChangesDoNotInvokeThePredicate()
    {
        var options = new Dictionary<string, string> { ["keep"] = "old" };

        EmulationCoreOptionFunctions.ApplyValues(options, new Dictionary<string, string?>(),
            _ => throw new InvalidOperationException("unexpected predicate"));

        Assert.Equal("old", Assert.Single(options).Value);
    }

    [Fact]
    public void EditableIdsUseFieldFlagsAndOrdinalIdentityWithoutAddingABlockVisibilityRule()
    {
        EmulationSettingsBlock[] blocks =
        [
            new("one", EmulationMachineTab.General, "title",
            [
                Field("same"), Field("A"), Field("a"),
                Field("disabled") with { IsEnabled = false },
                Field("hidden") with { IsVisible = false },
                Field("information") with { Editor = EmulationSettingsEditor.Information }
            ]),
            new("two", EmulationMachineTab.General, "title",
                [Field("same"), Field("hidden-block-field"), Field("directory-action") with { Editor = EmulationSettingsEditor.OpenDirectory }],
                IsVisible: false)
        ];

        var actual = EmulationCoreOptionFunctions.EditableFieldIds(blocks);

        Assert.Equal(5, actual.Count);
        Assert.True(actual.SetEquals(["same", "A", "a", "hidden-block-field", "directory-action"]));
        Assert.DoesNotContain("SAME", actual);
    }

    [Fact]
    public void EditableIdsEnumerateTheDescriptionOnce()
    {
        var enumerations = 0;
        IEnumerable<EmulationSettingsBlock> Blocks()
        {
            enumerations++;
            yield return new("block", EmulationMachineTab.General, "title", [Field("one")]);
        }

        var actual = EmulationCoreOptionFunctions.EditableFieldIds(Blocks());

        Assert.Equal(1, enumerations);
        Assert.Equal("one", Assert.Single(actual));
    }

    private static EmulationSettingsField Field(string id) =>
        new(id, EmulationMachineTab.General, "block", "label", EmulationSettingsEditor.Text, "value");
}
