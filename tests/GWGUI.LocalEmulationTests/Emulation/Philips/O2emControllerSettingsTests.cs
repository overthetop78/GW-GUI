using System.Net.Http;
using System.Text.Json;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Philips.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Philips.Common.Machines.Common.Enums;
using GWGUI.Emulation.Philips.Emulators.O2em.Functions;
using GWGUI.Emulation.Philips.Modules;

namespace GWGUI.Tests.Emulation.Philips;

public sealed class O2emControllerSettingsTests
{
    [Theory]
    [InlineData(MachineModel.Odyssey2, "enabled")]
    [InlineData(MachineModel.Odyssey2, "disabled")]
    [InlineData(MachineModel.VideopacG7000, "enabled")]
    [InlineData(MachineModel.VideopacG7000, "disabled")]
    public void SwappingPortsIsAVisibleGlobalControllerSettingAndReachesTheCore(MachineModel model, string value)
    {
        using var http = new HttpClient();
        var module = new PhilipsEmulationModule("unused", "unused", http, "unused");
        var initial = new MachineConfiguration(model);
        var described = module.Describe(initial.MachineId, initial);
        var field = Assert.Single(described.Blocks.SelectMany(block => block.Fields), field => field.Id == "o2em_swap_gamepads");

        Assert.Equal(EmulationMachineTab.Controllers, field.Tab);
        Assert.Equal(EmulationSettingsEditor.Toggle, field.Editor);
        Assert.True(field.IsVisible);
        Assert.True(field.IsEnabled);
        Assert.True(field.IsGlobalControllerOption);
        Assert.Equal("disabled", field.Value);
        Assert.Equal("enabled", field.EnabledValue);
        Assert.Equal("disabled", field.DisabledValue);
        Assert.Equal("Emulation.Philips.Setting.SwapGamepads", field.LabelResourceKey);

        var changed = Assert.IsType<MachineConfiguration>(module.ApplySettings(initial,
            new Dictionary<string, string?> { [field.Id] = value }));
        var restored = JsonSerializer.Deserialize<MachineConfiguration>(JsonSerializer.Serialize(changed))!;

        Assert.Equal(value, O2emRuntimeFunctions.Options(restored)[field.Id]);
        Assert.Equal(value, Assert.Single(module.Describe(restored.MachineId, restored).Blocks.SelectMany(block => block.Fields), item => item.Id == field.Id).Value);
        Assert.Equal(initial.Input, changed.Input);
        Assert.Empty(initial.Options);
    }
}
