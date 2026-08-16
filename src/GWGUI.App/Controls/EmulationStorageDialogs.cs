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
        Width = 820;
        SizeToContent = SizeToContent.Height;
        ResizeMode = ResizeMode.NoResize;

        _model.ItemsSource = new[]
        {
            new Choice("35dd", LocExtension.Get("Emulation.AmigaDdFloppy")),
            new Choice("35hd", LocExtension.Get("Emulation.AmigaHdFloppy"))
        };
        ComboBoxSelection.SelectByValue<Choice>(_model, settings.Model, choice => choice.Value);
        _speed.ItemsSource = new[]
        {
            new Choice("100", "100 %"), new Choice("200", "200 %"),
            new Choice("400", "400 %"), new Choice("800", "800 %"),
            new Choice("0", LocExtension.Get("Emulation.Maximum"))
        };
        ComboBoxSelection.SelectByValue<Choice>(_speed, settings.Speed, choice => choice.Value);
        _writeProtected.Content = LocExtension.Get("Emulation.FloppyWriteProtection");
        _writeProtected.IsChecked = settings.WriteProtected;
        _redirectWrites.Content = LocExtension.Get("Emulation.FloppyWriteRedirect");
        _redirectWrites.IsChecked = settings.RedirectWrites;

        var drive = StorageDialogUi.CompactFields(
            (LocExtension.Get("Emulation.DeviceId"), new TextBox { Text = identifier, IsReadOnly = true }),
            (LocExtension.Get("Emulation.Model"), _model));
        var behavior = new StackPanel();
        behavior.Children.Add(StorageDialogUi.CompactFields(
            (LocExtension.Get("Emulation.FloppySpeed"), _speed)));
        _writeProtected.Margin = new Thickness(0, 8, 0, 4);
        _redirectWrites.Margin = new Thickness(0, 4, 0, 4);
        behavior.Children.Add(_writeProtected);
        behavior.Children.Add(_redirectWrites);
        var create = new Button
        {
            Content = LocExtension.Get("Emulation.CreateBlankFloppy"),
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(0, 14, 0, 0)
        };
        create.Click += (_, _) => CreateBlankFloppy();
        var blankMedia = new StackPanel();
        blankMedia.Children.Add(create);
        blankMedia.Children.Add(StorageDialogUi.Info(LocExtension.Get("Emulation.RemovableMediaRuntimeHint")));
        var body = new StackPanel();
        body.Children.Add(StorageDialogUi.SideBySide(
            StorageDialogUi.IconCard("\uE964", LocExtension.Get("Emulation.FloppyDrive"), drive),
            StorageDialogUi.IconCard("\uE713", LocExtension.Get("Emulation.InputBehavior"), behavior)));
        body.Children.Add(StorageDialogUi.IconCard("\uE7C3", LocExtension.Get("Emulation.BlankMedia"), blankMedia));

        Content = StorageDialogUi.DialogLayout(
            StorageDialogUi.DialogHeader("\uE964", Title, $"{LocExtension.Get("Emulation.FloppyDevice")} · {machineName}"),
            body,
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
        Title = $"{LocExtension.Get("Emulation.ConfigureDevice")} {identifier}";
        Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive);
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Width = 940;
        Height = 690;
        ResizeMode = ResizeMode.NoResize;

        var address = new TextBox { Text = identifier, IsReadOnly = true };
        var interfaceChoice = new ComboBox { ItemsSource = new[] { LocExtension.Get("Visual.Automatic") }, SelectedIndex = 0 };
        var reader = StorageDialogUi.SideBySide(
            StorageDialogUi.IconCard("\uEDA2", LocExtension.Get("Emulation.Device"),
                StorageDialogUi.CompactFields((LocExtension.Get("Emulation.DeviceId"), address))),
            StorageDialogUi.IconCard("\uE8AB", LocExtension.Get("Emulation.Interface"),
                StorageDialogUi.CompactFields((LocExtension.Get("Emulation.Interface"), interfaceChoice))));

        _existingPath.Text = currentPath ?? string.Empty;
        var existing = new StackPanel { Margin = new Thickness(8) };
        existing.Children.Add(StorageDialogUi.PathField(LocExtension.Get("Emulation.DiskImage"), _existingPath, BrowseExisting));
        existing.Children.Add(StorageDialogUi.Info(LocExtension.Get("Emulation.ExistingDiskHint")));

        _sizeUnit.ItemsSource = new[] { LocExtension.Get("Emulation.UnitMiB"), LocExtension.Get("Emulation.UnitGiB") };
        _sizeUnit.SelectedIndex = 0;
        _sizePreset.ItemsSource = new[]
        {
            new DiskSizeChoice(20, $"20 {LocExtension.Get("Emulation.UnitMiB")}"),
            new DiskSizeChoice(40, $"40 {LocExtension.Get("Emulation.UnitMiB")}"),
            new DiskSizeChoice(80, $"80 {LocExtension.Get("Emulation.UnitMiB")}"),
            new DiskSizeChoice(120, $"120 {LocExtension.Get("Emulation.UnitMiB")}"),
            new DiskSizeChoice(250, $"250 {LocExtension.Get("Emulation.UnitMiB")}"),
            new DiskSizeChoice(500, $"500 {LocExtension.Get("Emulation.UnitMiB")}"),
            new DiskSizeChoice(1024, $"1 {LocExtension.Get("Emulation.UnitGiB")}"),
            new DiskSizeChoice(2048, $"2 {LocExtension.Get("Emulation.UnitGiB")}"),
            new DiskSizeChoice(4096, $"4 {LocExtension.Get("Emulation.UnitGiB")}"),
            new DiskSizeChoice(8192, $"8 {LocExtension.Get("Emulation.UnitGiB")}"),
            new DiskSizeChoice(null, LocExtension.Get("Emulation.CustomSize"))
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
        _preallocate.Content = LocExtension.Get("Emulation.PreallocateFile");
        _automaticGeometry.Content = LocExtension.Get("Emulation.AutomaticGeometry");

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
            (LocExtension.Get("Emulation.DiskSizeProfile"), _sizePreset),
            (LocExtension.Get("Emulation.CustomSize"), customSize));
        var destination = new TextBlock
        {
            Text = StoragePaths.AmigaHardDisksDirectory,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 0)
        };
        var allocation = new StackPanel();
        allocation.Children.Add(_preallocate);
        allocation.Children.Add(StorageDialogUi.Info(LocExtension.Get("Emulation.PreallocationHint")));
        var createTop = StorageDialogUi.SideBySide(
            StorageDialogUi.IconCard("\uE8B7", LocExtension.Get("Emulation.DiskImage"), image),
            StorageDialogUi.IconCard("\uE838", LocExtension.Get("Emulation.DestinationFolder"), destination));
        var create = new StackPanel { Margin = new Thickness(4) };
        create.Children.Add(createTop);
        create.Children.Add(StorageDialogUi.IconCard("\uECA5", LocExtension.Get("Emulation.Allocation"), allocation));

        _supportMode.Items.Add(new TabItem { Header = LocExtension.Get("Emulation.UseExistingDisk"), Content = existing });
        _supportMode.Items.Add(new TabItem { Header = LocExtension.Get("Emulation.CreateHardDisk"), Content = create });
        _supportMode.SelectedIndex = string.IsNullOrWhiteSpace(currentPath) ? 1 : 0;

        var geometry = StorageDialogUi.CompactFields(
            (LocExtension.Get("Emulation.Cylinders"), _cylinders),
            (LocExtension.Get("Emulation.Heads"), _heads),
            (LocExtension.Get("Emulation.Sectors"), _sectors),
            (LocExtension.Get("Emulation.BytesPerSector"), _bytesPerSector));
        var geometryPanel = new StackPanel { Margin = new Thickness(8) };
        geometryPanel.Children.Add(_automaticGeometry);
        geometryPanel.Children.Add(geometry);
        geometryPanel.Children.Add(_capacity);
        var advanced = new Expander
        {
            Header = LocExtension.Get("Emulation.AdvancedTab"),
            Content = StorageDialogUi.IconCard("\uE9D2", LocExtension.Get("Emulation.DiskGeometry"), geometryPanel),
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
            reader,
            StorageDialogUi.Card(LocExtension.Get("Emulation.AssociatedMedia"), support),
            footer);
        UpdateDiskGeometry();
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
        var folder = StoragePaths.AmigaHardDisksDirectory;
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, fileName);
        if (File.Exists(path) && MessageBox.Show(this,
                LocExtension.Get("Emulation.ReplaceExistingDisk", path).Replace("\\n", Environment.NewLine), Title,
                MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        if (!TryGetByteSize(out var byteSize))
        {
            MessageBox.Show(this, LocExtension.Get("Emulation.InvalidDiskSize"), Title,
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
            ? LocExtension.Get("Emulation.CalculatedCapacity", StorageSizeFormatter.FormatCapacity(byteSize))
            : LocExtension.Get("Emulation.InvalidDiskSize");
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
