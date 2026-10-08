using GWGUI.Emulation.Nintendo.Common.Services;
using GWGUI.Emulation.Nintendo.Common.Contracts;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Nintendo.Common.Dictionaries;
using GWGUI.Emulation.Nintendo.Common.Machines.Common.Constants;
using GWGUI.Emulation.Nintendo.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Nintendo.Common.Machines.Common.Dictionaries;
using GWGUI.Emulation.Nintendo.Common.Machines.Common.Functions;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Dictionaries;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Functions;

namespace GWGUI.Tests.Emulation.Nintendo;

public sealed class NintendoEmulatorCatalogTests
{
    [Theory]
    [InlineData("Nintendo3Ds", "azahar", "citra_graphics_api")]
    [InlineData("WiiU", "cemu", "cemu_gpu_api")]
    public void GraphicsApiChoicesMatchTheHostCapabilities(string machine, string emulator, string optionKey)
    {
        var configuration = new MachineConfiguration(machine, emulator);
        var field = CoreSettingsFunctions.Blocks(configuration).SelectMany(block => block.Fields)
            .Single(item => item.Id == optionKey);
        Assert.DoesNotContain(field.Choices!, choice => choice.Id == "Vulkan");
        Assert.Contains(field.Choices!, choice => choice.Id == "OpenGL");
        var runtimeOption = CoreSettingsFunctions.WithSupportedGraphicsApis(
            CoreCatalog.Get(emulator).Options.Single(option => option.Key == optionKey));
        Assert.DoesNotContain(runtimeOption.Values, value => value.Value == "Vulkan");
        Assert.Contains(runtimeOption.Values, value => value.Value == runtimeOption.DefaultValue);
    }

    [Fact]
    public void FailedMachineConstructionDisposesAllocatedAudio()
    {
        var configuration = new MachineConfiguration("Nes", "mesen", AudioEnabled: true);
        var audio = new TrackingAudioOutput();
        var context = new EmulatorCreationContext(".", "core.dll", "host.exe", () => audio,
            _ => throw new InvalidOperationException("Interrupted construction"));
        Assert.Throws<InvalidOperationException>(() => new Engine().Adapter(configuration).Create(configuration, context));
        Assert.True(audio.Disposed);
    }

    private sealed class TrackingAudioOutput : IAudioOutput
    {
        internal bool Disposed { get; private set; }
        public void Start(int sampleRate) { }
        public void Write(ReadOnlySpan<short> interleavedStereo) { }
        public void Flush() { }
        public void Stop() { }
        public void Dispose() => Disposed = true;
    }

    [Theory]
    [InlineData("Nintendo3Ds", "azahar,citra,citra2018,panda3ds")]
    [InlineData("NintendoDs", "desmume,desmume2015,melonds,melondsds,noods,skyemu")]
    [InlineData("NintendoDsi", "melondsds")]
    [InlineData("GameBoy", "mgba,vbam,skyemu,mesen2,doublecherrygb,fixgb,gambatte,gearboy,irogb,mesen_s,sameboy,tgbdual")]
    [InlineData("GameBoyAdvance", "skyemu,mednafen_gba,gpsp,meteor,mgba,vbam,vba_next")]
    [InlineData("GameBoyColor", "mgba,vbam,skyemu,mesen2,doublecherrygb,fixgb,gambatte,gearboy,irogb,mesen_s,sameboy,tgbdual")]
    [InlineData("Nes", "fceumm,fixnes,mesen,mesen2,nestopia,quicknes,rustynes")]
    [InlineData("Snes", "mesen2,mesen_s,nside_sfc_balanced,mednafen_snes,bsnes,bsnes_cplusplus98,bsnes_jg,bsnes_hd_beta,bsnes2014_accuracy,bsnes2014_balanced,bsnes2014_performance,bsnes_mercury_accuracy,bsnes_mercury_balanced,bsnes_mercury_performance,snes9x,snes9x2002,snes9x2005,snes9x2005_plus,snes9x2010,mednafen_supafaust")]
    [InlineData("GameWatch", "gw")]
    [InlineData("GameCube", "dolphin")]
    [InlineData("Wii", "dolphin")]
    [InlineData("Nintendo64", "mupen64plus-next,parallel_n64")]
    [InlineData("PokemonMini", "pokemini")]
    [InlineData("VirtualBoy", "beetle_vb")]
    [InlineData("WiiU", "cemu")]
    public void RequestedMachinesExposeExactlyTheirEmulators(string machine, string ids)
    {
        Assert.NotNull(ModelCatalog.Get(machine));
        Assert.Equal(ids.Split(',').Order(), EmulatorCatalog.GetAll(machine).Select(item => item.Id).Order());
        Assert.Contains(EmulatorCatalog.DefaultFor(machine), ids.Split(','));
    }

    [Fact]
    public void EveryCoreHasDistinctOptionsWithSelectableDefaults()
    {
        Assert.Equal(56, CoreCatalog.All.Count);
        Assert.Equal(56, CoreCatalog.All.Select(core => core.Id).Distinct().Count());
        foreach (var core in CoreCatalog.All)
        {
            Assert.Equal(core.Options.Count, core.Options.Select(option => option.Key).Distinct().Count());
            foreach (var option in core.Options)
                Assert.Contains(option.Values, value => value.Value == option.DefaultValue);
        }
    }

    [Theory]
    [InlineData("NintendoDs", "ds")]
    [InlineData("NintendoDsi", "dsi")]
    public void MelonDsDsConsoleModeFollowsSelectedMachine(string machine, string mode)
    {
        var configuration = CoreSettingsFunctions.Configure(new MachineConfiguration(machine, "melondsds"));
        Assert.Equal(mode, configuration.Options!["melonds_console_mode"]);
    }

    [Theory]
    [InlineData("NintendoDsi", "melondsds")]
    [InlineData("GameBoyAdvance", "mgba")]
    [InlineData("GameBoy", "mgba")]
    [InlineData("GameBoyColor", "mgba")]
    [InlineData("GameBoy", "vbam")]
    [InlineData("GameBoyColor", "vbam")]
    public void FirmwareSelectionPreservesArbitraryUserFileNames(string machine, string emulator)
    {
        var configuration = new MachineConfiguration(machine, emulator);
        var slot = FirmwareFunctions.Slots(configuration).First();
        var selected = FirmwareConfigurationFunctions.Apply(configuration, new Dictionary<string, string?>
        {
            [slot.FieldId] = "my-personal-bios-dump.bin"
        }, new Engine().Adapter(configuration));
        Assert.Equal("my-personal-bios-dump.bin", selected.FirmwarePaths![slot.FieldId]);
        Assert.Null(selected.Options);
    }

    [Fact]
    public void FirmwareSlotsBelongToTheSelectedMachine()
    {
        var ds = FirmwareFunctions.Slots(new MachineConfiguration("NintendoDs", "melondsds"));
        Assert.DoesNotContain(ds, slot => slot.FileName.StartsWith("dsi_", StringComparison.Ordinal));
        var gb = FirmwareFunctions.Slots(new MachineConfiguration("GameBoy", "mesen2"));
        Assert.DoesNotContain(gb, slot => slot.FileName.StartsWith("dsp", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("NintendoDsi", "melondsds", ".dsi")]
    [InlineData("PokemonMini", "pokemini", ".min")]
    [InlineData("WiiU", "cemu", ".rpx")]
    public void AddedMachinesExposeTheirMediaExtensions(string machine, string emulator, string extension)
    {
        var configuration = new MachineConfiguration(machine, emulator);
        var storage = StorageSettingsFunctions.Describe(configuration, new Engine().Adapter(configuration));
        Assert.Contains(storage.AvailableDevices, device => device.AcceptedExtensions.Contains(extension));
    }

    [Theory]
    [InlineData("NintendoDsi")]
    [InlineData("PokemonMini")]
    public void AddedMachinesHaveEmbeddedPngImages(string machine)
    {
        var definition = MachineCatalog.All.Single(item => item.Id == machine);
        Assert.NotNull(definition.ImageResourceName);
        Assert.Contains(definition.ImageResourceName, typeof(ModelCatalog).Assembly.GetManifestResourceNames());
    }

    [Theory]
    [InlineData("Nes", "mesen2", "mesen_palette", EmulationMachineTab.Video)]
    [InlineData("Snes", "mesen2", "mesen_snes_hide_bg_layer_1", EmulationMachineTab.Video)]
    [InlineData("GameBoy", "mesen2", "mesen_gameboy_adjust_colors", EmulationMachineTab.Video)]
    [InlineData("NintendoDs", "desmume", "desmume_cpu_mode", EmulationMachineTab.Cpu)]
    [InlineData("GameBoyAdvance", "gpsp", "gpsp_bios", EmulationMachineTab.Rom)]
    public void NativeSettingsUseTheirFunctionalTab(string machine, string emulator, string key, EmulationMachineTab tab)
    {
        var configuration = new MachineConfiguration(machine, emulator);
        var blocks = SettingsDescriptionFunctions.Create(configuration, new Engine().Adapter(configuration));
        var block = Assert.Single(blocks, block => block.Fields.Any(field => field.Id == key));
        var field = Assert.Single(block.Fields, field => field.Id == key);
        Assert.Equal(tab, block.Tab);
        Assert.Equal(tab, field.Tab);
        Assert.Equal(block.Id, field.BlockId);
        Assert.Single(blocks, block => block.Tab == EmulationMachineTab.General);
    }

    [Theory]
    [InlineData("Nes", "mesen2", "mesen_snes_hide_bg_layer_1")]
    [InlineData("Snes", "mesen2", "mesen_palette")]
    [InlineData("GameBoy", "mesen2", "mesen_palette")]
    [InlineData("GameCube", "dolphin", "dolphin_sensor_bar_position")]
    [InlineData("Wii", "dolphin", "dolphin_skip_gc_bios")]
    [InlineData("GameBoy", "skyemu", "system_gba_bios_enable")]
    [InlineData("GameBoyAdvance", "skyemu", "system_nds_bios_enable")]
    [InlineData("GameBoy", "mgba", "mgba_solar_sensor_level")]
    [InlineData("GameBoyColor", "mgba", "mgba_force_gbp")]
    [InlineData("GameBoyAdvance", "mgba", "mgba_gb_model")]
    [InlineData("GameBoy", "vbam", "vbam_forceRTCenable")]
    [InlineData("GameBoyColor", "vbam", "vbam_layer_1")]
    [InlineData("GameBoyAdvance", "vbam", "vbam_gbHardware")]
    public void SettingsOfOtherMachinesAreNotPresented(string machine, string emulator, string key)
    {
        var fields = CoreSettingsFunctions.Blocks(new MachineConfiguration(machine, emulator))
            .SelectMany(block => block.Fields);
        Assert.DoesNotContain(fields, field => field.Id == key);
    }

    [Theory]
    [InlineData("GameBoy", "mgba", "mgba_gb_model", "Game Boy")]
    [InlineData("GameBoyColor", "mgba", "mgba_gb_model", "Game Boy Color")]
    [InlineData("GameBoy", "vbam", "vbam_gbHardware", "gb")]
    [InlineData("GameBoyColor", "vbam", "vbam_gbHardware", "gbc")]
    public void GameBoyHardwareDefaultsFollowTheSelectedMachine(string machine, string emulator, string key, string expected)
    {
        var configuration = CoreSettingsFunctions.Configure(new MachineConfiguration(machine, emulator));
        Assert.Equal(expected, configuration.Options![key]);
    }

    [Theory]
    [InlineData("GameBoy", "mgba", ".gb")]
    [InlineData("GameBoyColor", "mgba", ".gbc")]
    [InlineData("GameBoy", "vbam", ".gb")]
    [InlineData("GameBoyColor", "vbam", ".gbc")]
    public void GameBoyAssociationsExposeCompatibleBiosAndCartridges(string machine, string emulator, string extension)
    {
        var configuration = new MachineConfiguration(machine, emulator);
        var firmware = FirmwareFunctions.Slots(configuration);
        Assert.Contains(firmware, slot => slot.FileName == "gb_bios.bin");
        Assert.Contains(firmware, slot => slot.FileName == "gbc_bios.bin");
        Assert.DoesNotContain(firmware, slot => slot.FileName == "gba_bios.bin");
        var storage = StorageSettingsFunctions.Describe(configuration, new Engine().Adapter(configuration));
        Assert.Contains(storage.AvailableDevices, device => device.AcceptedExtensions.Contains(extension));
        Assert.DoesNotContain(storage.AvailableDevices, device => device.AcceptedExtensions.Contains(".gba"));
    }

    [Theory]
    [InlineData("GameBoy", "Game Boy")]
    [InlineData("GameBoyColor", "Game Boy")]
    [InlineData("GameBoyAdvance", "Game Boy Advance")]
    [InlineData("NintendoDs", "Nintendo DS")]
    public void SkyEmuUsesTheConfiguredMachine(string machine, string core)
    {
        var configuration = CoreSettingsFunctions.Configure(new MachineConfiguration(machine, "skyemu",
            Options: new Dictionary<string, string> { ["system_core_override"] = "Nintendo DS" }));
        Assert.Equal(core, configuration.Options!["system_core_override"]);
        Assert.DoesNotContain(CoreSettingsFunctions.Blocks(configuration).SelectMany(block => block.Fields),
            field => field.Id == "system_core_override");
    }
}
