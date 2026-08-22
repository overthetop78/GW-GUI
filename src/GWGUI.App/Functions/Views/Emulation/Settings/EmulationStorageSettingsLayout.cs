using GWGUI.App.Constants.Localization;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Views.Controls.Emulation.Storage;
using System.Windows;
using System.Windows.Controls;


namespace GWGUI.App.Functions.Views.Emulation.Settings;

/// <summary>
/// Shared storage page. Each machine editor supplies a device list already populated
/// with the devices, models, media and permissions supported by the selected machine.
/// </summary>
internal static partial class EmulationSettingsLayout
{
    internal static ScrollViewer StorageSettingsPage(EmulationStorageDeviceList devices,
        UIElement? emulatorOptions = null)
    {
        var content = new StackPanel();
        content.Children.Add(devices);
        content.Children.Add(InformationBanner(LocExtension.Get(EmulationResourceKeys.StorageRuntimeHint)));
        if (emulatorOptions is not null) content.Children.Add(emulatorOptions);

        var card = ActionCard(content, LocExtension.Get(EmulationResourceKeys.StorageDeviceList));
        card.Margin = new Thickness(0, 0, 0, 8);
        var page = new Grid { Margin = new Thickness(12) };
        page.Children.Add(card);
        return ScrollPage(page);
    }
}
