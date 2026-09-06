using GWGUI.Emulation.Atari.Contracts;
using GWGUI.Emulation.Atari.Enums;
using GWGUI.Emulation.Atari.Functions;
using GWGUI.Emulation.Amiga.Contracts;
using GWGUI.Emulation.Amiga.Enums;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
namespace GWGUI.Tests.Emulation.MachineAdapters;
internal static class MachineConfigurationMappingScenarios
{
    public static void AtariOption(string source,string value,string target,string? expected)
    {
        var configuration = new AtariMachineConfiguration(AtariMachineModel.Ste,options:new Dictionary<string,string>{{source,value},{"custom","kept"}});
        var mapped = AtariMachineOptionFunctions.Apply(configuration);
        if(expected is null) Assert.False(mapped.ContainsKey(target)); else Assert.Equal(expected,mapped[target]);
        Assert.Equal(value,configuration.Options[source]); Assert.Equal("kept",mapped["custom"]);
    }
    public static void AmigaOptions()
    {
        using var http = new System.Net.Http.HttpClient();
        var module = new GWGUI.Emulation.Amiga.Modules.AmigaEmulationModule("virtual-config","virtual-base",http,"virtual-core");
        var original = AmigaMachineConfiguration.A500("original-rom");
        var mapped = Assert.IsType<AmigaMachineConfiguration>(module.ApplySettings(original,new Dictionary<string,string?>
        {
            ["puae_crop"]="automatic",["configuration.kickstartPath"]="selected-rom",
            ["configuration.extendedRomPath"]=" ",["configuration.romKeyPath"]="selected-key",
            ["configuration.audioEnabled"]="disabled",["configuration.audioLatency"]="75",
            ["configuration.audioOutput"]="virtual-audio",["configuration.audioStereoSeparation"]="50",
            ["configuration.cpuSpeed"]="200|4",["configuration.parallelJoystickAdapter"]="enabled",
            ["custom"]="kept",["puae_floppy_multidrive"]=null
        }));
        Assert.Equal("automatic",mapped.Options!["puae_crop"]); Assert.Equal("selected-rom",mapped.KickstartPath);
        Assert.Null(mapped.ExtendedRomPath); Assert.Equal("selected-key",mapped.RomKeyPath); Assert.False(mapped.AudioEnabled);
        Assert.Equal(75,mapped.Audio!.LatencyMilliseconds); Assert.Equal(50,mapped.Audio.StereoSeparation); Assert.Equal("virtual-audio",mapped.Audio.OutputDeviceId);
        Assert.True(mapped.Input!.ParallelJoystickAdapterEnabled); Assert.Equal("kept",mapped.Options["custom"]);
        Assert.Equal("200",mapped.Options["puae_cpu_throttle"]); Assert.Equal("4",mapped.Options["puae_cpu_multiplier"]);
        Assert.False(mapped.Options.ContainsKey("puae_floppy_multidrive")); Assert.False(mapped.Options.ContainsKey("configuration.cpuSpeed"));
        Assert.Equal("original-rom",original.KickstartPath); Assert.False(original.Options!.ContainsKey("puae_crop")); Assert.Equal(original.Id,mapped.Id);
        var storage = GWGUI.Emulation.Amiga.Functions.AmigaStorageSettingsFunctions.Describe(mapped);
        var changed = GWGUI.Emulation.Amiga.Functions.AmigaStorageSettingsFunctions.Apply(mapped,storage with
        {
            ConfiguredSlots=[EmulationMediaSlot.Floppy0], MountedMedia=[new("virtual.adf",EmulationMediaSlot.Floppy0,EmulationMediaType.Floppy,true,true)]
        });
        Assert.Equal("virtual.adf",changed.InitialDiskPath); Assert.True(Assert.Single(changed.Media!).IsReadOnly);
        Assert.Equal("automatic",changed.Options!["puae_crop"]);
    }
    public static void AtariModule(string model)
    {
        using var http = new System.Net.Http.HttpClient();
        var module = new GWGUI.Emulation.Atari.Modules.AtariEmulationModule("virtual-config","virtual-base",http,"virtual-core");
        var original = new AtariMachineConfiguration(Enum.Parse<AtariMachineModel>(model),options:new Dictionary<string,string>{{"removed","old"},{"preserved","kept"}});
        var mapped = Assert.IsType<AtariMachineConfiguration>(module.ApplySettings(original,new Dictionary<string,string?>{{"removed",null},{"specific","synthetic"}}));
        Assert.False(mapped.Options.ContainsKey("removed")); Assert.Equal("synthetic",mapped.Options["specific"]); Assert.Equal("kept",mapped.Options["preserved"]);
        Assert.Equal(original.Model,mapped.Model); Assert.Equal(original.Core,mapped.Core); Assert.Equal(original.Id,mapped.Id);
        Assert.Equal("old",original.Options["removed"]); Assert.False(original.Options.ContainsKey("specific"));
        if(original.Family==AtariMachineFamily.St)
        {
            var options=AtariMachineOptionFunctions.Apply(mapped);
            Assert.Equal(model is "Ste" or "MegaSte" ? "ste" : model=="Tt" ? "tt" : model=="Falcon" ? "falcon" : "st",options["hatari_machinetype"]);
        }
    }
    public static void EightBitNormalization()
    {
        var original = new AtariMachineConfiguration(AtariMachineModel.Atari400,options:new Dictionary<string,string>
        {{"gwgui_atari_video_standard","bad"},{"atari800_internalbasic","bad"},{"atari800_show_diskled","bad"},{"atari800_axlon_shadow","enabled"},{"custom","kept"}});
        var options = AtariEightBitSettingsFunctions.Normalize(original);
        Assert.Equal("Ntsc",options["gwgui_atari_video_standard"]);
        Assert.Equal("disabled",options["atari800_internalbasic"]); Assert.Equal("enabled",options["atari800_show_diskled"]);
        Assert.Equal("disabled",options["atari800_axlon_shadow"]); Assert.Equal("kept",options["custom"]);
        Assert.Equal("bad",original.Options["gwgui_atari_video_standard"]);
    }
    public static void Options()
    {
        var options = new Dictionary<string, string> { ["gwgui_atari_main_memory"] = "4194304", ["gwgui_atari_cpu_frequency"] = "16", ["custom"] = "kept" };
        var configuration = new AtariMachineConfiguration(AtariMachineModel.Ste, options: options);
        var mapped = AtariMachineOptionFunctions.Apply(configuration);
        Assert.Equal("4", mapped["hatari_ramsize"]); Assert.Equal("16", mapped["hatari_cpu_freq"]); Assert.Equal("ste", mapped["hatari_machinetype"]);
        Assert.Equal("kept", mapped["custom"]); Assert.Equal(3, options.Count); Assert.Equal(3, configuration.Options.Count);
        var amiga = AmigaMachineConfiguration.A500("virtual-rom", "virtual-floppy");
        Assert.Equal("A500", amiga.Model); Assert.Equal("virtual-rom", amiga.KickstartPath); Assert.Equal("virtual-floppy", amiga.InitialDiskPath);
        Assert.NotEqual(Guid.Empty, amiga.EnsureId().Id); Assert.Equal(amiga.Id, amiga.EnsureId().Id);
    }
    public static void Media()
    {
        var mapped = GWGUI.Emulation.Amiga.Functions.EmulationMediaConversionFunctions.ToCommon([
            new("disk-a", AmigaMediaCategory.Floppy), new("hard-disk", AmigaMediaCategory.HardDrive), new("disk-b", AmigaMediaCategory.Floppy)]);
        Assert.Equal(new[] { EmulationMediaSlot.Floppy0, EmulationMediaSlot.HardDisk0, EmulationMediaSlot.Floppy1 }, mapped.Select(item => item.Slot));
        Assert.Equal(Path.GetFullPath("disk-b"), mapped[2].Path);
        var original = new EmulationMedia("virtual", EmulationMediaSlot.Floppy1, EmulationMediaType.Floppy, true, false);
        var atari = GWGUI.Emulation.Atari.Functions.EmulationMediaConversionFunctions.ToAtari(original, []);
        Assert.Equal(original, GWGUI.Emulation.Atari.Functions.EmulationMediaConversionFunctions.ToCommon(atari));
    }
}
