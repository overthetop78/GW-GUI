using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Localization;
using GWGUI.Emulation;

namespace GWGUI.App.Controls;

public sealed class AddStorageDeviceDialog : Window
{
    private readonly ComboBox _type = new();

    public EmulationMediaType SelectedType =>
        _type.SelectedItem is StorageDeviceChoice choice ? choice.Type : EmulationMediaType.Floppy;

    public AddStorageDeviceDialog(IEnumerable<EmulationMediaType> availableTypes)
    {
        Title = LocExtension.Get(EmulationResourceKeys.StorageDeviceAdd);
        Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive);
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        SizeToContent = SizeToContent.WidthAndHeight;
        ResizeMode = ResizeMode.NoResize;
        MinWidth = 460;
        _type.ItemsSource = availableTypes.Select(type =>
            new StorageDeviceChoice(type, DeviceLabel(type))).ToArray();
        _type.SelectedIndex = 0;
        Content = StorageDialogUi.DialogLayout(
            StorageDialogUi.Card(LocExtension.Get(EmulationResourceKeys.DeviceType),
                StorageDialogUi.Field(LocExtension.Get(EmulationResourceKeys.DeviceType), _type)),
            StorageDialogUi.Footer(this, LocExtension.Get(EmulationResourceKeys.StorageDeviceAdd)));
    }

    private static string DeviceLabel(EmulationMediaType type) => type switch
    {
        EmulationMediaType.Floppy => LocExtension.Get(EmulationResourceKeys.FloppyDevice),
        EmulationMediaType.HardDisk => LocExtension.Get(EmulationResourceKeys.HardDiskDevice),
        EmulationMediaType.CompactDisc => LocExtension.Get(EmulationResourceKeys.CompactDiscDevice),
        EmulationMediaType.Cassette => LocExtension.Get(EmulationResourceKeys.CassetteDevice),
        EmulationMediaType.Cartridge => LocExtension.Get(EmulationResourceKeys.CartridgeDevice),
        _ => type.ToString()
    };
}
