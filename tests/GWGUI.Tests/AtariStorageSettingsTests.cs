using GWGUI.App.Controls;
using GWGUI.Emulation;
using GWGUI.Emulation.Atari;

namespace GWGUI.Tests;

[Collection(AtariNativeCoreTestConstants.CollectionName)]
public sealed class AtariStorageSettingsTests
{
    public static TheoryData<AtariMachineModel> EveryModel => new(Enum.GetValues<AtariMachineModel>());

    [Theory]
    [MemberData(nameof(EveryModel))]
    public void EveryModelShowsExactlyItsAvailableMedia(AtariMachineModel model)
    {
        var view = AtariStorageSettingsFunctions.Create(new AtariMachineConfiguration(model));
        var expected = AtariCompatibilityCatalog.Get(model).Media
            .Where(rule => rule.Availability == AtariMediaAvailability.Available)
            .Select(rule => rule.Kind).Distinct().Order().ToArray();

        Assert.Equal(expected, view.Types.Select(value => value.Kind).Order());
        Assert.All(view.Types, type => Assert.NotEmpty(view.Slots[type.Kind]));
    }

    [Fact]
    public void JaguarAndJaguarCdExposeDifferentDevices()
    {
        var jaguar = AtariStorageSettingsFunctions.Create(
            new AtariMachineConfiguration(AtariMachineModel.Jaguar));
        var jaguarCd = AtariStorageSettingsFunctions.Create(
            new AtariMachineConfiguration(AtariMachineModel.JaguarCd));

        Assert.Contains(jaguar.Types, value => value.Kind == AtariMediaKind.Cartridge);
        Assert.DoesNotContain(jaguar.Types, value => value.Kind == AtariMediaKind.CompactDisc);
        Assert.Contains(jaguarCd.Types, value => value.Kind == AtariMediaKind.Cartridge);
        Assert.Contains(jaguarCd.Types, value => value.Kind == AtariMediaKind.CompactDisc);
    }

    [Theory]
    [MemberData(nameof(EveryModel))]
    public void EveryModelHasExactlyOneFixedPrimaryDevice(AtariMachineModel model)
    {
        var view = AtariStorageSettingsFunctions.Create(new AtariMachineConfiguration(model));
        var item = Assert.Single(view.Devices);
        var device = item.Configuration;
        var expected = ExpectedPrimaryDevice(model);

        Assert.Equal(expected.Kind, device.Kind);
        Assert.Equal(expected.Slot, device.Slot);
        Assert.False(item.CanRemove);
    }

    [Fact]
    public void ConfiguredCompatibleAdditionalDevicesRemainVisibleAndRemovable()
    {
        var extra = new AtariMediaConfiguration(AtariStorageSettingsTestConstants.FirstPath,
            AtariMediaKind.Floppy, EmulationMediaSlot.Floppy1);
        var view = AtariStorageSettingsFunctions.Create(
            new AtariMachineConfiguration(AtariMachineModel.St, media: [extra]));

        Assert.Equal(2, view.Devices.Count);
        Assert.False(view.Devices.Single(item => item.Configuration.Slot == EmulationMediaSlot.Floppy0).CanRemove);
        Assert.True(view.Devices.Single(item => item.Configuration.Slot == EmulationMediaSlot.Floppy1).CanRemove);
    }

    [Theory]
    [InlineData(AtariMachineModel.St, true)]
    [InlineData(AtariMachineModel.Atari800, true)]
    [InlineData(AtariMachineModel.Atari2600, false)]
    [InlineData(AtariMachineModel.JaguarCd, false)]
    public void AddIsOnlyAvailableWhenTheSelectedCoreReallySupportsAdditionalMedia(
        AtariMachineModel model, bool expected)
    {
        var view = AtariStorageSettingsFunctions.Create(new AtariMachineConfiguration(model));
        Assert.Equal(expected, AtariStorageSettingsFunctions.CanAdd(model, view));
    }

    [Fact]
    public void DuplicateIdentifierIsRejected()
    {
        var existing = new AtariMediaConfiguration(AtariStorageSettingsTestConstants.FirstPath,
            AtariMediaKind.Floppy, EmulationMediaSlot.Floppy0);
        var source = new AtariMachineConfiguration(AtariMachineModel.St, media: [existing]);
        var duplicate = new AtariMediaConfiguration(AtariStorageSettingsTestConstants.SecondPath,
            AtariMediaKind.Floppy, EmulationMediaSlot.Floppy0);

        Assert.Throws<InvalidOperationException>(() =>
            AtariStorageSettingsFunctions.AddOrReplace(source, duplicate, null));
    }

    [Fact]
    public void ConfigureAndRemovePreserveOtherConfigurationValues()
    {
        var source = new AtariMachineConfiguration(AtariMachineModel.Atari800,
            options: new Dictionary<string, string>
            {
                [AtariStorageSettingsTestConstants.OptionKey] = AtariStorageSettingsTestConstants.OptionValue
            });
        var media = new AtariMediaConfiguration(AtariStorageSettingsTestConstants.FirstPath,
            AtariMediaKind.Cassette, EmulationMediaSlot.Cassette0);

        var added = AtariStorageSettingsFunctions.AddOrReplace(source, media, null);
        var removed = AtariStorageSettingsFunctions.Remove(added, EmulationMediaSlot.Cassette0);

        Assert.Single(added.Media);
        Assert.Empty(removed.Media);
        Assert.Equal(source.Id, removed.Id);
        Assert.Equal(AtariStorageSettingsTestConstants.OptionValue,
            removed.Options[AtariStorageSettingsTestConstants.OptionKey]);
    }

    private static (AtariMediaKind Kind, EmulationMediaSlot Slot) ExpectedPrimaryDevice(
        AtariMachineModel model) => model switch
    {
        AtariMachineModel.JaguarCd => (AtariMediaKind.CompactDisc, EmulationMediaSlot.Cd0),
        AtariMachineModel.Atari2600 or AtariMachineModel.Atari5200 or AtariMachineModel.Atari7800
            or AtariMachineModel.Lynx or AtariMachineModel.Jaguar or AtariMachineModel.Xegs
            => (AtariMediaKind.Cartridge, EmulationMediaSlot.Cartridge0),
        _ => (AtariMediaKind.Floppy, EmulationMediaSlot.Floppy0)
    };
}

internal static class AtariStorageSettingsTestConstants
{
    internal const string FirstPath = "first.img";
    internal const string SecondPath = "second.img";
    internal const string OptionKey = "future_storage_option";
    internal const string OptionValue = "preserved";
}
