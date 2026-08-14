using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GWGUI.App.Localization;
using Microsoft.Win32;

namespace GWGUI.App.Controls;

public sealed record FloppyDriveSettings(string Model, string Speed, bool WriteProtected, bool RedirectWrites);
public sealed record CompactDiscDriveSettings(string Model, string Speed);

public sealed class AddStorageDeviceDialog : Window
{
    private readonly ComboBox _type = new();
    public EmulationStorageDeviceType SelectedType =>
        _type.SelectedItem is StorageDeviceChoice choice ? choice.Type : EmulationStorageDeviceType.Floppy;

    public AddStorageDeviceDialog(IEnumerable<EmulationStorageDeviceType> availableTypes)
    {
        Title = LocExtension.Get("Emulation.AddStorageDevice");
        Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive);
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        SizeToContent = SizeToContent.WidthAndHeight;
        ResizeMode = ResizeMode.NoResize;
        MinWidth = 460;
        _type.ItemsSource = availableTypes.Select(type => new StorageDeviceChoice(type, DeviceLabel(type))).ToArray();
        _type.SelectedIndex = 0;
        Content = StorageDialogUi.DialogLayout(
            StorageDialogUi.Card(LocExtension.Get("Emulation.Type"),
                StorageDialogUi.Field(LocExtension.Get("Emulation.Type"), _type)),
            StorageDialogUi.Footer(this, LocExtension.Get("Emulation.AddStorageDevice")));
    }

    private static string DeviceLabel(EmulationStorageDeviceType type) => type switch
    {
        EmulationStorageDeviceType.Floppy => LocExtension.Get("Emulation.FloppyDevice"),
        EmulationStorageDeviceType.HardDisk => LocExtension.Get("Emulation.HardDiskDevice"),
        EmulationStorageDeviceType.CompactDisc => LocExtension.Get("Emulation.CompactDiscDevice"),
        EmulationStorageDeviceType.Zip => "ZIP",
        EmulationStorageDeviceType.Tape => LocExtension.Get("Emulation.TapeDevice"),
        _ => type.ToString()
    };

    private sealed record StorageDeviceChoice(EmulationStorageDeviceType Type, string Text)
    {
        public override string ToString() => Text;
    }
}

public sealed class FloppyDriveConfigurationDialog : Window
{
    private readonly ComboBox _model = new();
    private readonly ComboBox _speed = new();
    private readonly CheckBox _writeProtected = new();
    private readonly CheckBox _redirectWrites = new();

    public FloppyDriveSettings Settings => new(
        (_model.SelectedItem as Choice)?.Value ?? "35dd",
        (_speed.SelectedItem as Choice)?.Value ?? "100",
        _writeProtected.IsChecked == true,
        _redirectWrites.IsChecked == true);

    public FloppyDriveConfigurationDialog(string identifier, string machineName, FloppyDriveSettings settings)
    {
        Title = $"{LocExtension.Get("Emulation.ConfigureDevice")} {identifier}";
        Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive);
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Width = 760;
        SizeToContent = SizeToContent.Height;
        ResizeMode = ResizeMode.NoResize;

        _model.ItemsSource = new[]
        {
            new Choice("35dd", LocExtension.Get("Emulation.AmigaDdFloppy")),
            new Choice("35hd", LocExtension.Get("Emulation.AmigaHdFloppy"))
        };
        Select(_model, settings.Model);
        _speed.ItemsSource = new[]
        {
            new Choice("100", "100 %"), new Choice("200", "200 %"),
            new Choice("400", "400 %"), new Choice("800", "800 %"),
            new Choice("0", LocExtension.Get("Emulation.Maximum"))
        };
        Select(_speed, settings.Speed);
        _writeProtected.Content = LocExtension.Get("Emulation.FloppyWriteProtection");
        _writeProtected.IsChecked = settings.WriteProtected;
        _redirectWrites.Content = LocExtension.Get("Emulation.FloppyWriteRedirect");
        _redirectWrites.IsChecked = settings.RedirectWrites;

        var fields = new StackPanel();
        fields.Children.Add(StorageDialogUi.TwoColumnFields(
            (LocExtension.Get("Emulation.DeviceId"), new TextBox { Text = identifier, IsReadOnly = true }),
            (LocExtension.Get("Emulation.Model"), _model),
            (LocExtension.Get("Emulation.FloppySpeed"), _speed)));
        fields.Children.Add(_writeProtected);
        fields.Children.Add(_redirectWrites);
        var create = new Button
        {
            Content = LocExtension.Get("Emulation.CreateBlankFloppy"),
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(0, 14, 0, 0)
        };
        create.Click += (_, _) => CreateBlankFloppy();
        fields.Children.Add(create);
        fields.Children.Add(StorageDialogUi.Info(LocExtension.Get("Emulation.RemovableMediaRuntimeHint")));

        Content = StorageDialogUi.DialogLayout(
            StorageDialogUi.DialogHeader("\uE964", Title, $"{LocExtension.Get("Emulation.FloppyDevice")} · {machineName}"),
            StorageDialogUi.Card(LocExtension.Get("Emulation.FloppyDevice"), fields),
            StorageDialogUi.Footer(this, LocExtension.Get("Common.Save")));
    }

    private void CreateBlankFloppy()
    {
        Directory.CreateDirectory(StoragePaths.AmigaFloppyImagesDirectory);
        var dialog = new SaveFileDialog
        {
            Filter = LocExtension.Get("Emulation.FloppyImageFilter"),
            DefaultExt = ".adf",
            AddExtension = true,
            InitialDirectory = StoragePaths.AmigaFloppyImagesDirectory,
            FileName = "blank.adf"
        };
        if (dialog.ShowDialog(this) != true) return;
        using var stream = new FileStream(dialog.FileName, FileMode.Create, FileAccess.Write, FileShare.None);
        stream.SetLength(Settings.Model == "35hd" ? 1_802_240 : 901_120);
        MessageBox.Show(this, LocExtension.Get("Emulation.BlankFloppyCreated", dialog.FileName), Title,
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private static void Select(ComboBox comboBox, string value)
    {
        comboBox.SelectedItem = comboBox.Items.OfType<Choice>().FirstOrDefault(choice => choice.Value == value);
        if (comboBox.SelectedItem is null) comboBox.SelectedIndex = 0;
    }

    private sealed record Choice(string Value, string Text)
    {
        public override string ToString() => Text;
    }
}

public sealed class HardDiskDriveConfigurationDialog : Window
{
    private readonly string _identifier;
    private readonly TabControl _supportMode = new();
    private readonly TextBox _existingPath = new();
    private readonly TextBox _newName = new() { Text = "Workbench.hdf" };
    private readonly TextBox _newFolder = new();
    private readonly ComboBox _newSize = new();
    private readonly CheckBox _preallocate = new() { IsChecked = true };

    public string? SupportPath { get; private set; }

    public HardDiskDriveConfigurationDialog(string identifier, string machineName, string? currentPath)
    {
        _identifier = identifier;
        SupportPath = currentPath;
        Title = $"{LocExtension.Get("Emulation.ConfigureDevice")} {identifier}";
        Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive);
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Width = 900;
        Height = 680;
        ResizeMode = ResizeMode.NoResize;

        var address = new ComboBox { ItemsSource = new[] { identifier }, SelectedIndex = 0, IsEnabled = false };
        var interfaceChoice = new ComboBox { ItemsSource = new[] { LocExtension.Get("Visual.Automatic") }, SelectedIndex = 0 };
        var type = new ComboBox { ItemsSource = new[] { LocExtension.Get("Emulation.HardDiskDevice") }, SelectedIndex = 0, IsEnabled = false };
        var model = new ComboBox { ItemsSource = new[] { "HDF" }, SelectedIndex = 0 };
        var reader = StorageDialogUi.TwoColumnFields(
            (LocExtension.Get("Emulation.DeviceId"), address),
            (LocExtension.Get("Emulation.Interface"), interfaceChoice),
            (LocExtension.Get("Emulation.Type"), type),
            (LocExtension.Get("Emulation.Model"), model));

        _existingPath.Text = currentPath ?? string.Empty;
        var existing = new StackPanel { Margin = new Thickness(8) };
        existing.Children.Add(StorageDialogUi.PathField(LocExtension.Get("Emulation.DiskImage"), _existingPath, BrowseExisting));
        existing.Children.Add(StorageDialogUi.Info(LocExtension.Get("Emulation.ExistingDiskHint")));

        _newFolder.Text = StoragePaths.AmigaHardDisksDirectory;
        _newSize.ItemsSource = new[] { 20, 40, 80, 120, 250, 500, 1024, 2048, 4096, 8192 };
        _newSize.SelectedItem = 2048;
        _preallocate.Content = LocExtension.Get("Emulation.PreallocateFile");
        var create = new StackPanel { Margin = new Thickness(8) };
        create.Children.Add(StorageDialogUi.PathField(LocExtension.Get("Read.FileName"), _newName, null));
        create.Children.Add(StorageDialogUi.PathField(LocExtension.Get("Read.Folder"), _newFolder, BrowseFolder));
        create.Children.Add(StorageDialogUi.TwoColumnFields(
            (LocExtension.Get("Emulation.HardDiskSize"), _newSize),
            (LocExtension.Get("Explorer.Format"), new TextBox { Text = "HDF", IsReadOnly = true })));
        create.Children.Add(_preallocate);
        create.Children.Add(StorageDialogUi.Info(LocExtension.Get("Emulation.BlankHardDiskHint")));

        _supportMode.Items.Add(new TabItem { Header = LocExtension.Get("Emulation.UseExistingDisk"), Content = existing });
        _supportMode.Items.Add(new TabItem { Header = LocExtension.Get("Emulation.CreateHardDisk"), Content = create });
        _supportMode.SelectedIndex = string.IsNullOrWhiteSpace(currentPath) ? 1 : 0;

        var advanced = new Expander
        {
            Header = LocExtension.Get("Emulation.AdvancedTab"),
            Content = new TextBlock
            {
                Text = LocExtension.Get("Emulation.AutomaticDiskGeometry"),
                Margin = new Thickness(8),
                TextWrapping = TextWrapping.Wrap
            },
            Margin = new Thickness(0, 10, 0, 0)
        };
        var support = new StackPanel();
        support.Children.Add(_supportMode);
        support.Children.Add(advanced);

        var footer = StorageDialogUi.Footer(this, LocExtension.Get("Emulation.UseDisk"), Accept);
        var remove = new Button { Content = LocExtension.Get("Emulation.RemoveMedia"), HorizontalAlignment = HorizontalAlignment.Left };
        remove.Click += (_, _) => { SupportPath = null; DialogResult = true; };
        footer.Children.Insert(0, remove);

        Content = StorageDialogUi.DialogLayout(
            StorageDialogUi.DialogHeader("\uEDA2", Title, $"{LocExtension.Get("Emulation.HardDiskDevice")} · {machineName}"),
            StorageDialogUi.Card(LocExtension.Get("Emulation.StorageTab"), reader),
            StorageDialogUi.Card(LocExtension.Get("Emulation.AssociatedMedia"), support),
            footer);
    }

    private void BrowseExisting()
    {
        Directory.CreateDirectory(StoragePaths.AmigaHardDisksDirectory);
        var dialog = new OpenFileDialog
        {
            Filter = LocExtension.Get("Emulation.HardDiskFilter"),
            InitialDirectory = StoragePaths.AmigaHardDisksDirectory
        };
        if (dialog.ShowDialog(this) == true) _existingPath.Text = dialog.FileName;
    }

    private void BrowseFolder()
    {
        var dialog = new OpenFolderDialog { Multiselect = false, InitialDirectory = _newFolder.Text };
        if (dialog.ShowDialog(this) == true) _newFolder.Text = dialog.FolderName;
    }

    private void Accept()
    {
        if (_supportMode.SelectedIndex == 0)
        {
            if (!File.Exists(_existingPath.Text))
            {
                MessageBox.Show(this, LocExtension.Get("Emulation.DiskImageRequired"), Title,
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            SupportPath = Path.GetFullPath(_existingPath.Text);
            DialogResult = true;
            return;
        }

        var fileName = Path.GetFileName(_newName.Text.Trim());
        if (string.IsNullOrWhiteSpace(fileName)) fileName = $"{_identifier.TrimEnd(':')}.hdf";
        if (!fileName.EndsWith(".hdf", StringComparison.OrdinalIgnoreCase)) fileName += ".hdf";
        var folder = string.IsNullOrWhiteSpace(_newFolder.Text) ? StoragePaths.AmigaHardDisksDirectory : _newFolder.Text.Trim();
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, fileName);
        if (File.Exists(path) && MessageBox.Show(this,
                LocExtension.Get("Emulation.ReplaceExistingDisk", path).Replace("\\n", Environment.NewLine), Title,
                MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        var size = _newSize.SelectedItem is int selected ? selected : 2048;
        var byteSize = size * 1024L * 1024L;
        using (var stream = new FileStream(path, new FileStreamOptions
               {
                   Mode = FileMode.Create,
                   Access = FileAccess.Write,
                   Share = FileShare.None,
                   PreallocationSize = _preallocate.IsChecked == true ? byteSize : 0
               }))
            stream.SetLength(byteSize);
        SupportPath = path;
        DialogResult = true;
    }
}

public sealed class CompactDiscDriveConfigurationDialog : Window
{
    private readonly ComboBox _model = new();
    private readonly ComboBox _speed = new();

    public CompactDiscDriveSettings Settings => new(
        _model.SelectedItem?.ToString() ?? "CD-ROM",
        (_speed.SelectedItem as SpeedChoice)?.Value ?? "100");

    public CompactDiscDriveConfigurationDialog(string identifier, string machineName, CompactDiscDriveSettings settings,
        bool supportsWriter)
    {
        Title = $"{LocExtension.Get("Emulation.ConfigureDevice")} {identifier}";
        Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive);
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Width = 720;
        SizeToContent = SizeToContent.Height;
        ResizeMode = ResizeMode.NoResize;
        _model.ItemsSource = supportsWriter ? new[] { "CD-ROM", LocExtension.Get("Emulation.CdWriter") } : new[] { "CD-ROM" };
        _model.SelectedIndex = 0;
        _speed.ItemsSource = new[] { new SpeedChoice("100", "1×"), new SpeedChoice("0", LocExtension.Get("Emulation.Maximum")) };
        _speed.SelectedItem = _speed.Items.OfType<SpeedChoice>().FirstOrDefault(choice => choice.Value == settings.Speed) ?? _speed.Items[0];
        var fields = StorageDialogUi.TwoColumnFields(
            (LocExtension.Get("Emulation.DeviceId"), new TextBox { Text = identifier, IsReadOnly = true }),
            (LocExtension.Get("Emulation.Model"), _model),
            (LocExtension.Get("Emulation.CdSpeed"), _speed));
        var body = new StackPanel();
        body.Children.Add(fields);
        body.Children.Add(StorageDialogUi.Info(supportsWriter
            ? LocExtension.Get("Emulation.CdWriterHint")
            : LocExtension.Get("Emulation.RemovableMediaRuntimeHint")));
        Content = StorageDialogUi.DialogLayout(
            StorageDialogUi.DialogHeader("\uE958", Title, $"{LocExtension.Get("Emulation.CompactDiscDevice")} · {machineName}"),
            StorageDialogUi.Card(LocExtension.Get("Emulation.CompactDiscDevice"), body),
            StorageDialogUi.Footer(this, LocExtension.Get("Common.Save")));
    }

    private sealed record SpeedChoice(string Value, string Text)
    {
        public override string ToString() => Text;
    }
}

internal static class StorageDialogUi
{
    public static Grid DialogLayout(params UIElement[] elements)
    {
        var root = new Grid { Margin = new Thickness(18) };
        for (var index = 0; index < elements.Length; index++)
            root.RowDefinitions.Add(new RowDefinition { Height = index == elements.Length - 1 ? GridLength.Auto : new GridLength(1, GridUnitType.Auto) });
        for (var index = 0; index < elements.Length; index++)
        {
            Grid.SetRow(elements[index], index);
            root.Children.Add(elements[index]);
        }
        return root;
    }

    public static FrameworkElement DialogHeader(string icon, string title, string subtitle)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(4, 0, 4, 12) };
        panel.Children.Add(new TextBlock
        {
            Text = icon,
            FontFamily = new FontFamily("Segoe MDL2 Assets"),
            FontSize = 28,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 14, 0)
        });
        var text = new StackPanel();
        text.Children.Add(new TextBlock { Text = title, FontWeight = FontWeights.SemiBold, FontSize = 20 });
        text.Children.Add(new TextBlock { Text = subtitle, Margin = new Thickness(0, 3, 0, 0) });
        panel.Children.Add(text);
        return panel;
    }

    public static Border Card(string title, UIElement content)
    {
        var panel = new StackPanel();
        panel.Children.Add(new TextBlock
        {
            Text = title,
            FontWeight = FontWeights.SemiBold,
            FontSize = 16,
            Margin = new Thickness(0, 0, 0, 12)
        });
        panel.Children.Add(content);
        var card = new Border { Child = panel, Margin = new Thickness(0, 0, 0, 12) };
        card.SetResourceReference(FrameworkElement.StyleProperty, "Card");
        return card;
    }

    public static Grid TwoColumnFields(params (string Label, FrameworkElement Control)[] fields)
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(135) });
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(135) });
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        var rowCount = (int)Math.Ceiling(fields.Length / 2d);
        for (var row = 0; row < rowCount; row++) grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        for (var index = 0; index < fields.Length; index++)
        {
            var row = index / 2;
            var column = (index % 2) * 2;
            var label = new TextBlock
            {
                Text = fields[index].Label,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(column == 0 ? 0 : 18, 6, 8, 6)
            };
            Grid.SetRow(label, row);
            Grid.SetColumn(label, column);
            grid.Children.Add(label);
            fields[index].Control.Margin = new Thickness(0, 4, 0, 4);
            Grid.SetRow(fields[index].Control, row);
            Grid.SetColumn(fields[index].Control, column + 1);
            grid.Children.Add(fields[index].Control);
        }
        return grid;
    }

    public static Grid Field(string label, FrameworkElement control)
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.Children.Add(new TextBlock { Text = label, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) });
        Grid.SetColumn(control, 1);
        grid.Children.Add(control);
        return grid;
    }

    public static Grid PathField(string label, TextBox textBox, Action? browse)
    {
        var grid = new Grid { Margin = new Thickness(0, 3, 0, 3) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(130) });
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        if (browse is not null) grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.Children.Add(new TextBlock { Text = label, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0) });
        Grid.SetColumn(textBox, 1);
        grid.Children.Add(textBox);
        if (browse is not null)
        {
            var button = new Button { Content = LocExtension.Get("Common.Browse"), MinWidth = 110 };
            button.Click += (_, _) => browse();
            Grid.SetColumn(button, 2);
            grid.Children.Add(button);
        }
        return grid;
    }

    public static Border Info(string text)
    {
        var block = new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap };
        block.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
        var border = new Border
        {
            Child = block,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12),
            Margin = new Thickness(0, 12, 0, 0),
            Background = new SolidColorBrush(Color.FromRgb(244, 248, 255))
        };
        border.SetResourceReference(Border.BorderBrushProperty, "BorderBrush");
        return border;
    }

    public static StackPanel Footer(Window window, string acceptText, Action? accept = null)
    {
        var footer = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 2, 0, 0)
        };
        var cancel = new Button { Content = LocExtension.Get("Common.Cancel"), IsCancel = true, MinWidth = 110 };
        var ok = new Button { Content = acceptText, IsDefault = true, MinWidth = 140 };
        ok.Click += (_, _) =>
        {
            if (accept is null) window.DialogResult = true;
            else accept();
        };
        footer.Children.Add(cancel);
        footer.Children.Add(ok);
        return footer;
    }
}
