using GWGUI.App.Constants.Localization;
using GWGUI.App.Contracts.Storage;
using GWGUI.App.Functions.Views.Emulation.Storage;
using GWGUI.App.Localization.Extensions;
using System.Windows;
using System.Windows.Controls;

namespace GWGUI.App.Views.Dialogs.Emulation.Storage;

public sealed class CompactDiscDriveConfigurationDialog : Window
{
    private readonly ComboBox _model = new();
    private readonly ComboBox _speed = new();

    public CompactDiscDriveSettings Settings => new(
        _model.SelectedItem?.ToString() ?? "CD-ROM",
        (_speed.SelectedItem as StorageDialogChoice)?.Value ?? "100");

    public CompactDiscDriveConfigurationDialog(string identifier, string machineName,
        CompactDiscDriveSettings settings, bool supportsWriter)
    {
        Title = $"{LocExtension.Get(EmulationResourceKeys.StorageDeviceConfigure)} {identifier}";
        Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive);
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Width = 720;
        SizeToContent = SizeToContent.Height;
        ResizeMode = ResizeMode.NoResize;
        _model.ItemsSource = supportsWriter
            ? new[] { "CD-ROM", LocExtension.Get("Emulation.Storage.Cd.Writer") }
            : new[] { "CD-ROM" };
        _model.SelectedIndex = 0;
        _speed.ItemsSource = new[]
        {
            new StorageDialogChoice("100", "1×"),
            new StorageDialogChoice("0", LocExtension.Get("Emulation.Value.Maximum"))
        };
        _speed.SelectedItem = _speed.Items.OfType<StorageDialogChoice>()
            .FirstOrDefault(choice => choice.Value == settings.Speed) ?? _speed.Items[0];
        var fields = StorageDialogUi.TwoColumnFields(
            (LocExtension.Get(EmulationResourceKeys.DeviceIdentifier),
                new TextBox { Text = identifier, IsReadOnly = true }),
            (LocExtension.Get(EmulationResourceKeys.Model), _model),
            (LocExtension.Get("Emulation.Storage.Cd.Speed"), _speed));
        var body = new StackPanel();
        body.Children.Add(fields);
        body.Children.Add(StorageDialogUi.Info(supportsWriter
            ? LocExtension.Get("Emulation.Storage.Cd.WriterHint")
            : LocExtension.Get(EmulationResourceKeys.StorageRuntimeHint)));
        Content = StorageDialogUi.DialogLayout(
            StorageDialogUi.DialogHeader("\uE958", Title,
                $"{LocExtension.Get(EmulationResourceKeys.CompactDiscDevice)} · {machineName}"),
            StorageDialogUi.Card(LocExtension.Get(EmulationResourceKeys.CompactDiscDevice), body),
            StorageDialogUi.Footer(this, LocExtension.Get("Common.Save")));
    }
}
