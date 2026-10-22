using GWGUI.Emulation.Bandai.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Bandai.Common.Machines.Common.Enums;
using GWGUI.Emulation.Bandai.Common.Services;
using GWGUI.Emulation.Bandai.Emulators.BeetleWonderSwan.Factories;
using GWGUI.Emulation.Enums;

namespace GWGUI.Tests.Emulation.Bandai;

public sealed class MonochromePaletteTests
{
    private const string PaletteKey = "wswan_mono_palette";

    [Theory]
    [InlineData(MachineModel.WonderSwan, "wonderswan")]
    [InlineData(MachineModel.WonderSwanColor, "wondeswan_color")]
    [InlineData(MachineModel.SwanCrystal, "swancrystal")]
    public void ModelPaletteIsUsedInBothImageSettingsAndRuntime(MachineModel model, string expected)
    {
        var configuration = new MachineConfiguration(model);
        var adapter = new BeetleWonderSwanMachineFactory();
        var palette = Assert.Single(adapter.DescribeSettings(configuration).SelectMany(block => block.Fields), field => field.Id == PaletteKey);
        Assert.Equal(EmulationMachineTab.Video, palette.Tab);
        Assert.Equal(EmulationVideoSettingsSection.Image, palette.VideoSection);
        Assert.Equal(expected, palette.Value);
        Assert.Equal(expected, adapter.GetRuntimeOptions(configuration)[PaletteKey]);
        Assert.Contains(palette.Choices!, choice => choice.Id == "gb_dmg");
        Assert.Contains(palette.Choices!, choice => choice.Id == "virtual_boy");
        Assert.Empty(configuration.Options);
    }

    [Theory]
    [InlineData(MachineModel.WonderSwan)]
    [InlineData(MachineModel.WonderSwanColor)]
    [InlineData(MachineModel.SwanCrystal)]
    public void ExplicitPaletteKeepsPriorityOverTheModelDefault(MachineModel model)
    {
        var configuration = new MachineConfiguration(model, options: new Dictionary<string, string> { [PaletteKey] = "virtual_boy" });
        var adapter = new BeetleWonderSwanMachineFactory();
        Assert.Equal("virtual_boy", adapter.GetRuntimeOptions(configuration)[PaletteKey]);
        Assert.Equal("virtual_boy", Assert.Single(adapter.DescribeSettings(configuration).SelectMany(block => block.Fields), field => field.Id == PaletteKey).Value);
    }

    [Theory]
    [InlineData(MachineModel.Playdia)]
    [InlineData(MachineModel.TamagotchiP1)]
    public void OtherBandaiMachinesDoNotReceiveWonderSwanPalettes(MachineModel model)
    {
        var configuration = new MachineConfiguration(model);
        var adapter = new Engine().Adapter(configuration);
        Assert.DoesNotContain(PaletteKey, adapter.GetRuntimeOptions(configuration).Keys);
        var settings = (GWGUI.Emulation.Bandai.Common.Interfaces.IEmulatorSettings)adapter;
        Assert.DoesNotContain(settings.DescribeSettings(configuration).SelectMany(block => block.Fields), field => field.Id == PaletteKey);
    }
}
