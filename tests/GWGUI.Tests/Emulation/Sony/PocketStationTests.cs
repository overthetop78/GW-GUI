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
using GWGUI.Emulation.Sony.Emulators.PokketStation.Functions;
using System.Text.Json;

namespace GWGUI.Tests.Emulation.Sony;

public sealed class PocketStationTests
{
    [Fact]
    public void TwoCardsKeepTheirSlotsAndExposeTwoIndependentUnassignedPorts()
    {
        var configuration = LinkedConfiguration();
        var plan = Assert.IsType<GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts.CoreContentLoadPlan>(LinkFunctions.CreateLoadPlan(configuration));
        Assert.Equal(0x101u, plan.SubsystemId);
        Assert.EndsWith("unit-a.gme", plan.Paths[0]);
        Assert.EndsWith("unit-b.gme", plan.Paths[1]);
        Assert.Equal(new uint[] { 0x100, 0x200 }, plan.SaveMemoryRegions.Select(region => region.Id));
        Assert.Equal(2, plan.SaveMemoryRegions.Select(region => region.FileName).Distinct().Count());
        var input = InputSettingsFunctions.Describe(configuration);
        Assert.Equal(2, input.ControllerPorts.Count);
        Assert.All(input.ControllerPorts, port => Assert.Empty(port.Bindings.Values));
        Assert.Contains(input.ControllerPorts[0].Bindings.Definitions, item => item.Id == EmulationControllerCommandIds.L);
        Assert.Contains(input.ControllerPorts[0].Bindings.Definitions, item => item.Id == EmulationControllerCommandIds.R);
        Assert.DoesNotContain(input.ControllerPorts[1].Bindings.Definitions, item => item.Id is EmulationControllerCommandIds.L or EmulationControllerCommandIds.R);
        var storage = StorageSettingsFunctions.Describe(configuration);
        Assert.Equal(2, storage.ConfiguredSlots.Count(slot => slot.Category == EmulationMediaCategory.MemoryCard));
        var restored = JsonSerializer.Deserialize<MachineConfiguration>(JsonSerializer.Serialize(configuration))!;
        Assert.Equal(plan.Paths, LinkFunctions.CreateLoadPlan(restored)!.Paths);
        Assert.Equal(new int?[] { 1, 0 }, restored.Media!.Select(media => media.SlotIndex));
    }

    [Fact]
    public void RemovingTheSecondaryCardReturnsToASingleUnitWithoutRenumberingThePrimary()
    {
        var configuration = LinkedConfiguration();
        var storage = StorageSettingsFunctions.Describe(configuration);
        var updated = StorageSettingsFunctions.Apply(configuration, storage with { ConfiguredSlots = [EmulationMediaSlot.MemoryCard0] });
        Assert.Null(LinkFunctions.CreateLoadPlan(updated));
        Assert.Single(InputSettingsFunctions.Describe(updated).ControllerPorts);
        Assert.Equal(0, Assert.Single(updated.Media!).SlotIndex);
        Assert.DoesNotContain(new PokketStationMachineFactory().GetSettingsBlocks(updated).SelectMany(block => block.Fields), field => field.Id == LinkConstants.LayoutKey);
    }

    [Fact]
    public void ASecondaryCardAloneCannotBeLoadedAsThePrimary()
    {
        var configuration = Configuration() with { Media = [new MediaConfiguration("unit-b.gme", MediaCategory.MemoryCard, SlotIndex: 1)] };
        Assert.Throws<InvalidOperationException>(() => LinkFunctions.CreateLoadPlan(configuration));
        Assert.Equal(1, Assert.Single(EmulationMediaConversionFunctions.ToCommon(configuration.Media)).Slot.Index);
    }

    [Theory]
    [InlineData("pokket_link_layout", "Side by side")]
    [InlineData("pokket_link_layout", "Stacked")]
    [InlineData("pokket_link_audio", "Both units")]
    [InlineData("pokket_link_audio", "Unit A")]
    [InlineData("pokket_link_audio", "Unit B")]
    [InlineData("pokket_link_audio", "Unit A left, unit B right")]
    [InlineData("pokket_link_unit_b_id", "Different from unit A")]
    [InlineData("pokket_link_unit_b_id", "Same as unit A")]
    public void EveryLinkChoiceReachesTheNativeConfiguration(string key, string value)
    {
        var factory = new PokketStationMachineFactory();
        var original = LinkedConfiguration();
        var field = factory.GetSettingsBlocks(original).SelectMany(block => block.Fields).Single(field => field.Id == key);
        Assert.Contains(field.Choices!, choice => choice.Id == value);
        Assert.Equal(LinkConstants.Help, field.ExplanationResourceKey);
        Assert.Equal(key == LinkConstants.LayoutKey ? EmulationMachineTab.Video : key == LinkConstants.AudioKey ? EmulationMachineTab.Audio : EmulationMachineTab.General, field.Tab);
        using var http = new System.Net.Http.HttpClient();
        var module = new GWGUI.Emulation.Sony.Modules.SonyEmulationModule(Environment.CurrentDirectory, Environment.CurrentDirectory, http, Environment.CurrentDirectory);
        var updated = Assert.IsType<MachineConfiguration>(module.ApplySettings(original, new Dictionary<string, string?> { [key] = value }));
        Assert.Equal(value, factory.GetRuntimeOptions(updated)[key]);
    }

    private static MachineConfiguration LinkedConfiguration() => Configuration() with
    {
        Media =
        [
            new MediaConfiguration("unit-b.gme", MediaCategory.MemoryCard, MountOrder: 0, SlotIndex: 1),
            new MediaConfiguration("unit-a.gme", MediaCategory.MemoryCard, MountOrder: 1, SlotIndex: 0)
        ]
    };

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
        Assert.Equal(2, storage.AvailableDevices.Count);
        var device = Assert.Single(storage.AvailableDevices, item => item.IsPermanent);
        Assert.Equal(EmulationMediaSlot.MemoryCard0, device.Slot);
        Assert.Equal(EmulationMediaType.MemoryCard, device.MediaType);
        Assert.True(device.IsPermanent);
        Assert.Contains(EmulationMediaSlot.MemoryCard0, storage.ConfiguredSlots);
        Assert.Equal(CoreConstants.Definition.Extensions, device.AcceptedExtensions);
        var secondary = Assert.Single(storage.AvailableDevices, item => !item.IsPermanent);
        Assert.Equal(new EmulationMediaSlot(EmulationMediaCategory.MemoryCard, 1), secondary.Slot);
        Assert.Equal(EmulationMediaType.MemoryCard, secondary.MediaType);
        Assert.DoesNotContain(secondary.Slot, storage.ConfiguredSlots);
        Assert.DoesNotContain(storage.AvailableDevices, item => item.MediaType == EmulationMediaType.CompactDisc);
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
        var controller = Assert.Single(port.ControllerChoices, choice => choice.Id == "Joystick");
        Assert.Equal(EmulationControllerVisualIds.SonyPocketStation, controller.DefaultVisualId);
        Assert.Equal(EmulationControllerVisualIds.SonyPocketStation, Assert.Single(controller.CompatibleVisualIds!));
        Assert.All(port.ControllerChoices.Where(choice => choice.Id != controller.Id), choice => Assert.Null(choice.DefaultVisualId));
    }

    private static MachineConfiguration Configuration() => new(ModelConstants.PocketStation, CoreConstants.Id);
}
