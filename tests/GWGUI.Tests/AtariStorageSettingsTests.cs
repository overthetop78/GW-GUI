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
}

internal static class AtariStorageSettingsTestConstants
{
    internal const string FirstPath = "first.img";
    internal const string SecondPath = "second.img";
    internal const string OptionKey = "future_storage_option";
    internal const string OptionValue = "preserved";
}
