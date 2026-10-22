using System.Buffers.Binary;
using System.Globalization;
using System.IO;
using System.Net.Http;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Philips.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Philips.Common.Machines.Common.Dictionaries;
using GWGUI.Emulation.Philips.Common.Machines.Common.Enums;
using GWGUI.Emulation.Philips.Common.Machines.Common.Functions;
using GWGUI.Emulation.Philips.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Philips.Emulators.O2em.Functions;
using GWGUI.Emulation.Philips.Modules;

namespace GWGUI.Tests.Emulation.Philips;

public sealed class O2emModelTests
{
    [Theory]
    [InlineData(MachineModel.VideopacG7400, "g7400.bin", "c500ff71236068e0dc0d0603d265ae76", "Philips Videopac+ G7400")]
    [InlineData(MachineModel.JopacJo7400, "jopac.bin", "279008e4a0db2dc5f1c048853b033828", "Brandt Jopac JO7400")]
    public void PlusModelsHaveTheirOwnBiosAndRetainTheirConfigurationSurfaces(MachineModel model, string bios, string md5, string name)
    {
        using var http = new HttpClient();
        var module = new PhilipsEmulationModule("unused", "unused", http, "unused");
        var machine = Assert.Single(module.Machines, machine => machine.Id == model.ToString());
        var configuration = Assert.IsType<MachineConfiguration>(module.CreateConfiguration(machine.Id));

        Assert.Equal(MachineFamily.Videopac, configuration.Family);
        Assert.Equal(Emulator.O2em, configuration.Core);
        Assert.True(ConfigurationFunctions.IsCompatible(model, Emulator.O2em));
        Assert.False(ConfigurationFunctions.IsCompatible(model, Emulator.SameCdi));
        Assert.False(ConfigurationFunctions.IsCompatible(model, Emulator.M2000));
        Assert.Equal(bios, O2emRuntimeFunctions.Options(configuration)["o2em_bios"]);
        Assert.Equal(bios, MediaRuntimeConstants.FileName(model));
        Assert.Equal(md5, MediaRuntimeConstants.Md5(model));
        Assert.True(module.TryGetString(machine.DisplayResourceKey, CultureInfo.GetCultureInfo("fr-FR"), out var localizedName));
        Assert.Equal(name, localizedName);

        var settings = module.Describe(machine.Id, configuration);
        Assert.True(settings.Visibility.Tabs[EmulationMachineTab.Keyboard]);
        Assert.True(settings.Visibility.Tabs[EmulationMachineTab.Controllers]);
        Assert.True(settings.Visibility.Tabs[EmulationMachineTab.Rom]);
        Assert.Contains(settings.Blocks.SelectMany(block => block.Fields), field => field.Id == FirmwareFunctions.Field(FirmwareCategory.SystemBios));
        Assert.Contains(settings.Blocks.SelectMany(block => block.Fields), field => field.Id == FirmwareFunctions.Field(FirmwareCategory.VoiceSamples));
        Assert.Equal(2, module.DescribeInputSettings(configuration).ControllerPorts.Count);
        Assert.Equal(new[] { EmulationMediaSlot.Cartridge0 }, configuration.ConfiguredSlots);

        var artwork = Assert.Single(MachineCatalog.All, definition => definition.Id == machine.Id);
        using var image = typeof(MachineCatalog).Assembly.GetManifestResourceStream(artwork.ImageResourceName!);
        Assert.NotNull(image);
        Span<byte> header = stackalloc byte[24];
        image.ReadExactly(header);
        Assert.True(header[..8].SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }));
        Assert.Equal(512u, BinaryPrimitives.ReadUInt32BigEndian(header[16..20]));
        Assert.InRange(BinaryPrimitives.ReadUInt32BigEndian(header[20..24]), 1u, 512u);
    }
}
