using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GWGUI.App.Localization;
using Microsoft.Win32;

namespace GWGUI.App.Controls;

public sealed record FloppyDriveSettings(string Model, string Speed, bool WriteProtected, bool RedirectWrites);
public sealed record FloppyDriveModelChoice(string Value, string DisplayName, long BlankImageSize = 0);
public sealed record FloppyDriveDialogOptions(
    IReadOnlyList<FloppyDriveModelChoice> Models,
    string ImageDirectory,
    string ImageFilter,
    string DefaultExtension,
    bool CanCreateBlankMedia = true);
public sealed record CompactDiscDriveSettings(string Model, string Speed);

public sealed class AddStorageDeviceDialog : Window
{
    private readonly ComboBox _type = new();
    public EmulationStorageDeviceType SelectedType =>
        _type.SelectedItem is StorageDeviceChoice choice ? choice.Type : EmulationStorageDeviceType.Floppy;

    public AddStorageDeviceDialog(IEnumerable<EmulationStorageDeviceType> availableTypes)
    {
        Title = LocExtension.Get("Emulation.Storage.Device.Add");
        Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive);
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        SizeToContent = SizeToContent.WidthAndHeight;
        ResizeMode = ResizeMode.NoResize;
        MinWidth = 460;
        _type.ItemsSource = availableTypes.Select(type => new StorageDeviceChoice(type, DeviceLabel(type))).ToArray();
        _type.SelectedIndex = 0;
        Content = StorageDialogUi.DialogLayout(
            StorageDialogUi.Card(LocExtension.Get("Emulation.Device.Name.Type"),
                StorageDialogUi.Field(LocExtension.Get("Emulation.Device.Name.Type"), _type)),
            StorageDialogUi.Footer(this, LocExtension.Get("Emulation.Storage.Device.Add")));
    }

    private static string DeviceLabel(EmulationStorageDeviceType type) => type switch
    {
        EmulationStorageDeviceType.Floppy => LocExtension.Get("Emulation.Storage.Floppy.Device"),
        EmulationStorageDeviceType.HardDisk => LocExtension.Get("Emulation.Storage.HardDisk.Device"),
        EmulationStorageDeviceType.CompactDisc => LocExtension.Get("Emulation.Storage.Cd.Device"),
        EmulationStorageDeviceType.Zip => "ZIP",
        EmulationStorageDeviceType.Tape => LocExtension.Get("Emulation.Storage.Tape.Device"),
        EmulationStorageDeviceType.Cartridge => LocExtension.Get("Emulation.Atari.Storage.Cartridges"),
        EmulationStorageDeviceType.Directory => LocExtension.Get("Emulation.Folder.StorageBase"),
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
    private readonly FloppyDriveDialogOptions _options;

    public FloppyDriveSettings Settings => new(
        (_model.SelectedItem as Choice)?.Value ?? "35dd",
        (_speed.SelectedItem as Choice)?.Value ?? "100",
        _writeProtected.IsChecked == true,
        _redirectWrites.IsChecked == true);

    public FloppyDriveConfigurationDialog(string identifier, string machineName, FloppyDriveSettings settings)
        : this(identifier, machineName, settings, new FloppyDriveDialogOptions(
        [
            new("35dd", LocExtension.Get("Emulation.Amiga.Storage.Floppy.Dd"), 901_120),
            new("35hd", LocExtension.Get("Emulation.Amiga.Storage.Floppy.Hd"), 1_802_240)
        ], StoragePaths.AmigaFloppyImagesDirectory,
        LocExtension.Get("Emulation.Storage.Floppy.ImageFilter"), ".adf"))
    {
    }

    public FloppyDriveConfigurationDialog(string identifier, string machineName, FloppyDriveSettings settings,
        FloppyDriveDialogOptions options)
    {
        _options = options;
        Title = $"{LocExtension.Get("Emulation.Storage.Device.Configure")} {identifier}";
        Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive);
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Width = 820;
        SizeToContent = SizeToContent.Height;
        ResizeMode = ResizeMode.NoResize;

        _model.ItemsSource = options.Models.Select(choice =>
            new Choice(choice.Value, choice.DisplayName)).ToArray();
        ComboBoxSelection.SelectByValue<Choice>(_model, settings.Model, choice => choice.Value);
        _speed.ItemsSource = new[]
        {
            new Choice("100", "100 %"), new Choice("200", "200 %"),
            new Choice("400", "400 %"), new Choice("800", "800 %"),
            new Choice("0", LocExtension.Get("Emulation.Value.Maximum"))
        };
        ComboBoxSelection.SelectByValue<Choice>(_speed, settings.Speed, choice => choice.Value);
        _writeProtected.Content = LocExtension.Get("Emulation.Storage.Floppy.WriteProtection");
        _writeProtected.IsChecked = settings.WriteProtected;
        _redirectWrites.Content = LocExtension.Get("Emulation.Storage.Floppy.WriteRedirect");
        _redirectWrites.IsChecked = settings.RedirectWrites;

        var drive = StorageDialogUi.CompactFields(
            (LocExtension.Get("Emulation.Device.Name.Id"), new TextBox { Text = identifier, IsReadOnly = true }),
            (LocExtension.Get("Emulation.Model"), _model));
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
            blankMedia.Children.Add(StorageDialogUi.Info(LocExtension.Get("Emulation.Storage.Media.RemovableRuntimeHint")));
            body.Children.Add(StorageDialogUi.IconCard("\uE7C3", LocExtension.Get("Emulation.Storage.Media.Blank"), blankMedia));
        }

        Content = StorageDialogUi.DialogLayout(
            StorageDialogUi.DialogHeader("\uE964", Title, $"{LocExtension.Get("Emulation.Storage.Floppy.Device")} · {machineName}"),
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
        using var stream = new FileStream(dialog.FileName, FileMode.Create, FileAccess.Write, FileShare.None);
        var size = _options.Models.FirstOrDefault(model => model.Value == Settings.Model)?.BlankImageSize ?? 0;
        if (size > 0) stream.SetLength(size);
        MessageBox.Show(this, LocExtension.Get("Emulation.Storage.Floppy.BlankCreated", dialog.FileName), Title,
            MessageBoxButton.OK, MessageBoxImage.Information);
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
    private readonly TextBox _newName = new() { Text = EmulationControlDefaults.HardDiskFileName };
    private readonly ComboBox _sizePreset = new();
    private readonly TextBox _customSize = new() { Text = EmulationControlDefaults.HardDiskSizeMiB.ToString() };
    private readonly ComboBox _sizeUnit = new();
    private readonly ComboBox _imageFormat = new() { ItemsSource = new[] { "HDF" }, SelectedIndex = 0, IsEnabled = false };
    private readonly CheckBox _preallocate = new() { IsChecked = true };
    private readonly CheckBox _automaticGeometry = new() { IsChecked = true };
    private readonly TextBox _cylinders = new();
    private readonly TextBox _heads = new() { Text = EmulationControlDefaults.HardDiskHeads.ToString() };
    private readonly TextBox _sectors = new() { Text = EmulationControlDefaults.HardDiskSectorsPerTrack.ToString() };
    private readonly TextBox _bytesPerSector = new() { Text = EmulationControlDefaults.HardDiskBytesPerSector.ToString() };
    private readonly TextBlock _capacity = new() { TextWrapping = TextWrapping.Wrap };

    public string? SupportPath { get; private set; }

    public HardDiskDriveConfigurationDialog(string identifier, string machineName, string? currentPath)
    {
        _identifier = identifier;
        SupportPath = currentPath;
        Title = $"{LocExtension.Get("Emulation.Storage.Device.Configure")} {identifier}";
        Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive);
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Width = 940;
        Height = 690;
        ResizeMode = ResizeMode.NoResize;

        var address = new TextBox { Text = identifier, IsReadOnly = true };
        var interfaceChoice = new ComboBox { ItemsSource = new[] { LocExtension.Get("Visual.Automatic") }, SelectedIndex = 0 };
        var reader = StorageDialogUi.SideBySide(
            StorageDialogUi.IconCard("\uEDA2", LocExtension.Get("Emulation.Device.Name"),
                StorageDialogUi.CompactFields((LocExtension.Get("Emulation.Device.Name.Id"), address))),
            StorageDialogUi.IconCard("\uE8AB", LocExtension.Get("Emulation.Storage.Device.Interface"),
                StorageDialogUi.CompactFields((LocExtension.Get("Emulation.Storage.Device.Interface"), interfaceChoice))));

        _existingPath.Text = currentPath ?? string.Empty;
        var existing = new StackPanel { Margin = new Thickness(8) };
        existing.Children.Add(StorageDialogUi.PathField(LocExtension.Get("Emulation.Storage.Disk.Image"), _existingPath, BrowseExisting));
        existing.Children.Add(StorageDialogUi.Info(LocExtension.Get("Emulation.Storage.Disk.ExistingHint")));

        _sizeUnit.ItemsSource = new[] { LocExtension.Get("Emulation.Storage.Unit.MiB"), LocExtension.Get("Emulation.Storage.Unit.GiB") };
        _sizeUnit.SelectedIndex = 0;
        _sizePreset.ItemsSource = new[]
        {
            new DiskSizeChoice(20, $"20 {LocExtension.Get("Emulation.Storage.Unit.MiB")}"),
            new DiskSizeChoice(40, $"40 {LocExtension.Get("Emulation.Storage.Unit.MiB")}"),
            new DiskSizeChoice(80, $"80 {LocExtension.Get("Emulation.Storage.Unit.MiB")}"),
            new DiskSizeChoice(120, $"120 {LocExtension.Get("Emulation.Storage.Unit.MiB")}"),
            new DiskSizeChoice(250, $"250 {LocExtension.Get("Emulation.Storage.Unit.MiB")}"),
            new DiskSizeChoice(500, $"500 {LocExtension.Get("Emulation.Storage.Unit.MiB")}"),
            new DiskSizeChoice(1024, $"1 {LocExtension.Get("Emulation.Storage.Unit.GiB")}"),
            new DiskSizeChoice(2048, $"2 {LocExtension.Get("Emulation.Storage.Unit.GiB")}"),
            new DiskSizeChoice(4096, $"4 {LocExtension.Get("Emulation.Storage.Unit.GiB")}"),
            new DiskSizeChoice(8192, $"8 {LocExtension.Get("Emulation.Storage.Unit.GiB")}"),
            new DiskSizeChoice(null, LocExtension.Get("Emulation.Storage.Geometry.CustomSize"))
        };
        _sizePreset.SelectedIndex = 7;
        _sizePreset.SelectionChanged += (_, _) => UpdateDiskGeometry();
        _customSize.TextChanged += (_, _) => UpdateDiskGeometry();
        _sizeUnit.SelectionChanged += (_, _) => UpdateDiskGeometry();
        _automaticGeometry.Checked += (_, _) => UpdateDiskGeometry();
        _automaticGeometry.Unchecked += (_, _) => UpdateDiskGeometry();
        _cylinders.TextChanged += (_, _) => UpdateCapacity();
        _heads.TextChanged += (_, _) => UpdateCapacity();
        _sectors.TextChanged += (_, _) => UpdateCapacity();
        _bytesPerSector.TextChanged += (_, _) => UpdateCapacity();
        _preallocate.Content = LocExtension.Get("Emulation.Storage.File.Preallocate");
        _automaticGeometry.Content = LocExtension.Get("Emulation.Storage.Geometry.AutomaticProfile");

        var customSize = new Grid();
        customSize.ColumnDefinitions.Add(new ColumnDefinition());
        customSize.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });
        customSize.Children.Add(_customSize);
        Grid.SetColumn(_sizeUnit, 1);
        _sizeUnit.Margin = new Thickness(6, 0, 0, 0);
        customSize.Children.Add(_sizeUnit);
        var image = StorageDialogUi.CompactFields(
            (LocExtension.Get("Read.FileName"), _newName),
            (LocExtension.Get("Explorer.Format"), _imageFormat),
            (LocExtension.Get("Emulation.Storage.Geometry.SizeProfile"), _sizePreset),
            (LocExtension.Get("Emulation.Storage.Geometry.CustomSize"), customSize));
        var destination = new TextBlock
        {
            Text = StoragePaths.AmigaHardDisksDirectory,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 0)
        };
        var allocation = new StackPanel();
        allocation.Children.Add(_preallocate);
        allocation.Children.Add(StorageDialogUi.Info(LocExtension.Get("Emulation.Storage.File.PreallocationHint")));
        var createTop = StorageDialogUi.SideBySide(
            StorageDialogUi.IconCard("\uE8B7", LocExtension.Get("Emulation.Storage.Disk.Image"), image),
            StorageDialogUi.IconCard("\uE838", LocExtension.Get("Emulation.Storage.File.DestinationFolder"), destination));
        var create = new StackPanel { Margin = new Thickness(4) };
        create.Children.Add(createTop);
        create.Children.Add(StorageDialogUi.IconCard("\uECA5", LocExtension.Get("Emulation.Storage.Geometry.Allocation"), allocation));

        _supportMode.Items.Add(new TabItem { Header = LocExtension.Get("Emulation.Storage.Disk.UseExisting"), Content = existing });
        _supportMode.Items.Add(new TabItem { Header = LocExtension.Get("Emulation.Storage.HardDisk.Create"), Content = create });
        _supportMode.SelectedIndex = string.IsNullOrWhiteSpace(currentPath) ? 1 : 0;

        var geometry = StorageDialogUi.CompactFields(
            (LocExtension.Get("Emulation.Storage.Geometry.Cylinders"), _cylinders),
            (LocExtension.Get("Emulation.Storage.Geometry.Heads"), _heads),
            (LocExtension.Get("Emulation.Storage.Geometry.Sectors"), _sectors),
            (LocExtension.Get("Emulation.Storage.Geometry.BytesPerSector"), _bytesPerSector));
        var geometryPanel = new StackPanel { Margin = new Thickness(8) };
        geometryPanel.Children.Add(_automaticGeometry);
        geometryPanel.Children.Add(geometry);
        geometryPanel.Children.Add(_capacity);
        var advanced = new Expander
        {
            Header = LocExtension.Get("Emulation.Tab.Advanced"),
            Content = StorageDialogUi.IconCard("\uE9D2", LocExtension.Get("Emulation.Storage.Geometry.Label"), geometryPanel),
            Margin = new Thickness(0, 10, 0, 0)
        };
        var support = new StackPanel();
        support.Children.Add(_supportMode);
        support.Children.Add(advanced);

        var footer = StorageDialogUi.Footer(this, LocExtension.Get("Emulation.Storage.Disk.Use"), Accept);
        var remove = new Button { Content = LocExtension.Get("Emulation.Storage.Media.Remove"), HorizontalAlignment = HorizontalAlignment.Left };
        remove.Click += (_, _) => { SupportPath = null; DialogResult = true; };
        footer.Children.Insert(0, remove);

        Content = StorageDialogUi.DialogLayout(
            StorageDialogUi.DialogHeader("\uEDA2", Title, $"{LocExtension.Get("Emulation.Storage.HardDisk.Device")} · {machineName}"),
            reader,
            StorageDialogUi.Card(LocExtension.Get("Emulation.Storage.Media.Associated"), support),
            footer);
        UpdateDiskGeometry();
    }

    private void BrowseExisting()
    {
        Directory.CreateDirectory(StoragePaths.AmigaHardDisksDirectory);
        var dialog = new OpenFileDialog
        {
            Filter = LocExtension.Get("Emulation.Storage.HardDisk.Filter"),
            InitialDirectory = StoragePaths.AmigaHardDisksDirectory
        };
        if (dialog.ShowDialog(this) == true) _existingPath.Text = dialog.FileName;
    }

    private void Accept()
    {
        if (_supportMode.SelectedIndex == 0)
        {
            if (!File.Exists(_existingPath.Text))
            {
                MessageBox.Show(this, LocExtension.Get("Emulation.Storage.Disk.ImageRequired"), Title,
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
        var folder = StoragePaths.AmigaHardDisksDirectory;
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, fileName);
        if (File.Exists(path) && MessageBox.Show(this,
                LocExtension.Get("Emulation.Storage.Disk.ReplaceExisting", path).Replace("\\n", Environment.NewLine), Title,
                MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        if (!TryGetByteSize(out var byteSize))
        {
            MessageBox.Show(this, LocExtension.Get("Emulation.Storage.Disk.InvalidSize"), Title,
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
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

    private void UpdateDiskGeometry()
    {
        var custom = (_sizePreset.SelectedItem as DiskSizeChoice)?.SizeMiB is null;
        _customSize.IsEnabled = custom;
        _sizeUnit.IsEnabled = custom;
        var automatic = _automaticGeometry.IsChecked == true;
        _cylinders.IsEnabled = !automatic;
        _heads.IsEnabled = !automatic;
        _sectors.IsEnabled = !automatic;
        _bytesPerSector.IsEnabled = !automatic;
        if (automatic && TryGetSelectedSize(out var byteSize))
        {
            const long heads = EmulationControlDefaults.HardDiskHeads;
            const long sectors = EmulationControlDefaults.HardDiskSectorsPerTrack;
            const long bytesPerSector = EmulationControlDefaults.HardDiskBytesPerSector;
            _heads.Text = heads.ToString();
            _sectors.Text = sectors.ToString();
            _bytesPerSector.Text = bytesPerSector.ToString();
            _cylinders.Text = Math.Max(1, (long)Math.Ceiling(byteSize / (double)(heads * sectors * bytesPerSector))).ToString();
        }
        UpdateCapacity();
    }

    private void UpdateCapacity()
    {
        _capacity.Text = TryGetByteSize(out var byteSize)
            ? LocExtension.Get("Emulation.Storage.Geometry.CalculatedCapacity", StorageSizeFormatter.FormatCapacity(byteSize))
            : LocExtension.Get("Emulation.Storage.Disk.InvalidSize");
    }

    private bool TryGetByteSize(out long byteSize)
    {
        if (_automaticGeometry.IsChecked == true) return TryGetSelectedSize(out byteSize);
        if (long.TryParse(_cylinders.Text, out var cylinders) && cylinders > 0 &&
            long.TryParse(_heads.Text, out var heads) && heads > 0 &&
            long.TryParse(_sectors.Text, out var sectors) && sectors > 0 &&
            long.TryParse(_bytesPerSector.Text, out var bytesPerSector) && bytesPerSector > 0)
        {
            try
            {
                byteSize = checked(cylinders * heads * sectors * bytesPerSector);
                return byteSize > 0;
            }
            catch (OverflowException) { }
        }
        byteSize = 0;
        return false;
    }

    private bool TryGetSelectedSize(out long byteSize)
    {
        if ((_sizePreset.SelectedItem as DiskSizeChoice)?.SizeMiB is long preset)
        {
            byteSize = preset * 1024L * 1024L;
            return true;
        }
        if (double.TryParse(_customSize.Text, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.CurrentCulture, out var custom) && custom > 0)
        {
            var multiplier = _sizeUnit.SelectedIndex == 1 ? 1024d * 1024d * 1024d : 1024d * 1024d;
            if (custom <= long.MaxValue / multiplier)
            {
                byteSize = (long)Math.Round(custom * multiplier);
                return byteSize > 0;
            }
        }
        byteSize = 0;
        return false;
    }

    private sealed record DiskSizeChoice(long? SizeMiB, string Text)
    {
        public override string ToString() => Text;
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
        Title = $"{LocExtension.Get("Emulation.Storage.Device.Configure")} {identifier}";
        Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive);
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Width = 720;
        SizeToContent = SizeToContent.Height;
        ResizeMode = ResizeMode.NoResize;
        _model.ItemsSource = supportsWriter ? new[] { "CD-ROM", LocExtension.Get("Emulation.Storage.Cd.Writer") } : new[] { "CD-ROM" };
        _model.SelectedIndex = 0;
        _speed.ItemsSource = new[] { new SpeedChoice("100", "1×"), new SpeedChoice("0", LocExtension.Get("Emulation.Value.Maximum")) };
        _speed.SelectedItem = _speed.Items.OfType<SpeedChoice>().FirstOrDefault(choice => choice.Value == settings.Speed) ?? _speed.Items[0];
        var fields = StorageDialogUi.TwoColumnFields(
            (LocExtension.Get("Emulation.Device.Name.Id"), new TextBox { Text = identifier, IsReadOnly = true }),
            (LocExtension.Get("Emulation.Model"), _model),
            (LocExtension.Get("Emulation.Storage.Cd.Speed"), _speed));
        var body = new StackPanel();
        body.Children.Add(fields);
        body.Children.Add(StorageDialogUi.Info(supportsWriter
            ? LocExtension.Get("Emulation.Storage.Cd.WriterHint")
            : LocExtension.Get("Emulation.Storage.Media.RemovableRuntimeHint")));
        Content = StorageDialogUi.DialogLayout(
            StorageDialogUi.DialogHeader("\uE958", Title, $"{LocExtension.Get("Emulation.Storage.Cd.Device")} · {machineName}"),
            StorageDialogUi.Card(LocExtension.Get("Emulation.Storage.Cd.Device"), body),
            StorageDialogUi.Footer(this, LocExtension.Get("Common.Save")));
    }

    private sealed record SpeedChoice(string Value, string Text)
    {
        public override string ToString() => Text;
    }
}
