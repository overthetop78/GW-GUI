using GWGUI.Emulation;
using GWGUI.Emulation.Amiga;
using GWGUI.Emulation.Atari;

namespace GWGUI.Tests;

public sealed class EmulationConfigurationSummaryTests
{
    [Fact]
    public void AmigaStorage_UsesConfiguredDriveCountsInsteadOfModelMaximums()
    {
        var configuration = AmigaMachineConfiguration.A500(string.Empty) with
        {
            Options = new Dictionary<string, string>
            {
                ["gwgui_floppy_drive_count"] = "1",
                ["gwgui_hard_drive_count"] = "0",
                ["gwgui_cd_drive_enabled"] = "disabled"
            }
        };

        var storage = AmigaStorageSettingsFunctions.Describe(configuration);

        Assert.Single(storage.ConfiguredSlots);
        Assert.Equal(EmulationMediaSlot.Floppy0, storage.ConfiguredSlots[0]);
        var primaryDrive = storage.AvailableDevices.Single(device => device.Slot == EmulationMediaSlot.Floppy0);
        Assert.True(primaryDrive.IsRemovable);
        Assert.True(primaryDrive.IsPermanent);
    }

    [Fact]
    public void AtariStorage_DefaultsToOnlyThePrimaryDrive()
    {
        var storage = AtariStorageSettingsFunctions.Describe(
            new AtariMachineConfiguration(AtariMachineModel.St));

        Assert.Single(storage.ConfiguredSlots);
        Assert.Equal(EmulationMediaSlot.Floppy0, storage.ConfiguredSlots[0]);
    }

    [Fact]
    public void AmigaConfiguredMedia_RestoresLegacyInitialDisk()
    {
        var configuration = AmigaMachineConfiguration.A500(string.Empty, "Workbench.adf");

        var media = GWGUI.Emulation.Amiga.Services.AmigaExternalCore.ResolveConfiguredMedia(configuration);

        var disk = Assert.Single(media);
        Assert.Equal("Workbench.adf", disk.Path);
        Assert.Equal(AmigaMediaCategory.Floppy, disk.Category);
    }

    [Fact]
    public void AmigaSummary_ContainsSavedHardwareAndRuntimeDetails()
    {
        var configuration = AmigaMachineConfiguration.A500("Kickstart 1.3.rom") with
        {
            Options = new Dictionary<string, string>
            {
                ["puae_cpu_model"] = "68000",
                ["puae_video_standard"] = "PAL",
                ["puae_chipmem_size"] = "1",
                ["gwgui_floppy_drive_count"] = "2"
            }
        };

        var summary = AmigaConfigurationSummaryFunctions.Create(configuration);

        Assert.Equal("Emulation.Amiga.Model.A500", summary.MachineDisplayResourceKey);
        Assert.Contains("CPU 68000", summary.Details);
        Assert.Contains("RAM 512 KiB", summary.Details);
        Assert.Contains("Kickstart 1.3", summary.Details);
        Assert.Contains("DF 2", summary.Details);
        Assert.Contains("Video D3D11", summary.Details);
        Assert.Contains("Audio On", summary.Details);
    }

    [Fact]
    public void AtariSummary_ContainsSavedHardwareAndRuntimeDetails()
    {
        var configuration = new AtariMachineConfiguration(AtariMachineModel.St,
            options: new Dictionary<string, string>
            {
                [AtariConfigurationOptionConstants.MainMemory] = (1024 * 1024).ToString()
            });

        var summary = AtariConfigurationSummaryFunctions.Create(configuration);

        Assert.Equal("Emulation.Atari.Model.St", summary.MachineDisplayResourceKey);
        Assert.Contains("RAM 1 MiB", summary.Details);
        Assert.Contains("Core Hatari", summary.Details);
        Assert.Contains("Video D3D11", summary.Details);
        Assert.Contains("Audio On", summary.Details);
    }
}
