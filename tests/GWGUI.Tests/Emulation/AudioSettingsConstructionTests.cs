using GWGUI.Emulation;
using System.Globalization;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation;

public sealed class AudioSettingsConstructionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ChoiceLabelCultureDoesNotChangeStoredValuesOrFallback(bool invariantLabels)
    {
        var previous = CultureInfo.CurrentCulture;
        var culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        culture.NumberFormat.NegativeSign = "~";
        try
        {
            CultureInfo.CurrentCulture = culture;
            var field = EmulationSdk.HardwareSettings.AudioSelection(
                new Dictionary<string, string>(), "latency", "audio-block", "latency-label",
                -25, [-25, 0, 25], "ms", invariantLabels ? CultureInfo.InvariantCulture : null);
            Assert.Equal("-25", field.Value);
            Assert.Equal(new[] { "-25", "0", "25" }, field.Choices!.Select(choice => choice.Id));
            Assert.Equal(invariantLabels ? "-25 ms" : "~25 ms", field.Choices![0].InvariantDisplayValue);
            Assert.All(field.Choices!, choice => Assert.Equal(string.Empty, choice.DisplayResourceKey));
            Assert.Equal("latency", field.Id);
            Assert.Equal("audio-block", field.BlockId);
            Assert.Equal("latency-label", field.LabelResourceKey);
            Assert.Equal(EmulationMachineTab.Audio, field.Tab);
            Assert.Equal(EmulationSettingsEditor.Selection, field.Editor);
            Assert.True(field.IsEnabled);
            Assert.True(field.IsVisible);
            Assert.False(field.RequiresRestart);
            Assert.False(field.RefreshSettingsOnChange);
            Assert.Equal(EmulationSettingsChoiceSource.Declared, field.ChoiceSource);
            Assert.Null(field.ExplanationResourceKey);
            Assert.Null(field.DetailedExplanationResourceKey);
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }

    [Theory]
    [InlineData("")]
    [InlineData("37")]
    [InlineData("-10")]
    [InlineData("saved-custom-value")]
    public void SavedValuesArePreservedWithoutNewClampingOrValidation(string saved)
    {
        var field = EmulationSdk.HardwareSettings.AudioSelection(
            new Dictionary<string, string> { ["volume"] = saved }, "volume", "audio", "volume-label",
            100, [0, 50, 100], "%", CultureInfo.InvariantCulture);
        Assert.Equal(saved, field.Value);
    }

    [Theory]
    [InlineData("ms", "20 ms")]
    [InlineData("%", "20 %")]
    [InlineData("", "20 ")]
    public void UnitsKeepTheirExistingSpacing(string unit, string expected)
    {
        var field = EmulationSdk.HardwareSettings.AudioSelection(
            new Dictionary<string, string>(), "value", "audio", "label", 20, [20], unit, CultureInfo.InvariantCulture);
        Assert.Equal(expected, Assert.Single(field.Choices!).InvariantDisplayValue);
    }

    [Fact]
    public void ChoicesKeepOrderDuplicatesAndFreshAllocation()
    {
        var values = new List<int> { 30, 20, 30 };
        var options = new Dictionary<string, string>();
        var first = EmulationSdk.HardwareSettings.AudioSelection(options, "id", "audio", "label", 20, values, "ms", null);
        var second = EmulationSdk.HardwareSettings.AudioSelection(options, "id", "audio", "label", 20, values, "ms", null);
        Assert.NotSame(first.Choices, second.Choices);
        Assert.NotSame(first.Choices![0], second.Choices![0]);
        values[0] = 999;
        Assert.Equal(new[] { "30", "20", "30" }, first.Choices.Select(choice => choice.Id));
        Assert.Equal(new[] { "30", "20", "30" }, second.Choices.Select(choice => choice.Id));
    }
}