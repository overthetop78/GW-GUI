using GWGUI.App.Controls;
using GWGUI.App.Localization;
using GWGUI.Emulation;
using GWGUI.Emulation.Atari;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

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
        if (model == AtariMachineModel.Atari400)
        {
            Assert.Empty(view.Devices);
            return;
        }
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

    [Fact]
    public void Atari400StartsWithoutAnInventedDriveAndCanAddItsPhysicalDevices()
    {
        var source = new AtariMachineConfiguration(AtariMachineModel.Atari400);
        var initial = AtariStorageSettingsFunctions.Create(source);
        Assert.Empty(initial.Devices);
        Assert.True(AtariStorageSettingsFunctions.CanAdd(source.Model, initial));
        Assert.Contains(initial.Types, type => type.Kind == AtariMediaKind.Floppy);
        Assert.Contains(initial.Types, type => type.Kind == AtariMediaKind.Cassette);
        Assert.Contains(initial.Types, type => type.Kind == AtariMediaKind.Cartridge);

        var withDrive = AtariStorageSettingsFunctions.AddDevice(source,
            AtariMediaKind.Floppy, EmulationMediaSlot.Floppy0);
        var drive = Assert.Single(AtariStorageSettingsFunctions.Create(withDrive).Devices);
        Assert.Equal("D1:", drive.Identifier);
        Assert.True(drive.CanRemove);
        Assert.StartsWith("Atari 8-bit", drive.Model, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(AtariMachineModel.St, "A:", "Format.atarist.720")]
    [InlineData(AtariMachineModel.Atari800, "D1:", "Format.atari.90")]
    public void PrimaryFloppyUsesMachineIdentifierAndActualDriveModel(
        AtariMachineModel model, string identifier, string modelResource)
    {
        var device = Assert.Single(AtariStorageSettingsFunctions.Create(
            new AtariMachineConfiguration(model)).Devices);

        Assert.Equal(identifier, device.Identifier);
        Assert.Equal(LocExtension.Get(modelResource), device.Model);
    }

    [Fact]
    public void ConfiguredSecondStFloppyUsesDriveBAndSelectedCapacity()
    {
        var source = AtariStorageSettingsFunctions.AddDevice(
            new AtariMachineConfiguration(AtariMachineModel.Falcon),
            AtariMediaKind.Floppy, EmulationMediaSlot.Floppy1);
        source = AtariStorageSettingsFunctions.ConfigureFloppy(source, EmulationMediaSlot.Floppy1,
            new FloppyDriveSettings("atarist.1440", "100", false, false));

        var device = AtariStorageSettingsFunctions.Create(source).Devices.Single(item => item.CanRemove);

        Assert.Equal("B:", device.Identifier);
        Assert.Equal(LocExtension.Get("Format.atarist.1440"), device.Model);
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

    [Fact]
    public void Atari400PeripheralAndOsdOptionsPersistWhileStDoesNotExposeThem()
    {
        WpfTestHost.Run(() =>
        {
            var app = Application.Current as GWGUI.App.App ?? new GWGUI.App.App();
            app.InitializeComponent();
            var section = new AtariStorageSettingsSection();
            var source = new AtariMachineConfiguration(AtariMachineModel.Atari400);
            section.Load(source);

            CheckBox(section, "_showSpeedOsd").IsChecked = true;
            CheckBox(section, "_showSectorOsd").IsChecked = true;
            CheckBox(section, "_realTimeClock").IsChecked = true;
            CheckBox(section, "_printerDevice").IsChecked = true;
            CheckBox(section, "_serialDevice").IsChecked = true;
            var saved = section.Apply(source);

            Assert.Equal(AtariEightBitSettingsConstants.Enabled,
                saved.Options[AtariEightBitSettingsConstants.ShowSpeedOptionKey]);
            Assert.Equal(AtariEightBitSettingsConstants.Enabled,
                saved.Options[AtariEightBitSettingsConstants.ShowSectorOptionKey]);
            Assert.Equal(AtariEightBitSettingsConstants.Enabled,
                saved.Options[AtariEightBitSettingsConstants.RealTimeClockOptionKey]);
            Assert.Equal(AtariEightBitSettingsConstants.Enabled,
                saved.Options[AtariEightBitSettingsConstants.PrinterDeviceOptionKey]);
            Assert.Equal(AtariEightBitSettingsConstants.Enabled,
                saved.Options[AtariEightBitSettingsConstants.SerialDeviceOptionKey]);

            section.Load(new AtariMachineConfiguration(AtariMachineModel.St));
            Assert.Equal(Visibility.Collapsed, CheckBox(section, "_showSpeedOsd").Visibility);
            Assert.Equal(Visibility.Collapsed, CheckBox(section, "_realTimeClock").Visibility);
            Assert.Equal(Visibility.Collapsed, CheckBox(section, "_printerDevice").Visibility);
        });
    }

    private static CheckBox CheckBox(AtariStorageSettingsSection section, string field) =>
        Assert.IsType<CheckBox>(typeof(AtariStorageSettingsSection).GetField(field,
            BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(section));

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
