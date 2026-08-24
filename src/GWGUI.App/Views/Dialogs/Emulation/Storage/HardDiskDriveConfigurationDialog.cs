using GWGUI.App.Constants.Emulation;
using GWGUI.App.Constants.Localization;
using GWGUI.App.Contracts.Storage;
using GWGUI.App.Functions.Storage;
using GWGUI.App.Functions.Views.Emulation.Storage;
using GWGUI.App.Localization.Extensions;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GWGUI.Emulation;
using Microsoft.Win32;


namespace GWGUI.App.Views.Dialogs.Emulation.Storage;

public sealed class HardDiskDriveConfigurationDialog : Window
{
    private readonly string _identifier;
    private readonly string _imageDirectory;
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

    public HardDiskDriveConfigurationDialog(string identifier, string machineName, string? currentPath,
        string imageDirectory)
    {
        _identifier = identifier;
        _imageDirectory = imageDirectory;
        SupportPath = currentPath;
        Title = $"{LocExtension.Get(EmulationResourceKeys.StorageDeviceConfigure)} {identifier}";
        Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive);
        ShowInTaskbar = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Width = 980;
        Height = 760;
        MinWidth = 820;
        MinHeight = 620;
        MaxHeight = SystemParameters.WorkArea.Height;
        ResizeMode = ResizeMode.CanResize;

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
            Text = _imageDirectory,
            TextWrapping = TextWrapping.NoWrap,
            TextTrimming = TextTrimming.CharacterEllipsis,
            ToolTip = _imageDirectory,
            Margin = new Thickness(0, 6, 0, 0)
        };
        var destinationAndAllocation = new StackPanel();
        destinationAndAllocation.Children.Add(destination);
        _preallocate.Margin = new Thickness(0, 18, 0, 0);
        destinationAndAllocation.Children.Add(_preallocate);
        destinationAndAllocation.Children.Add(StorageDialogUi.Info(
            LocExtension.Get("Emulation.Storage.File.PreallocationHint")));
        var createTop = StorageDialogUi.SideBySide(
            StorageDialogUi.IconCard("\uE8B7", LocExtension.Get("Emulation.Storage.Disk.Image"), image),
            StorageDialogUi.IconCard("\uE838", LocExtension.Get("Emulation.Storage.File.DestinationFolder"),
                destinationAndAllocation));
        var create = new StackPanel { Margin = new Thickness(4) };
        create.Children.Add(createTop);

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

        var body = new StackPanel();
        body.Children.Add(reader);
        body.Children.Add(StorageDialogUi.Card(LocExtension.Get("Emulation.Storage.Media.Associated"), support));

        var root = new Grid { Margin = new Thickness(18) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition());
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var header = StorageDialogUi.DialogHeader("\uEDA2", Title,
            $"{LocExtension.Get("Emulation.Storage.HardDisk.Device")} · {machineName}");
        root.Children.Add(header);

        var scroll = new ScrollViewer
        {
            Content = body,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Margin = new Thickness(0, 12, 0, 12)
        };
        Grid.SetRow(scroll, 1);
        root.Children.Add(scroll);

        Grid.SetRow(footer, 2);
        root.Children.Add(footer);
        Content = root;
        UpdateDiskGeometry();
    }

    private void BrowseExisting()
    {
        Directory.CreateDirectory(_imageDirectory);
        var dialog = new OpenFileDialog
        {
            Filter = LocExtension.Get("Emulation.Storage.HardDisk.Filter"),
            InitialDirectory = _imageDirectory
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
        var folder = _imageDirectory;
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

}

