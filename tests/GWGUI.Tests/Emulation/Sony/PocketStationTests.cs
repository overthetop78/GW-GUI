using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Sony.Common.Dictionaries;
using GWGUI.Emulation.Sony.Common.Machines.Common.Constants;
using GWGUI.Emulation.Sony.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Sony.Common.Machines.Common.Functions;
using GWGUI.Emulation.Sony.Emulators.PokketStation.Constants;
using GWGUI.Emulation.Sony.Emulators.PokketStation.Factories;

namespace GWGUI.Tests.Emulation.Sony;

public sealed class PocketStationTests
{
    [Fact]
    public void CoreIsOfferedOnlyForPocketStation()
    {
        var definition = Assert.Single(EmulatorCatalog.GetAll(ModelConstants.PocketStation));
        Assert.Equal(CoreConstants.Id, definition.Id);
        Assert.DoesNotContain(EmulatorCatalog.GetAll(ModelConstants.PlayStation),
            item => item.Id == CoreConstants.Id);
    }

    [Fact]
    public void StorageOffersPermanentMemoryCardWithoutOpticalDrive()
    {
        var storage = StorageSettingsFunctions.Describe(Configuration());
        var device = Assert.Single(storage.AvailableDevices);
        Assert.Equal(EmulationMediaSlot.MemoryCard0, device.Slot);
        Assert.Equal(EmulationMediaType.MemoryCard, device.MediaType);
        Assert.True(device.IsPermanent);
        Assert.Contains(EmulationMediaSlot.MemoryCard0, storage.ConfiguredSlots);
        Assert.Equal(CoreConstants.Definition.Extensions, device.AcceptedExtensions);
    }

    [Fact]
    public void BiosUsesUserSelectableRomFieldAndIsRequired()
    {
        var adapter = new PokketStationMachineFactory();
        Assert.True(adapter.RequiresExternalFirmware);
        var slot = Assert.Single(adapter.GetFirmwareSlots(Configuration()));
        Assert.True(slot.IsRequired);
        var field = Assert.Single(FirmwareConfigurationFunctions.Fields(Configuration(), adapter));
        Assert.Equal(slot.FieldId, field.Id);
        Assert.Equal(EmulationMachineTab.Rom, field.Tab);
        Assert.Equal(EmulationSettingsEditor.Path, field.Editor);
        Assert.Equal(EmulationDefaultFolderCategory.Firmware, field.DefaultFolderCategory);
    }

    [Fact]
    public void IntegratedControlsExposeExactlyTheFiveNativeButtons()
    {
        var port = Assert.Single(InputSettingsFunctions.Describe(Configuration()).ControllerPorts);
        Assert.Null(port.Bindings.DefaultVisualId);
        var commands = port.Bindings.Definitions.Select(item => item.Id).ToHashSet();
        Assert.True(commands.SetEquals(new[]
        {
            EmulationControllerCommandIds.Up, EmulationControllerCommandIds.Down,
            EmulationControllerCommandIds.Left, EmulationControllerCommandIds.Right,
            EmulationControllerCommandIds.A
        }));
        Assert.All(port.ControllerChoices, choice => Assert.Null(choice.DefaultVisualId));
    }

    private static MachineConfiguration Configuration() =>
        new(ModelConstants.PocketStation, CoreConstants.Id);
}
