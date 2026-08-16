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
    public void MainEmulationNavigationUsesOneConfigurationSelectorAndOneMachineTabControl()
    {
        RunOnSta(() =>
        {
            var app = Application.Current as GWGUI.App.App ?? new GWGUI.App.App();
            app.InitializeComponent();
            var section = new EmulationSection();
            var root = Assert.IsType<Grid>(section.Content);
            var selectors = Descendants(root).OfType<ComboBox>().ToArray();
            var machineTabs = Descendants(root).OfType<TabControl>().ToArray();

            Assert.Single(selectors);
            Assert.Single(machineTabs);
            Assert.DoesNotContain(Descendants(root), item => item is AmigaEmulationSection);
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
    public void DetailedErrorPresentationKeepsTheLocalizedDescription()
    {
        var expected = AtariEmulationSectionTestConstants.MissingMediaFileName;
        var result = ControlErrorPresenter.DescribeDetailed(new InvalidOperationException(expected),
            expected,
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

    private static IEnumerable<DependencyObject> Descendants(DependencyObject root)
    {
        yield return root;
        switch (root)
        {
            case Panel panel:
                foreach (UIElement child in panel.Children)
                    foreach (var descendant in Descendants(child)) yield return descendant;
                break;
            case Decorator decorator when decorator.Child is not null:
                foreach (var descendant in Descendants(decorator.Child)) yield return descendant;
                break;
            case ContentControl content when content.Content is DependencyObject child:
                foreach (var descendant in Descendants(child)) yield return descendant;
                break;
        }
    }
}
