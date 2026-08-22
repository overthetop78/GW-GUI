using GWGUI.App.Constants.Localization;
using GWGUI.App.Contracts.Storage;
using GWGUI.App.Functions.Views.Common;
using GWGUI.App.Functions.Views.Emulation.Storage;
using GWGUI.App.Localization.Extensions;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using GWGUI.Emulation;
using Microsoft.Win32;


namespace GWGUI.App.Views.Dialogs.Emulation.Storage;

public sealed class FloppyDriveConfigurationDialog : Window
{
    private readonly ComboBox _model = new();
    private readonly ComboBox _speed = new();
    private readonly CheckBox _writeProtected = new();
    private readonly CheckBox _redirectWrites = new();
    private readonly FloppyDriveDialogOptions _options;

    public FloppyDriveSettings Settings => new(
        (_model.SelectedItem as StorageDialogChoice)?.Value ?? "35dd",
        (_speed.SelectedItem as StorageDialogChoice)?.Value ?? "100",
        _writeProtected.IsChecked == true,
        _redirectWrites.IsChecked == true);

    public FloppyDriveConfigurationDialog(string identifier, string machineName,
        FloppyDriveSettings settings, FloppyDriveDialogOptions options)
    {
        _options = options;
        Title = $"{LocExtension.Get(EmulationResourceKeys.StorageDeviceConfigure)} {identifier}";
        Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive);
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Width = 820;
        SizeToContent = SizeToContent.Height;
        ResizeMode = ResizeMode.NoResize;

        _model.ItemsSource = options.Models.Select(choice =>
            new StorageDialogChoice(choice.Value, string.IsNullOrWhiteSpace(choice.DisplayResourceKey)
                ? choice.InvariantDisplayValue ?? choice.Value
                : LocExtension.Get(choice.DisplayResourceKey))).ToArray();
        ComboBoxSelection.SelectByValue<StorageDialogChoice>(
            _model, settings.Model, choice => choice.Value);
        _speed.ItemsSource = new[]
        {
            new StorageDialogChoice("100", "100 %"), new StorageDialogChoice("200", "200 %"),
            new StorageDialogChoice("400", "400 %"), new StorageDialogChoice("800", "800 %"),
            new StorageDialogChoice("0", LocExtension.Get("Emulation.Value.Maximum"))
        };
        ComboBoxSelection.SelectByValue<StorageDialogChoice>(
            _speed, settings.Speed, choice => choice.Value);
        _writeProtected.Content = LocExtension.Get("Emulation.Storage.Floppy.WriteProtection");
        _writeProtected.IsChecked = settings.WriteProtected;
        _redirectWrites.Content = LocExtension.Get("Emulation.Storage.Floppy.WriteRedirect");
        _redirectWrites.IsChecked = settings.RedirectWrites;

        var drive = StorageDialogUi.CompactFields(
            (LocExtension.Get(EmulationResourceKeys.DeviceIdentifier),
                new TextBox { Text = identifier, IsReadOnly = true }),
            (LocExtension.Get(EmulationResourceKeys.Model), _model));
        var behavior = new StackPanel();
        behavior.Children.Add(StorageDialogUi.CompactFields(
            (LocExtension.Get("Emulation.Storage.Floppy.Speed"), _speed)));
        _writeProtected.Margin = new Thickness(0, 8, 0, 4);
        _redirectWrites.Margin = new Thickness(0, 4, 0, 4);
        behavior.Children.Add(_writeProtected);
        behavior.Children.Add(_redirectWrites);
        var body = new StackPanel();
        body.Children.Add(StorageDialogUi.SideBySide(
            StorageDialogUi.IconCard("\uE964", LocExtension.Get("Emulation.Storage.Floppy.Drive"), drive),
            StorageDialogUi.IconCard("\uE713", LocExtension.Get("Emulation.Input.Behavior"), behavior)));
        if (options.CanCreateBlankMedia)
        {
            var create = new Button
            {
                Content = LocExtension.Get("Emulation.Storage.Floppy.CreateBlank"),
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 14, 0, 0)
            };
            create.Click += (_, _) => CreateBlankFloppy();
            var blankMedia = new StackPanel();
            blankMedia.Children.Add(create);
            blankMedia.Children.Add(StorageDialogUi.Info(
                LocExtension.Get(EmulationResourceKeys.StorageRuntimeHint)));
            body.Children.Add(StorageDialogUi.IconCard("\uE7C3",
                LocExtension.Get("Emulation.Storage.Media.Blank"), blankMedia));
        }

        Content = StorageDialogUi.DialogLayout(
            StorageDialogUi.DialogHeader("\uE964", Title,
                $"{LocExtension.Get(EmulationResourceKeys.FloppyDevice)} · {machineName}"),
            body,
            StorageDialogUi.Footer(this, LocExtension.Get("Common.Save")));
    }

    private void CreateBlankFloppy()
    {
        Directory.CreateDirectory(_options.ImageDirectory);
        var dialog = new SaveFileDialog
        {
            Filter = _options.ImageFilter,
            DefaultExt = _options.DefaultExtension,
            AddExtension = true,
            InitialDirectory = _options.ImageDirectory,
            FileName = $"blank{_options.DefaultExtension}"
        };
        if (dialog.ShowDialog(this) != true) return;
        using var stream = new FileStream(
            dialog.FileName, FileMode.Create, FileAccess.Write, FileShare.None);
        var size = _options.Models.FirstOrDefault(model =>
            model.Value == Settings.Model)?.BlankImageSize ?? 0;
        if (size > 0) stream.SetLength(size);
        MessageBox.Show(this,
            LocExtension.Get("Emulation.Storage.Floppy.BlankCreated", dialog.FileName), Title,
            MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
