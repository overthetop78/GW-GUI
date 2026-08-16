using GWGUI.App.Controls;
using GWGUI.Emulation.Atari;

namespace GWGUI.Tests;

public sealed class AtariGeneralSettingsTests
{
    [Fact]
    public void ReplacingGeneralSettingsPreservesFieldsOwnedByOtherPages()
    {
        var source = new AtariMachineConfiguration(AtariMachineModel.St,
            options: AtariGeneralSettingsTestConstants.UnknownOptions);
        var folders = AtariGeneralSettingsFunctions.DefaultFolders();
        var merged = AtariGeneralSettingsFunctions.MergeOptions(source.Options,
            AtariGeneralSettingsTestConstants.DisplayedOptions);

        var result = AtariGeneralSettingsFunctions.ReplaceGeneral(source, AtariMachineModel.St,
            folders, source.Firmwares, merged);

        Assert.Equal(source.Id, result.Id);
        Assert.Equal(source.Input, result.Input);
        Assert.Equal(source.Media, result.Media);
        Assert.Equal(AtariGeneralSettingsTestConstants.UnknownValue,
            result.Options[AtariGeneralSettingsTestConstants.UnknownKey]);
        Assert.Equal(AtariGeneralSettingsTestConstants.ChangedValue,
            result.Options[AtariGeneralSettingsTestConstants.DisplayedKey]);
    }

    [Fact]
    public void EveryGeneralFolderReceivesAnAtariDefault()
    {
        var folders = AtariGeneralSettingsFunctions.CompleteFolders(new AtariFolderConfiguration());

        Assert.All(new[] { folders.Shared, folders.Floppies, folders.Cassettes, folders.Cartridges,
            folders.CompactDiscs, folders.HardDisks, folders.States, folders.Captures },
            path => Assert.False(string.IsNullOrWhiteSpace(path)));
    }

    [Fact]
    public void CoreOptionHeadingKeepsItsAnnouncedCategory()
    {
        var option = new AtariCoreOption(AtariGeneralSettingsTestConstants.DisplayedKey,
            AtariGeneralSettingsTestConstants.OptionName, null,
            AtariGeneralSettingsTestConstants.OptionCategory,
            AtariGeneralSettingsTestConstants.OriginalValue,
            AtariGeneralSettingsTestConstants.OriginalValue, []);

        var heading = AtariGeneralSettingsFunctions.OptionHeading(option);

        Assert.Contains(AtariGeneralSettingsTestConstants.OptionCategory, heading);
        Assert.Contains(AtariGeneralSettingsTestConstants.OptionName, heading);
    }

    [Theory]
    [InlineData(AtariMachineModel.St, AtariCoreKind.Hatari)]
    [InlineData(AtariMachineModel.Atari800, AtariCoreKind.Atari800)]
    [InlineData(AtariMachineModel.Atari2600, AtariCoreKind.Stella)]
    [InlineData(AtariMachineModel.Atari7800, AtariCoreKind.ProSystem)]
    [InlineData(AtariMachineModel.Lynx, AtariCoreKind.BeetleLynx)]
    [InlineData(AtariMachineModel.Jaguar, AtariCoreKind.VirtualJaguar)]
    public void ModelDeterminesCore(AtariMachineModel model, AtariCoreKind expected) =>
        Assert.Equal(expected, new AtariMachineConfiguration(model).Core);
}

internal static class AtariGeneralSettingsTestConstants
{
    internal const string UnknownKey = "future_option";
    internal const string UnknownValue = "preserved";
    internal const string DisplayedKey = "known_option";
    internal const string OriginalValue = "before";
    internal const string ChangedValue = "after";
    internal const string OptionName = "Option name";
    internal const string OptionCategory = "category";
    internal static readonly IReadOnlyDictionary<string, string> UnknownOptions =
        new Dictionary<string, string>
        {
            [UnknownKey] = UnknownValue,
            [DisplayedKey] = OriginalValue
        };
    internal static readonly IReadOnlyList<KeyValuePair<string, string>> DisplayedOptions =
        [KeyValuePair.Create(DisplayedKey, ChangedValue)];
}
