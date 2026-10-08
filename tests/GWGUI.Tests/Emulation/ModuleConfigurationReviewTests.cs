using System.Net.Http;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Enums;
using CommodoreMachines = GWGUI.Emulation.Commodore.Common.Machines.Common;
using NecMachines = GWGUI.Emulation.Nec.Common.Machines.Common;
using SonyMachines = GWGUI.Emulation.Sony.Common.Machines.Common;

namespace GWGUI.Tests.Emulation;

public sealed class ModuleConfigurationReviewTests
{
    [Theory]
    [InlineData("PlayStation", 2048)]
    [InlineData("PlayStation2", 32768)]
    [InlineData("Psp", 32768)]
    [InlineData("PsVita", 524288)]
    [InlineData("PlayStation3", 262144)]
    [InlineData("PlayStation4", 8388608)]
    [InlineData("PlayStation5", 16777216)]
    public void SonyMachineRamUsesKib(string machine, int expectedCapacity) =>
        Assert.Equal(expectedCapacity, SonyMachines.Dictionaries.ModelCatalog.Get(machine).RamKib);

    [Theory]
    [InlineData("A500", GWGUI.Emulation.Commodore.Common.Machines.Common.Enums.Emulator.PUAE)]
    [InlineData("A1200", GWGUI.Emulation.Commodore.Common.Machines.Common.Enums.Emulator.PUAE2021)]
    [InlineData("A4000", GWGUI.Emulation.Commodore.Common.Machines.Common.Enums.Emulator.Amiberry)]
    public void CommodoreHardDiskDirectoryIsAppliedToItsImageDialogs(string machine,
        GWGUI.Emulation.Commodore.Common.Machines.Common.Enums.Emulator emulator)
    {
        using var client = new HttpClient();
        var module = new GWGUI.Emulation.Commodore.Modules.CommodoreEmulationModule(".", ".", client, ".");
        var original = new CommodoreMachines.Contracts.MachineConfiguration(machine, emulator);
        var field = Assert.Single(module.Describe(machine, original).Blocks.SelectMany(block => block.Fields),
            field => field.Id == "folders.hardDisks");
        Assert.Equal(EmulationSettingsEditor.DirectoryPath, field.Editor);
        var updated = Assert.IsType<CommodoreMachines.Contracts.MachineConfiguration>(module.ApplySettings(original,
            new Dictionary<string, string?> { [field.Id] = "custom-hdd-directory" }));
        Assert.Equal("custom-hdd-directory", updated.HardDiskDirectory);
        Assert.DoesNotContain(field.Id, updated.Options!.Keys);
        Assert.All(module.DescribeStorageSettings(updated).AvailableDevices
            .Where(device => device.MediaType == EmulationMediaType.HardDisk),
            device => Assert.Equal(updated.HardDiskDirectory, device.ImageDirectory));
    }

    [Theory]
    [InlineData("Pc9801", "neko_project_ii")]
    [InlineData("Pc9821", "np2kai")]
    public void NecHardDiskDirectoryFollowsTheConfiguredMachine(string machine, string emulator)
    {
        using var client = new HttpClient();
        var module = new GWGUI.Emulation.Nec.Modules.NecEmulationModule(".", ".", client, ".");
        var original = new NecMachines.Contracts.MachineConfiguration(machine, emulator);
        var updated = Assert.IsType<NecMachines.Contracts.MachineConfiguration>(module.ApplySettings(original,
            new Dictionary<string, string?> { ["folders.hardDisks"] = "custom-hdd-directory" }));
        Assert.DoesNotContain("folders.hardDisks", updated.Options!.Keys);
        var devices = module.DescribeStorageSettings(updated).AvailableDevices
            .Where(device => device.MediaType == EmulationMediaType.HardDisk).ToArray();
        Assert.NotEmpty(devices);
        Assert.All(devices, device => Assert.Equal("custom-hdd-directory", device.ImageDirectory));
    }

    [Fact]
    public void NecCartridgeMachineDoesNotExposeCdOrHardDiskControls()
    {
        using var client = new HttpClient();
        var module = new GWGUI.Emulation.Nec.Modules.NecEmulationModule(".", ".", client, ".");
        var configuration = new NecMachines.Contracts.MachineConfiguration("PcEngine", "beetle_pce");
        var fields = module.Describe(configuration.Model, configuration).Blocks.SelectMany(block => block.Fields);
        Assert.DoesNotContain(fields, field => field.Id is "pce_cdbios" or "pce_arcadecard" or "pce_cddavolume" or "folders.hardDisks");
    }

    [Theory]
    [InlineData("PlayStation", "swanstation")]
    [InlineData("PlayStation2", "pcsx2")]
    public void SonyBiosSelectionKeepsArbitraryUserNamesOutOfCoreOptions(string machine, string emulator)
    {
        using var client = new HttpClient();
        var module = new GWGUI.Emulation.Sony.Modules.SonyEmulationModule(".", ".", client, ".");
        var original = new SonyMachines.Contracts.MachineConfiguration(machine, emulator);
        var field = module.Describe(machine, original).Blocks.SelectMany(block => block.Fields)
            .First(field => field.Tab == EmulationMachineTab.Rom);
        Assert.Equal(EmulationSettingsEditor.Path, field.Editor);
        var updated = Assert.IsType<SonyMachines.Contracts.MachineConfiguration>(module.ApplySettings(original,
            new Dictionary<string, string?> { [field.Id] = "my-personal-bios-dump.bin" }));
        Assert.Equal("my-personal-bios-dump.bin", updated.FirmwarePaths![field.Id]);
        Assert.DoesNotContain(field.Id, updated.Options!.Keys);
    }

    [Theory]
    [InlineData("PlayStation", "swanstation")]
    [InlineData("PlayStation2", "pcsx2")]
    [InlineData("Psp", "ppsspp")]
    public void SonyFailedConstructionReleasesAudio(string machine, string emulator)
    {
        var audio = new TrackingAudio();
        var configuration = new SonyMachines.Contracts.MachineConfiguration(machine, emulator);
        var context = new GWGUI.Emulation.Sony.Common.Contracts.EmulatorCreationContext(".", "core.dll", "host.exe",
            () => audio, _ => throw new InvalidOperationException("Interrupted construction"));
        Assert.Throws<InvalidOperationException>(() => new GWGUI.Emulation.Sony.Common.Services.Engine()
            .Adapter(configuration).Create(configuration, context));
        Assert.True(audio.Disposed);
    }

    private sealed class TrackingAudio : IAudioOutput
    {
        internal bool Disposed { get; private set; }
        public void Start(int sampleRate) { }
        public void Write(ReadOnlySpan<short> interleavedStereo) { }
        public void Flush() { }
        public void Stop() { }
        public void Dispose() => Disposed = true;
    }
}
