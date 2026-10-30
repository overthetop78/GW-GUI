using GWGUI.App.Constants.Emulation;
using GWGUI.App.Functions.Views.Emulation.Settings;
using GWGUI.App.Constants.Localization;
using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Contracts.Storage;
using GWGUI.App.Views.Controls.Emulation.Storage;
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
    public string? CreatedMediaToMount { get; private set; }

    private readonly ComboBox _model = new();
    private readonly FrameworkElement _speed;
    private readonly CheckBox _writeProtected = new();
    private readonly CheckBox _redirectWrites = new();
    private readonly FloppyDriveDialogOptions _options;
    private readonly Guid _clientGuid;
    private readonly Dictionary<string, FrameworkElement> _fields = new(StringComparer.Ordinal);

    public FloppyDriveSettings Settings => new((_model.SelectedItem as StorageDialogChoice)?.Value ?? "35dd", StorageConfigurationFieldFunctions.Read(_speed),
        _options.CanConfigureWriteProtection && _writeProtected.IsChecked == true, _options.CanRedirectWrites && _redirectWrites.IsChecked == true, _fields.ToDictionary(item => item.Key, item => StorageConfigurationFieldFunctions.Read(item.Value), StringComparer.Ordinal));

    public FloppyDriveConfigurationDialog(string identifier, string machineName, FloppyDriveSettings settings, FloppyDriveDialogOptions options, Guid clientGuid,
        string? moduleId = null, IReadOnlyList<EmulationSettingsField>? configurationFields = null)
    {
        _options = options;
        _clientGuid = clientGuid;
        Title = $"{LocExtension.Get(EmulationResourceKeys.StorageDeviceConfigure)} {identifier}";
        Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive);
        ShowInTaskbar = false;
        StorageDialogUi.CompactAppearance(this);
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Width = Math.Min(StorageConfigurationLayoutConstants.FloppyWidth, SystemParameters.WorkArea.Width * StorageConfigurationLayoutConstants.WorkAreaWidthRatio);
        MinWidth = StorageConfigurationLayoutConstants.MinimumWidth;
        MaxHeight = Math.Min(StorageConfigurationLayoutConstants.CompactMaximumHeight, SystemParameters.WorkArea.Height * StorageConfigurationLayoutConstants.WorkAreaHeightRatio);
        MinHeight = StorageConfigurationLayoutConstants.CompactMinimumHeight;
        SizeToContent = SizeToContent.Height;
        ResizeMode = ResizeMode.CanResize;

        _model.ItemsSource = options.Models.Select(choice =>
            new StorageDialogChoice(choice.Value, FloppyChoicePresentation.Model(choice, moduleId))).ToArray();
        ComboBoxSelection.SelectByValue<StorageDialogChoice>(_model, settings.Model, choice => choice.Value);
        _speed = StorageConfigurationFieldFunctions.Create(new EmulationSettingsField(
            nameof(_speed), EmulationMachineTab.Storage, string.Empty, "Emulation.Storage.Floppy.Speed",
            EmulationSettingsEditor.Selection, settings.Speed, options.SpeedChoices ?? EmulationDriveSpeedChoices.Standard,
            UseRadioButtons: true), moduleId, null);
        _writeProtected.Content = LocExtension.Get("Emulation.Storage.Floppy.WriteProtection");
        _writeProtected.IsChecked = settings.WriteProtected;
        _redirectWrites.Content = LocExtension.Get("Emulation.Storage.Floppy.WriteRedirect");
        _redirectWrites.IsChecked = settings.RedirectWrites;

        StorageDialogUi.WrapChoices(_model, nameof(StorageDialogChoice.Text));
        var drive = new StackPanel();
        if (options.Models.Count > 0)
        {
            drive.Children.Add(new TextBlock { Text = LocExtension.Get(EmulationResourceKeys.Model), Margin = new Thickness(0, 0, 0, 5) });
            _model.Margin = new Thickness(0, 0, 0, 8);
            drive.Children.Add(_model);
        }
        if (options.CanConfigureBehavior)
        {
            drive.Children.Add(new TextBlock { Text = LocExtension.Get("Emulation.Storage.Floppy.Speed"), TextWrapping = TextWrapping.Wrap });
            drive.Children.Add(_speed);
            _writeProtected.Margin = new Thickness(0, 8, 0, 4);
            if (options.CanConfigureWriteProtection) drive.Children.Add(_writeProtected);
            if (options.CanRedirectWrites) drive.Children.Add(_redirectWrites);
        }
        var body = new StackPanel();
        var driveCard = StorageDialogUi.IconCard(IconGlyphs.Memory, LocExtension.Get("Emulation.Storage.Floppy.Drive"), drive, compact: true);
        var groups = StorageConfigurationFieldFunctions.CreateGroups(configurationFields ?? [], moduleId, settings.Options, _fields, compact: true);
        body.Children.Add(groups.Children.Count == 0 ? driveCard : StorageDialogUi.CompactColumns(driveCard, groups));
        var actions = new DockPanel { LastChildFill = false };
        var footer = StorageDialogUi.Footer(this, LocExtension.Get("Common.Save"), compact: true);
        DockPanel.SetDock(footer, Dock.Right); actions.Children.Add(footer);
        if (options.CanCreateBlankMedia)
        {
            var create = new Button
            {
                Content = LocExtension.Get("Emulation.Storage.Floppy.CreateBlank"),
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 4, 8, 4),
                VerticalAlignment = VerticalAlignment.Center
            };
            create.Click += (_, _) =>
            {
                create.IsEnabled = false;
                try { CreateBlankFloppy(); }
                catch (Exception error) when (error is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
                { MessageBox.Show(this, error.Message, Title, MessageBoxButton.OK, MessageBoxImage.Warning); }
                finally { create.IsEnabled = true; }
            };
            actions.Children.Add(create);
        }

        var root = StorageDialogUi.DialogLayout(StorageDialogUi.DialogHeader(IconGlyphs.Memory, Title, $"{LocExtension.Get(EmulationResourceKeys.FloppyDevice)} · {machineName}", compact: true), new ScrollViewer { Content = body, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, MaxHeight = SystemParameters.WorkArea.Height * StorageConfigurationLayoutConstants.BodyHeightRatio }, actions);
        root.Margin = new Thickness(12);
        root.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);
        Content = root;
    }

    private void CreateBlankFloppy(FloppyDriveModelChoice model)
    {
        var configuration = new FloppyCreationSettingsView(model, _options, _clientGuid);
        Task CreateImage()
        {
            var selected = configuration.SelectedFormat ?? throw new InvalidOperationException(LocExtension.Get("Emulation.Hdd.InvalidFormat"));
            return EmulationSdk.MediaProcessing.CreateBlankDiskAsync(configuration.DestinationPath, selected.FormatId);
        }
        var creation = new MediaCreationDialog(this, LocExtension.Get("Emulation.Storage.Floppy.CreateBlank"), configuration,
            () => configuration.DestinationPath, CreateImage, () => configuration.IsComplete);
        try
        {
            if (creation.ShowDialog() == true)
            {
                CreatedMediaToMount = creation.CreatedPath;
                DialogResult = true;
            }
        }
        finally { creation.Close(); }
    }

    private void CreateBlankFloppy()
    {
        var model = _options.Models.FirstOrDefault(candidate => candidate.Value == Settings.Model);
        if (model?.CreationFormats.Count > 0)
        {
            CreateBlankFloppy(model);
            return;
        }
        var formats = model?.BlankFormats;
        var extension = formats?.Keys.FirstOrDefault() ?? _options.DefaultExtension;
        var dialog = new SaveFileDialog
        {
            Filter = formats?.Count > 0 ? string.Join("|", formats.Keys.Select(suffix => $"{suffix.TrimStart('.').ToUpperInvariant()}|*{suffix}")) : _options.ImageFilter,
            DefaultExt = extension,
            AddExtension = true,
            InitialDirectory = Directory.Exists(_options.ImageDirectory) ? _options.ImageDirectory : string.Empty,
            ClientGuid = _clientGuid,
            FileName = $"blank{extension}"
        };
        if (dialog.ShowDialog(this) != true) return;
        var creation = new MediaCreationDialog(this, LocExtension.Get("Emulation.Storage.Floppy.CreateBlank"), dialog.FileName, async () =>
        {
            if (formats?.Count > 0)
            {
                var choice = formats.FirstOrDefault(format => format.Key.Equals(Path.GetExtension(dialog.FileName), StringComparison.OrdinalIgnoreCase));
                if (choice.Value is null) throw new ArgumentException(LocExtension.Get("Emulation.Hdd.InvalidFormat"));
                await EmulationSdk.MediaProcessing.CreateBlankDiskAsync(dialog.FileName, choice.Value);
            }
            else
            {
                using var stream = new FileStream(dialog.FileName, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                if (model?.BlankImageSize is > 0) stream.SetLength(model.BlankImageSize);
            }
        });
        try
        {
            if (creation.ShowDialog() == true)
            {
                CreatedMediaToMount = dialog.FileName;
                DialogResult = true;
            }
        }
        finally { creation.Close(); }
    }
}
