using GWGUI.Emulation.Sony.Common.Dictionaries;
using GWGUI.Emulation.Sony.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Sony.Common.Machines.Common.Constants;
using GWGUI.Emulation.Sony.Common.Interfaces;
using BeetleCore = GWGUI.Emulation.Sony.Emulators.BeetlePsx.Constants.CoreConstants;
using BeetleHardwareCore = GWGUI.Emulation.Sony.Emulators.BeetlePsxHw.Constants.CoreConstants;
using RearmedCore = GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Constants.CoreConstants;
using PceeCore = GWGUI.Emulation.Sony.Emulators.Pcee2.Constants.CoreConstants;
using PlayCore = GWGUI.Emulation.Sony.Emulators.Play.Constants.CoreConstants;
using RpcsCore = GWGUI.Emulation.Sony.Emulators.Rpcs3.Constants.CoreConstants;

namespace GWGUI.Tests.Emulation.Sony;

public sealed class EmulatorCatalogTests
{
    [Theory]
    [InlineData(BeetleCore.Id, ModelConstants.PlayStation, false)]
    [InlineData(BeetleHardwareCore.Id, ModelConstants.PlayStation, false)]
    [InlineData(RearmedCore.Id, ModelConstants.PlayStation, false)]
    [InlineData(PceeCore.Id, ModelConstants.PlayStation2, true)]
    [InlineData(PlayCore.Id, ModelConstants.PlayStation2, false)]
    [InlineData(RpcsCore.Id, ModelConstants.PlayStation3, true)]
    public void NewCoreUsesItsOwnMachineAndFirmwarePolicy(string id, string model, bool required)
    {
        var adapter = Assert.Single(EmulatorCatalog.CreateAdapters(), item => item.EmulatorId == id);
        var firmware = Assert.IsAssignableFrom<IEmulatorFirmwareAdapter>(adapter);
        var media = Assert.IsAssignableFrom<IEmulatorMediaAdapter>(adapter);
        var settings = Assert.IsAssignableFrom<IEmulatorSettingsAdapter>(adapter);
        Assert.Equal(model, Assert.Single(adapter.Definition.MachineIds));
        Assert.Equal(required, firmware.RequiresExternalFirmware);
        var configuration = new MachineConfiguration(model, EmulatorId: id);
        Assert.Equal(required, firmware.GetFirmwareSlots(configuration).Any(slot => slot.IsRequired));
        Assert.Contains(EmulatorCatalog.GetAll(model), item => item.Id == id);
        Assert.NotEmpty(media.SupportedContentExtensions);
        if (id == PlayCore.Id)
        {
            Assert.Empty(firmware.GetFirmwareSlots(configuration));
        }
        Assert.NotEmpty(settings.GetSettingsBlocks(configuration));
    }
}
