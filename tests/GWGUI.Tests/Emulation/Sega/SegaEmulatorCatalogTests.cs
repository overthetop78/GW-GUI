using GWGUI.Emulation.Sega.Common.Dictionaries;
using GWGUI.Emulation.Sega.Common.Machines.Common.Constants;
using GWGUI.Emulation.Sega.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Sega.Common.Machines.Common.Dictionaries;
using GWGUI.Emulation.Sega.Common.Machines.Common.Enums;
using GWGUI.Emulation.Sega.Common.Machines.Common.Functions;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Dictionaries;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Services;
using GWGUI.Emulation.Services;
using GWGUI.Emulation.Enums;
using System.Globalization;
using System.Text.RegularExpressions;

namespace GWGUI.Tests.Emulation.Sega;

public sealed class SegaEmulatorCatalogTests
{
    [Theory]
    [InlineData("genesis_plus_gx_bios", EmulationMachineTab.Rom)]
    [InlineData("genesis_plus_gx_vdp_mode", EmulationMachineTab.Video)]
    [InlineData("genesis_plus_gx_system_bram", EmulationMachineTab.Storage)]
    [InlineData("genesis_plus_gx_cart_size", EmulationMachineTab.Ram)]
    [InlineData("genesis_plus_gx_audio_filter", EmulationMachineTab.Audio)]
    [InlineData("genesis_plus_gx_md_channel_0_volume", EmulationMachineTab.Audio)]
    [InlineData("genesis_plus_gx_overclock", EmulationMachineTab.Cpu)]
    [InlineData("genesis_plus_gx_gun_input", EmulationMachineTab.Controllers)]
    [InlineData("genesis_plus_gx_invert_mouse", EmulationMachineTab.Mouse)]
    public void CoreOptionsAppearInTheirOwnTab(string key, EmulationMachineTab expected)
    {
        var blocks = CoreSettingsFunctions.Blocks(new(ModelConstants.MegaCd, "genesisplusgx"));
        var block = Assert.Single(blocks, item => item.Fields.Any(field => field.Id == key));
        Assert.Equal(expected, block.Tab);
        Assert.Equal(expected, Assert.Single(block.Fields, field => field.Id == key).Tab);
        Assert.Equal(1, block.Columns);
        Assert.NotEqual("Cpu", block.Icon);
    }

    [Fact]
    public void RestartNoticeIsReservedForOptionsWhichRequireIt()
    {
        var fields = CoreSettingsFunctions.Blocks(new(ModelConstants.MegaCd, "genesisplusgx"))
            .SelectMany(block => block.Fields).ToArray();
        Assert.False(Assert.Single(fields, field => field.Id == "genesis_plus_gx_audio_filter").RequiresRestart);
        Assert.True(Assert.Single(fields, field => field.Id == "genesis_plus_gx_cart_size").RequiresRestart);
    }

    [Theory]
    [InlineData("Sg1000", "genesisplusgx")]
    [InlineData("Sc3000", "genesisplusgx")]
    [InlineData("MasterSystem", "genesisplusgx")]
    [InlineData("GameGear", "genesisplusgx")]
    [InlineData("MegaDrive", "genesisplusgx")]
    [InlineData("Sg1000", "genesis_plus_gx_wide")]
    [InlineData("MasterSystem", "picodrive")]
    public void OtherMachinesDoNotOfferMegaCdOptions(string machineId, string emulatorId)
    {
        var settings = SettingsDescriptionFunctions.Create(new MachineConfiguration(machineId, emulatorId));
        var fields = settings.SelectMany(block => block.Fields).ToArray();
        Assert.DoesNotContain(fields, field => field.Id.EndsWith("_add_on", StringComparison.Ordinal)
            || field.Id.EndsWith("_system_bram", StringComparison.Ordinal)
            || field.Id.EndsWith("_cart_bram", StringComparison.Ordinal)
            || field.Id.EndsWith("_cart_size", StringComparison.Ordinal)
            || field.Id.EndsWith("_ramcart", StringComparison.Ordinal));
        Assert.Single(settings, block => block.TitleResourceKey == SettingsDescriptionFunctionsConstants.ResourceGeneral);
    }

    [Fact]
    public void MegaCdKeepsItsOwnCdSettings()
    {
        var fields = CoreSettingsFunctions.Blocks(new(ModelConstants.MegaCd, "genesisplusgx"))
            .SelectMany(block => block.Fields).ToArray();
        Assert.Contains(fields, field => field.Id == "genesis_plus_gx_add_on");
        Assert.Contains(fields, field => field.Id == "genesis_plus_gx_system_bram");
    }

    [Fact]
    public void SwitchingToSg1000ResetsAnInapplicableCdOption()
    {
        var configuration = new MachineConfiguration(ModelConstants.Sg1000, "genesisplusgx",
            Options: new Dictionary<string, string> { ["genesis_plus_gx_add_on"] = "sega/mega cd" });
        var definition = CoreCatalog.Get(configuration.EmulatorId).Options.Single(option => option.Key == "genesis_plus_gx_add_on");
        Assert.Equal(definition.DefaultValue, CoreSettingsFunctions.Configure(configuration).Options![definition.Key]);
    }

    [Fact]
    public void BlueMsxSegaMachinesDoNotOfferMsxCartridgeMappers()
    {
        var fields = CoreSettingsFunctions.Blocks(new(ModelConstants.Sg1000, "bluemsx"))
            .SelectMany(block => block.Fields).ToArray();
        var mapper = Assert.Single(fields, field => field.Id == "bluemsx_cartmapper");
        Assert.Equal(["Auto", "SG1000", "SG1000Castle", "SG1000RamA", "SG1000RamB"], mapper.Choices!.Select(choice => choice.Id));
        Assert.DoesNotContain(fields, field => field.Id == "bluemsx_ym2413_enable");
    }

    [Theory]
    [InlineData("Naomi")]
    [InlineData("Naomi2")]
    [InlineData("Atomiswave")]
    [InlineData("SystemSp")]
    public void FlycastArcadeMachinesDoNotOfferDreamcastVmuSettings(string model)
    {
        var fields = CoreSettingsFunctions.Blocks(new(model, "flycast"))
            .SelectMany(block => block.Fields).ToArray();
        Assert.DoesNotContain(fields, field => field.Id.Contains("vmu", StringComparison.Ordinal)
            || field.Id.StartsWith("reicast_device_port", StringComparison.Ordinal)
            || field.Id == "reicast_dc_32mb_mod");
        Assert.Contains(fields, field => field.Id == "reicast_allow_service_buttons");
    }

    [Fact]
    public void EveryCoreScopeAndVisibleFieldBelongsToTheConfiguredMachine()
    {
        foreach (var core in CoreCatalog.All)
        {
            foreach (var option in core.Options)
            {
                if (option.MachineIds is { } scope)
                    Assert.All(scope, model => Assert.Contains(model, core.Emulator.MachineIds));
                foreach (var value in option.Values)
                    if (value.MachineIds is { } valueScope)
                        Assert.All(valueScope, model => Assert.Contains(model, core.Emulator.MachineIds));
            }
            foreach (var model in core.Emulator.MachineIds)
            {
                var configuration = new MachineConfiguration(model, core.Id);
                var settings = SettingsDescriptionFunctions.Create(configuration);
                foreach (var field in settings.SelectMany(block => block.Fields))
                {
                    var option = core.Options.FirstOrDefault(option => option.Key == field.Id);
                    if (option is null) continue;
                    Assert.True(option.IsVisible);
                    Assert.True(option.MachineIds is null || option.MachineIds.Contains(model));
                    Assert.Equal(option.Tab, field.Tab);
                    Assert.NotEmpty(field.Choices!);
                }
            }
        }
    }

    [Fact]
    public void CoreOptionResourcesResolveInEveryPublishedCulture()
    {
        var assembly = typeof(EmulatorCatalog).Assembly;
        var localization = new EmulationModuleLocalization(assembly, "GWGUI.Emulation.Sega.Resources.");
        var cultures = assembly.GetManifestResourceNames()
            .Select(name => Regex.Match(name, @"\.([a-z]{2}(?:-[A-Za-z]{2,4})?)\.resources$"))
            .Where(match => match.Success)
            .Select(match => CultureInfo.GetCultureInfo(match.Groups[1].Value))
            .DistinctBy(culture => culture.Name).ToArray();
        Assert.Equal(29, cultures.Length);
        var keys = CoreCatalog.All.SelectMany(core => core.Options)
            .SelectMany(option => new[] { option.Name, option.Description }
                .Concat(option.Values.Select(value => value.Label)))
            .OfType<string>()
            .Where(key => key.StartsWith("Emulation.", StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal).ToArray();
        Assert.NotEmpty(keys);
        foreach (var culture in cultures)
            foreach (var key in keys)
                Assert.True(localization.TryGetString(key, culture, out var value)
                    && !string.IsNullOrWhiteSpace(value), $"{culture.Name}: {key}");
    }

    [Theory]
    [InlineData("Sg1000", "bluemsx", "gearsystem", "genesisplusgx", "genesis_plus_gx_wide", "picodrive")]
    [InlineData("Sc3000", "bluemsx", "genesisplusgx", "genesis_plus_gx_wide", "picodrive")]
    [InlineData("Sf7000", "bluemsx")]
    [InlineData("MegaCd", "clownmdemu", "genesisplusgx", "genesis_plus_gx_wide", "picodrive")]
    [InlineData("ThirtyTwoX", "picodrive")]
    [InlineData("Saturn", "mednafen_saturn", "kronos", "yabause", "yabasanshiro", "ymir")]
    [InlineData("SystemSp", "flycast")]
    [InlineData("StV", "kronos")]
    [InlineData("Model3", "supermodel")]
    [InlineData("DreamcastVmu", "vemulator")]
    public void MachineOffersItsDeclaredCores(string machineId, params string[] expected)
    {
        Assert.Equal(expected.Order(StringComparer.Ordinal),
            EmulatorCatalog.GetAll(machineId).Select(item => item.Id).Order(StringComparer.Ordinal));
        Assert.Contains(EmulatorCatalog.DefaultFor(machineId), expected);
    }

    [Fact]
    public void EveryFactoryAndMachineHasAnExplicitValidDefinition()
    {
        var adapters = EmulatorCatalog.CreateAdapters();
        Assert.Equal(16, adapters.Count);
        Assert.Equal(adapters.Count, adapters.Select(adapter => adapter.EmulatorId).Distinct().Count());
        Assert.Equal(19, ModelCatalog.All.Count);
        Assert.All(ModelCatalog.All, model => Assert.Contains(EmulatorCatalog.GetAll(model.Id),
            core => core.Id == EmulatorCatalog.DefaultFor(model.Id)));
    }

    [Theory]
    [InlineData("Sg1000", "SEGA - SG-1000")]
    [InlineData("Sc3000", "SEGA - SC-3000")]
    [InlineData("Sf7000", "SEGA - SF-7000")]
    public void BlueMsxUsesTheChosenSegaMachineAndHidesItsSelector(string model, string profile)
    {
        var configuration = new MachineConfiguration(model, "bluemsx");
        Assert.Equal(profile, CoreSettingsFunctions.Configure(configuration).Options!["bluemsx_msxtype"]);
        Assert.DoesNotContain(CoreSettingsFunctions.Blocks(configuration).SelectMany(block => block.Fields),
            field => field.Id == "bluemsx_msxtype");
    }

    [Fact]
    public void FirmwareSlotsStaySpecificToTheirMachineAndCanHaveSeveralFiles()
    {
        var flycast = EmulatorCatalog.CreateAdapters().Single(adapter => adapter.EmulatorId == "flycast");
        Assert.Equal(["dc/dc_boot.bin", "dc/dc_flash.bin"],
            flycast.GetFirmwareSlots(new(ModelConstants.Dreamcast, flycast.EmulatorId)).Select(slot => slot.FileName));
        Assert.Equal("dc/segasp.zip", Assert.Single(flycast.GetFirmwareSlots(new(ModelConstants.SystemSp, flycast.EmulatorId))).FileName);
        Assert.Empty(EmulatorCatalog.CreateAdapters().Single(adapter => adapter.EmulatorId == "gearsystem")
            .GetFirmwareSlots(new(ModelConstants.Sg1000, "gearsystem")));
    }

    [Fact]
    public void FirmwarePathsAreIndependentOfSourceFileNames()
    {
        var adapter = EmulatorCatalog.CreateAdapters().Single(item => item.EmulatorId == "flycast");
        var configuration = new MachineConfiguration(ModelConstants.Dreamcast, adapter.EmulatorId);
        var slot = adapter.GetFirmwareSlots(configuration).First();
        var applied = FirmwareConfigurationFunctions.Apply(configuration,
            new Dictionary<string, string?> { [slot.FieldId] = "my-dreamcast-backup.rom" }, adapter);
        Assert.Equal("my-dreamcast-backup.rom", applied.FirmwarePaths![slot.FieldId]);
        Assert.Equal("my-dreamcast-backup.rom", FirmwareConfigurationFunctions.Fields(applied, adapter).First().Value);
    }

    [Fact]
    public void RuntimeOptionsExcludeApplicationSettingsAndEnforceTheSelectedHardware()
    {
        var configuration = new MachineConfiguration(ModelConstants.MasterSystem, "genesisplusgx",
            Options: new Dictionary<string, string> { ["foreign-setting"] = "value", ["genesis_plus_gx_region_detect"] = "pal" });
        var options = FirmwareFunctions.RuntimeOptions(configuration, string.Empty);
        Assert.DoesNotContain("foreign-setting", options.Keys);
        Assert.Equal("pal", options["genesis_plus_gx_region_detect"]);
        Assert.Equal("master system", options["genesis_plus_gx_system_hw"]);
    }

    [Fact]
    public void StorageFormatsBelongToTheSelectedCoreAndMediaType()
    {
        var megaDrive = StorageSettingsFunctions.Describe(new(ModelConstants.MegaDrive, "genesisplusgx"));
        var cartridge = megaDrive.AvailableDevices.Single();
        Assert.Contains(".bin", cartridge.AcceptedExtensions);
        Assert.DoesNotContain(".cue", cartridge.AcceptedExtensions);
        Assert.DoesNotContain(".32x", cartridge.AcceptedExtensions);
        var cd = StorageSettingsFunctions.Describe(new(ModelConstants.MegaCd, "genesisplusgx"));
        Assert.DoesNotContain(cd.AvailableDevices.SelectMany(device => device.AcceptedExtensions), extension => extension == ".sms");
    }

    [Fact]
    public void ACoreWithoutControllerMetadataReceivesTheStandardJoypad()
    {
        Assert.Equal(1u, ExternalCore.ResolveControllerDevice([], ControllerType.SegaMegaDriveThreeButton));
        Assert.Equal(0u, ExternalCore.ResolveControllerDevice([], ControllerType.None));
    }
}
