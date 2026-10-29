using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using GWGUI.App.Constants.Emulation;
using GWGUI.App.Contracts.Storage;
using GWGUI.App.Functions.Views.Emulation.Storage;
using GWGUI.App.Localization.Extensions;
using GWGUI.Emulation.Contracts;
using Microsoft.Win32;

namespace GWGUI.App.Views.Controls.Emulation.Storage;

/// <summary>Sépare le contenu de la disquette, son conteneur et le fichier de destination.</summary>
internal sealed class FloppyCreationSettingsView : Border
{
    private readonly ComboBox _disk = new();
    private readonly ComboBox _image = new() { FlowDirection = FlowDirection.LeftToRight };
    private readonly TextBox _path = new() { IsReadOnly = true, MinHeight = 32, VerticalContentAlignment = VerticalAlignment.Center, FlowDirection = FlowDirection.LeftToRight };
    private readonly IReadOnlyDictionary<string, FloppyCreationFormatChoice[]> _groups;

    internal FloppyCreationFormatChoice? SelectedFormat => _image.SelectedItem as FloppyCreationFormatChoice;
    internal string DestinationPath => _path.Text;
    internal bool IsComplete => SelectedFormat is not null && !string.IsNullOrWhiteSpace(DestinationPath);

    internal FloppyCreationSettingsView(FloppyDriveModelChoice model, FloppyDriveDialogOptions options, Guid clientGuid)
    {
        SetResourceReference(StyleProperty, "Card");
        Padding = new Thickness(16);
        _groups = model.CreationFormats.GroupBy(format => (format.DisplayName, format.Tracks, format.Sides, format.DoubleDensity, format.Capacity))
            .ToDictionary(group => group.First().FormatId, group => group.ToArray(), StringComparer.Ordinal);
        _disk.ItemsSource = _groups.Values.Select(group => new StorageDialogChoice(group[0].FormatId, FloppyChoicePresentation.DiskFormat(group[0]))).ToArray();
        StorageDialogUi.WrapChoices(_disk, nameof(StorageDialogChoice.Text));
        StorageDialogUi.WrapChoices(_image, nameof(FloppyCreationFormatChoice.DisplayName));
        _path.SetBinding(ToolTipProperty, new Binding(nameof(TextBox.Text)) { Source = _path });

        var fields = new StackPanel();
        AddField(MediaCreationConstants.FormatLabel, _disk);
        AddField(MediaCreationConstants.ImageFormatLabel, _image);
        var destination = new Grid();
        destination.ColumnDefinitions.Add(new ColumnDefinition());
        destination.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        destination.Children.Add(_path);
        var browse = new Button { Content = LocExtension.Get("Common.Browse"), Margin = new Thickness(10, 0, 0, 0), MinWidth = 100 };
        Grid.SetColumn(browse, 1); destination.Children.Add(browse);
        browse.Click += (_, _) => Browse();
        AddField(MediaCreationConstants.DestinationLabel, destination, last: true);
        Child = fields;

        _disk.SelectionChanged += (_, _) => RefreshImages();
        _image.SelectionChanged += (_, _) => RefreshExtension();
        var initial = _groups.Values.Select(group => group[0]).FirstOrDefault(format =>
            format.Tracks == model.Tracks && format.Sides == model.Sides && format.DoubleDensity == model.DoubleDensity)
            ?? model.CreationFormats.OrderByDescending(format => format.Capacity).First();
        _disk.SelectedItem = _disk.Items.Cast<StorageDialogChoice>().Single(choice => choice.Value == initial.FormatId);

        void AddField(string resource, FrameworkElement editor, bool last = false)
        {
            fields.Children.Add(new TextBlock { Text = LocExtension.Get(resource), FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 6) });
            editor.Margin = new Thickness(0, 0, 0, last ? 0 : 16);
            fields.Children.Add(editor);
        }

        void Browse()
        {
            var selected = SelectedFormat;
            var formats = selected is not null ? [selected] : _image.Items.Cast<FloppyCreationFormatChoice>().ToArray();
            var extensions = formats.Select(format => format.Extension).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            var dialog = new SaveFileDialog
            {
                Filter = string.Join("|", extensions.Select(extension => $"{extension.TrimStart('.').ToUpperInvariant()} (*{extension})|*{extension}")),
                DefaultExt = selected?.Extension ?? string.Empty,
                AddExtension = selected is not null,
                InitialDirectory = Directory.Exists(options.ImageDirectory) ? options.ImageDirectory : string.Empty,
                ClientGuid = clientGuid,
                FileName = string.IsNullOrWhiteSpace(DestinationPath) ? MediaCreationConstants.BlankFileName + selected?.Extension : Path.GetFileName(DestinationPath)
            };
            if (dialog.ShowDialog(Window.GetWindow(this)) == true) _path.Text = dialog.FileName;
        }
    }

    private void RefreshImages()
    {
        var previous = SelectedFormat;
        var formats = _disk.SelectedItem is StorageDialogChoice disk ? _groups[disk.Value] : [];
        _image.ItemsSource = formats.Select(FloppyChoicePresentation.ImageFormat).ToArray();
        _image.SelectedItem = _image.Items.Cast<FloppyCreationFormatChoice>().FirstOrDefault(format => format.Extension == previous?.Extension)
            ?? (_image.Items.Count == 1 ? _image.Items[0] : null);
        RefreshExtension();
    }

    private void RefreshExtension()
    {
        if (SelectedFormat is { } format && !string.IsNullOrWhiteSpace(DestinationPath))
            _path.Text = Path.ChangeExtension(DestinationPath, format.Extension);
    }
}
