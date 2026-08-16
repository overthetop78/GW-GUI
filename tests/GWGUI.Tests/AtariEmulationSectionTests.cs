using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using GWGUI.App.Controls;
using GWGUI.Emulation;
using GWGUI.Emulation.Atari;

namespace GWGUI.Tests;

[Collection(AtariNativeCoreTestConstants.CollectionName)]
public sealed class AtariEmulationSectionTests
{
    [Fact]
    public void MainEmulationNavigationContainsAmigaAndAtariSections()
    {
        RunOnSta(() =>
        {
            var app = Application.Current as GWGUI.App.App ?? new GWGUI.App.App();
            app.InitializeComponent();
            var section = new EmulationSection();
            var tabs = Assert.IsType<TabControl>(section.Content);

            Assert.Equal(AtariEmulationSectionTestConstants.FamilyTabCount, tabs.Items.Count);
            Assert.Single(tabs.Items.OfType<TabItem>(), item => item.Content is AmigaEmulationSection);
            Assert.Single(tabs.Items.OfType<TabItem>(), item => item.Content is AtariEmulationSection);
        });
    }

    [Fact]
    public void MissingRequiredFirmwareReportsItsExactRoleAndModel()
    {
        var configuration = new AtariMachineConfiguration(AtariMachineModel.Lynx);

        var error = Assert.Throws<AtariEmulationException>(() =>
            AtariEmulationFunctions.ValidateConfiguration(configuration));

        Assert.Equal(AtariErrorCode.FirmwareMissing, error.Code);
        Assert.Contains(nameof(AtariFirmwareKind.LynxBootRom), error.Message, StringComparison.Ordinal);
        Assert.Contains(nameof(AtariMachineModel.Lynx), error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MissingConfiguredFirmwareReportsItsPath()
    {
        var missing = Path.Combine(Path.GetTempPath(),
            Guid.NewGuid().ToString(AtariEmulationSectionTestConstants.IdentifierFormat),
            AtariEmulationSectionTestConstants.MissingFirmwareFileName);
        var configuration = new AtariMachineConfiguration(AtariMachineModel.Lynx,
            [new AtariFirmwareConfiguration(AtariFirmwareKind.LynxBootRom, missing, true)]);

        var error = Assert.Throws<AtariEmulationException>(() =>
            AtariEmulationFunctions.ValidateConfiguration(configuration));

        Assert.Equal(AtariErrorCode.FirmwareMissing, error.Code);
        Assert.Contains(missing, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MissingConfiguredMediaReportsItsPath()
    {
        var missing = Path.Combine(Path.GetTempPath(),
            Guid.NewGuid().ToString(AtariEmulationSectionTestConstants.IdentifierFormat),
            AtariEmulationSectionTestConstants.MissingMediaFileName);
        var configuration = new AtariMachineConfiguration(AtariMachineModel.Atari2600, media:
        [
            new AtariMediaConfiguration(missing, AtariMediaKind.Cartridge, EmulationMediaSlot.Cartridge0)
        ]);

        var error = Assert.Throws<AtariEmulationException>(() =>
            AtariEmulationFunctions.ValidateConfiguration(configuration));

        Assert.Equal(AtariErrorCode.ContentNotFound, error.Code);
        Assert.Contains(missing, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ConfigurationWithoutRequiredExternalFilesIsAccepted()
    {
        AtariEmulationFunctions.ValidateConfiguration(
            new AtariMachineConfiguration(AtariMachineModel.Atari2600));
    }

    [Fact]
    public void DetailedErrorPresentationKeepsTheOriginalErrorMessage()
    {
        var expected = AtariEmulationSectionTestConstants.MissingMediaFileName;
        var result = ControlErrorPresenter.DescribeDetailed(new InvalidOperationException(expected),
            AtariEmulationConstants.ConfigurationOpeningContext);

        Assert.Contains(expected, result, StringComparison.Ordinal);
    }

    private static void RunOnSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception error) { failure = error; }
            finally { Dispatcher.CurrentDispatcher.InvokeShutdown(); }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(AtariEmulationSectionTestConstants.StaTimeoutMilliseconds));
        if (failure is not null) throw failure;
    }
}
