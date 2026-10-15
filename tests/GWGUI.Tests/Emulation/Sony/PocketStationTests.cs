using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Sony.Common.Dictionaries;
using GWGUI.Emulation.Sony.Common.Machines.Common.Constants;
using GWGUI.Emulation.Sony.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Sony.Common.Machines.Common.Enums;
using GWGUI.Emulation.Sony.Common.Machines.Common.Functions;
using GWGUI.Emulation.Sony.Emulators.PokketStation.Constants;
using GWGUI.Emulation.Sony.Emulators.PokketStation.Factories;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;

namespace GWGUI.Tests.Emulation.Sony;

public sealed class PocketStationTests
{
    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void MountedMemoryCardRemainsAMemoryCardInTheRuntimeConfiguration(bool readOnly, bool inserted)
    {
        var original = Configuration() with { Media = [new MediaConfiguration("pocket.gme", MediaCategory.MemoryCard, IsReadOnly: readOnly, IsInserted: inserted)] };
        var mounted = Assert.Single(EmulationMediaConversionFunctions.ToCommon(original.Media!));
        Assert.Equal(EmulationMediaSlot.MemoryCard0, mounted.Slot);
        Assert.Equal(EmulationMediaType.MemoryCard, mounted.Type);
        var restored = EmulationMediaConversionFunctions.WithMedia(original, [mounted]);
        var card = Assert.Single(restored.Media!);
        Assert.Equal(MediaCategory.MemoryCard, card.Category);
        Assert.Equal(mounted.Path, card.Path);
        Assert.Equal(readOnly, card.IsReadOnly);
        Assert.Equal(inserted, card.IsInserted);
        Assert.Equal(0, card.MountOrder);
        Assert.Equal("pocket.gme", Assert.Single(original.Media!).Path);
    }

    [Fact]
    public void RenamedCoreKeepsConfigurationAndInstallationLocations()
    {
        var definition = CoreConstants.Definition;
        Assert.Equal("pokketstation", definition.Id);
        Assert.Equal("Pokket", definition.LibraryName);
        Assert.Equal("Pokket", definition.Emulator.DisplayName);
        Assert.Equal("pokketstation_libretro.dll", definition.LibraryFileName);
        Assert.Equal(definition.LibraryFileName, definition.ReleaseSettings.InstalledLibraryName);
        Assert.Equal("pokket_libretro.dll", definition.ReleaseSettings.ArchiveLibraryName);
        Assert.EndsWith("/pokket_libretro.dll.zip", definition.ReleaseSettings.OfficialArchiveUri.AbsoluteUri);
        Assert.Equal(new[] { "pokketstation" }, definition.PreviousLibraryNames);
    }

    [Theory]
    [InlineData("Pokket", true)]
    [InlineData("POKKET", true)]
    [InlineData("pokketstation", true)]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("PCSX2", false)]
    [InlineData("pokketstation_libretro.dll", false)]
    public void OnlyTheDeclaredCurrentAndPreviousNativeNamesAreAccepted(string? name, bool expected)
    {
        Assert.Equal(expected, CoreIdentityFunctions.Accepts(CoreConstants.Definition, name));
        Assert.False(CoreIdentityFunctions.Accepts(GWGUI.Emulation.Sony.Emulators.Pcee2.Constants.CoreConstants.Definition, name));
    }

    [Fact]
    public void CoreIsOfferedOnlyForPocketStation()
    {
        var definition = Assert.Single(EmulatorCatalog.GetAll(ModelConstants.PocketStation));
        Assert.Equal(CoreConstants.Id, definition.Id);
        Assert.DoesNotContain(EmulatorCatalog.GetAll(ModelConstants.PlayStation), item => item.Id == CoreConstants.Id);
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

    private static MachineConfiguration Configuration() => new(ModelConstants.PocketStation, CoreConstants.Id);
}
