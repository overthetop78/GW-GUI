using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Functions;
using SharpOptions = GWGUI.Emulation.Sharp.Common.Contracts;
using SharpModel = GWGUI.Emulation.Sharp.Common.Machines.Common.Enums.MachineModel;
using SharpConfiguration = GWGUI.Emulation.Sharp.Common.Machines.Common.Contracts.MachineConfiguration;
using SharpSettings = GWGUI.Emulation.Sharp.Common.Machines.Common.Functions.SettingsDescriptionFunctions;
using SinclairOptions = GWGUI.Emulation.Sinclair.Common.Contracts;
using SinclairModel = GWGUI.Emulation.Sinclair.Common.Machines.Common.Enums.MachineModel;
using SinclairConfiguration = GWGUI.Emulation.Sinclair.Common.Machines.Common.Contracts.MachineConfiguration;
using SinclairSettings = GWGUI.Emulation.Sinclair.Common.Machines.Common.Functions.SettingsDescriptionFunctions;

namespace GWGUI.Tests.Emulation;

public sealed class NativeOptionFieldTests
{
    [Theory]
    [InlineData(false, EmulationSettingsEditor.Toggle)]
    [InlineData(true, EmulationSettingsEditor.Selection)]
    public void SharpKeepsConfiguredNativeValuesAndChoiceMetadata(bool thirdChoice, EmulationSettingsEditor editor)
    {
        var initial = new SharpConfiguration(SharpModel.X68000);
        var declared = GWGUI.Emulation.Sharp.Common.Machines.Common.Functions.CoreOptionValidationFunctions
            .ForConfiguration(initial).First(option => option.Values.Count >= 2);
        var onValue = declared.Values[0].Value;
        var offValue = declared.Values[1].Value;
        var values = new List<EmulationResourceOptionValue>
        {
            new(onValue, "On", "Emulation.Value.Enabled", "YES"),
            new(offValue, "Off", "Emulation.Value.Disabled", "NO")
        };
        if (thirdChoice) values.Add(new("automatic", "Automatic", "Visual.Automatic"));
        var option = new EmulationResourceRestartOption(declared.Key, "Mode", null, "Video", offValue, "ignored",
            values, ResourceKey: "Native.Mode", ExplanationResourceKey: "Native.Mode.Help");
        var configuration = new SharpConfiguration(SharpModel.X68000,
            options: new Dictionary<string, string> { [declared.Key] = onValue });
        var field = SharpSettings.Native(configuration, option, EmulationMachineTab.Video, "video");
        Assert.Equal(editor, field.Editor);
        Assert.Equal(declared.Key, field.Id);
        Assert.Equal("video", field.BlockId);
        Assert.Equal("Native.Mode", field.LabelResourceKey);
        Assert.Equal(onValue, field.Value);
        Assert.Equal(onValue, field.EnabledValue);
        Assert.Equal(offValue, field.DisabledValue);
        Assert.True(field.IsEnabled);
        Assert.False(field.RequiresRestart);
        Assert.Equal("Native.Mode.Help", field.ExplanationResourceKey);
        Assert.Collection(field.Choices!.Take(2),
            on => Assert.Equal(new EmulationSettingsChoice(onValue, "Emulation.Value.Enabled", "YES"), on),
            off => Assert.Equal(new EmulationSettingsChoice(offValue, "Emulation.Value.Disabled", "NO"), off));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SinclairKeepsEnabledStateAndFallsBackToTheDeclaredDefault(bool enabled)
    {
        var option = new SinclairOptions.CoreOption("mode", "Mode", null, "Video", "native-off", "ignored",
            [new("native-on", "On", "Emulation.Value.Enabled"), new("native-off", "Off", "Emulation.Value.Disabled")],
            IsEnabled: enabled, ResourceKey: "Native.Mode", ExplanationResourceKey: "Native.Help");
        var field = SinclairSettings.Native(new SinclairConfiguration(SinclairModel.Spectrum48K), option,
            EmulationMachineTab.Video, "video");
        Assert.Equal(EmulationSettingsEditor.Toggle, field.Editor);
        Assert.Equal(enabled, field.IsEnabled);
        Assert.Equal("native-off", field.Value);
        Assert.Equal("native-on", field.EnabledValue);
        Assert.Equal("native-off", field.DisabledValue);
        Assert.Equal("Native.Help", field.ExplanationResourceKey);
    }

    [Fact]
    public void MissingResourceFailsBeforeAnyChoiceOrConfigurationRead()
    {
        var expected = new InvalidOperationException("missing resource");
        var touched = false;
        var actual = Assert.Throws<InvalidOperationException>(() =>
            EmulationCoreOptionFunctions.CreateSettingsField("mode", EmulationMachineTab.Video, "video",
                () => throw expected, new[] { "on", "off" },
                value => { touched = true; return new(value, value); },
                value => { touched = true; return true; },
                value => { touched = true; return true; },
                value => value,
                () => { touched = true; return "off"; }, () => "help"));
        Assert.Same(expected, actual);
        Assert.False(touched);
    }

    [Fact]
    public void ChoiceFailureWinsBeforeReadingTheConfiguredValue()
    {
        var expected = new InvalidOperationException("invalid choice resource");
        var readConfiguration = false;
        var actual = Assert.Throws<InvalidOperationException>(() =>
            EmulationCoreOptionFunctions.CreateSettingsField("mode", EmulationMachineTab.Video, "video",
                () => "Native.Mode", new[] { "on", "off" },
                value => throw expected, value => value == "on", value => value == "off", value => value,
                () => { readConfiguration = true; throw new InvalidOperationException("configuration read"); },
                () => "help"));
        Assert.Same(expected, actual);
        Assert.False(readConfiguration);
    }

    [Fact]
    public void ExistingFieldPathKeepsNativeValueReadsAndConstructorCallbackOrder()
    {
        var events = new List<string>();
        var field = EmulationCoreOptionFunctions.CreateSettingsField("mode", EmulationMachineTab.Video, "video",
            () => { events.Add("resource"); return "Native.Mode"; }, new[] { "on", "off" },
            value => { events.Add("choice " + value); return new(value, value); },
            value => { events.Add("enabled " + value); return value == "on"; },
            value => { events.Add("disabled " + value); return value == "off"; },
            value => { events.Add("native " + value); return value.ToUpperInvariant(); },
            () => { events.Add("configured"); return "OFF"; },
            () => { events.Add("help"); return "Native.Help"; },
            () => { events.Add("active"); return false; });
        Assert.Equal(new[] { "resource", "choice on", "choice off", "enabled on", "native on",
            "disabled on", "disabled off", "native off", "configured", "active", "help" }, events);
        Assert.Equal("ON", field.EnabledValue);
        Assert.Equal("OFF", field.DisabledValue);
        Assert.False(field.IsEnabled);
    }

    [Fact]
    public void CustomTogglePreservesLateMissingColecoValueFailure()
    {
        var events = new List<string>();
        var values = new[] { "enabled", "enabled" };
        Assert.Throws<InvalidOperationException>(() =>
            EmulationCoreOptionFunctions.CreateSettingsFieldWithToggle("mode", EmulationMachineTab.Video, "video",
                () => { events.Add("resource"); return "Native.Mode"; }, values,
                value => { events.Add("choice " + value); return new(value, value); },
                () => { events.Add("toggle"); return true; },
                () => { events.Add("configured"); return "enabled"; },
                () => { events.Add("help"); return "Native.Help"; },
                _ => { events.Add("enabled"); return values.First(value => value == "enabled"); },
                _ => { events.Add("disabled"); return values.First(value => value == "disabled"); }));
        Assert.Equal(new[] { "resource", "choice enabled", "choice enabled", "toggle", "configured", "help",
            "enabled", "disabled" }, events);
    }
}

