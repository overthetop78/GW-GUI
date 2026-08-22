using GWGUI.Emulation.Atari;
using GWGUI.MediaEngine.Exploration.Scp;

namespace GWGUI.Tests;

public sealed class AtariMachineOptionTests
{
    [Theory]
    [InlineData(AtariMachineModel.St, "st")]
    [InlineData(AtariMachineModel.Stf, "st")]
    [InlineData(AtariMachineModel.Stfm, "st")]
    [InlineData(AtariMachineModel.MegaSt, "st")]
    [InlineData(AtariMachineModel.Ste, "ste")]
    [InlineData(AtariMachineModel.MegaSte, "ste")]
    [InlineData(AtariMachineModel.Tt, "tt")]
    [InlineData(AtariMachineModel.Falcon, "falcon")]
    public void ModelIsMappedToNativeMachineType(AtariMachineModel model, string expected)
    {
        var result = AtariMachineOptionFunctions.Apply(new AtariMachineConfiguration(model));
        Assert.Equal(expected, result["hatari_machinetype"]);
    }

    [Fact]
    public void ExistingEditorsAreMappedToNativeOptions()
    {
        var configuration = new AtariMachineConfiguration(AtariMachineModel.MegaSte, options:
            new Dictionary<string, string>
            {
                ["gwgui_atari_main_memory"] = "8388608",
                ["gwgui_atari_cpu_frequency"] = "16",
                ["gwgui_atari_video_standard"] = "Monochrome",
                ["gwgui_atari_video_crop"] = "enabled",
                ["gwgui_atari_video_frameskip"] = "5",
                ["gwgui_atari_mouse_speed"] = "200",
                ["storage.speed.Floppy0"] = "400",
                ["storage.writeProtected.Floppy0"] = "True"
            });

        var result = AtariMachineOptionFunctions.Apply(configuration);

        Assert.Equal("8", result["hatari_ramsize"]);
        Assert.Equal("16", result["hatari_cpu_freq"]);
        Assert.Equal("true", result["hatari_video_hires"]);
        Assert.Equal("auto", result["hatari_forcerefresh"]);
        Assert.Equal("true", result["hatari_video_crop_overscan"]);
        Assert.Equal("5", result["hatari_frameskips"]);
        Assert.Equal("6", result["hatari_emulated_mouse_speed"]);
        Assert.Equal("true", result["hatari_fastfdc"]);
        Assert.Equal("on", result["hatari_writeprotect_floppy"]);
        Assert.Equal("false", result["hatari_nomouse"]);
        Assert.Equal("false", result["hatari_start_in_mouse_mode"]);
        Assert.Equal("false", result["hatari_nokeys"]);
        Assert.Equal("true", result["hatari_twojoy"]);
        Assert.Equal("false", result["hatari_led_status_display"]);
        Assert.Equal("0", result["hatari_joymousestatus_display"]);
        Assert.Equal("false", result["hatari_autoload_config"]);
    }

    [Fact]
    public void EnabledDriveActivityOsdIsPreservedForHatari()
    {
        var configuration = new AtariMachineConfiguration(AtariMachineModel.St, options:
            new Dictionary<string, string> { ["hatari_led_status_display"] = "true" });

        var result = AtariMachineOptionFunctions.Apply(configuration);

        Assert.Equal("true", result["hatari_led_status_display"]);
        Assert.Equal("1", result["hatari_joymousestatus_display"]);
    }
}
