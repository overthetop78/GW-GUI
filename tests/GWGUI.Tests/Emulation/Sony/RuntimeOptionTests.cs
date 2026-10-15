using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Sony.Common.Contracts;
using GWGUI.Emulation.Sony.Common.Dictionaries;
using GWGUI.Emulation.Sony.Common.Interfaces;
using GWGUI.Emulation.Sony.Common.Machines.Common.Constants;
using GWGUI.Emulation.Sony.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;

namespace GWGUI.Tests.Emulation.Sony;

public sealed class RuntimeOptionTests
{
    [Fact]
    public void CurrentSelectionsRemainSeparateFromNativeDefaultsAndSurviveUpdates()
    {
        var renderer = new CoreOption("renderer", "Renderer", null, null, "vulkan", "vulkan", [new("vulkan", "Vulkan"), new("software", "Software")]);
        var unknown = new CoreOption("future", "Future", null, null, "default", "native", []);
        var selected = new Dictionary<string, string> { ["renderer"] = "software", ["notAnnounced"] = "ignored" };
        var applied = CoreOptionMetadataFunctions.WithCurrentValues([renderer, unknown], selected);
        Assert.Equal("software", applied[0].CurrentValue);
        Assert.Equal("vulkan", applied[0].DefaultValue);
        Assert.Same(renderer.Values, applied[0].Values);
        Assert.Equal("vulkan", renderer.CurrentValue);
        Assert.Same(unknown, applied[1]);
        selected["renderer"] = "vulkan";
        var updated = CoreOptionMetadataFunctions.WithCurrentValues(applied, selected);
        Assert.Equal("vulkan", updated[0].CurrentValue);
        Assert.Equal("software", applied[0].CurrentValue);
        Assert.Same(unknown, updated[1]);
        Assert.Same(renderer, Assert.Single(CoreOptionMetadataFunctions.WithCurrentValues([renderer], new Dictionary<string, string>())));
    }

    [Theory]
    [InlineData("pcsx2", ModelConstants.PlayStation2, "pcsx2_axis_deadzone1", "12", "12%")]
    [InlineData("mednafen_psx", ModelConstants.PlayStation, "beetle_psx_cpu_freq_scale", "115", "120%")]
    public void RuntimeChangesUseTheSameAdapterConversionAsStartup(string core, string model, string key, string value, string expected)
    {
        var adapter = Assert.IsAssignableFrom<IEmulatorSettingsAdapter>(EmulatorCatalog.CreateAdapters().Single(item => item.EmulatorId == core));
        var configuration = new MachineConfiguration(model, core, Options: new Dictionary<string, string> { [key] = value });
        Assert.Equal(expected, adapter.GetRuntimeOptions(configuration)[key]);
        Assert.Equal(value, configuration.Options![key]);
    }

    [Fact]
    public void NativeValuesAndDefaultsSurvivePresentationMetadata()
    {
        var native = new CoreOption("test", "Native name", "Native explanation", "video", "future", "future",
            [new("enabled", "Native enabled"), new("future", "Future native mode")]);
        var description = new CoreOption("test", "Emulation.Option.Test.Label", "Emulation.Option.Test.Help", null, "enabled", "enabled",
            [new("enabled", "Emulation.Option.Sony.Value.Enabled")], IsVisible: false, Tab: EmulationMachineTab.Video, RequiresRestart: true);
        var merged = Assert.Single(CoreOptionMetadataFunctions.Merge([native], [description]));
        Assert.Equal(native.DefaultValue, merged.DefaultValue);
        Assert.Equal(native.CurrentValue, merged.CurrentValue);
        Assert.Equal(native.Values.Select(value => value.Value), merged.Values.Select(value => value.Value));
        Assert.Equal(description.Name, merged.Name);
        Assert.Equal(description.Description, merged.Description);
        Assert.StartsWith("Emulation.", merged.Category!);
        Assert.True(merged.RequiresRestart);
        Assert.False(merged.IsVisible);
        Assert.Equal(description.Values[0].Label, merged.Values[0].Label);
        Assert.Equal(native.Values[1].Label, merged.Values[1].Label);
    }

    [Fact]
    public void UnknownNativeOptionsArePreserved()
    {
        var native = new CoreOption("future", "Future option", null, null, "new", "new", [new("new", "New mode")]);
        Assert.Same(native, Assert.Single(CoreOptionMetadataFunctions.Merge([native], [])));
    }
}
