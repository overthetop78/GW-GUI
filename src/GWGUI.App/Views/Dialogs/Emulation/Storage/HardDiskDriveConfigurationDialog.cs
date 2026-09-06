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
using GWGUI.Emulation.HardDisks;


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
    private readonly ComboBox _imageFormat = new() { DisplayMemberPath = nameof(HardDiskImageFormat.DisplayName) };
    private readonly IReadOnlyList<HardDiskImageFormat> _formats;
    private readonly Func<string, Task<bool>>? _deleteImage;
    private readonly TextBlock _limits = new() { TextWrapping = TextWrapping.Wrap };
    private readonly ComboBox _preparation = new() { DisplayMemberPath = "Label", SelectedValuePath = "Value" };
    private readonly CheckBox _preallocate = new() { IsChecked = true };
    private readonly CheckBox _automaticGeometry = new() { IsChecked = true, IsEnabled = false };
    private readonly TextBox _cylinders = new();
    private readonly TextBox _heads = new() { Text = EmulationControlDefaults.HardDiskHeads.ToString() };
    private readonly TextBox _sectors = new() { Text = EmulationControlDefaults.HardDiskSectorsPerTrack.ToString() };
    private readonly TextBox _bytesPerSector = new() { Text = EmulationControlDefaults.HardDiskBytesPerSector.ToString() };
    private readonly TextBlock _capacity = new() { TextWrapping = TextWrapping.Wrap };

    public string? SupportPath { get; private set; }
    public string? InterfaceId { get; private set; }

    public HardDiskDriveConfigurationDialog(string identifier, string machineName, string? currentPath,
        string imageDirectory, IReadOnlyList<HardDiskImageFormat> formats,
        Func<string, Task<bool>>? deleteImage = null)
    {
        _identifier = identifier;
        _imageDirectory = imageDirectory;
        _formats = formats;
        if (formats.Count == 0) throw new ArgumentException(nameof(formats));
        _deleteImage = deleteImage;
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
        var interfaceChoice = new TextBlock();
        _imageFormat.ItemsSource = formats;
        _imageFormat.SelectedItem = formats.FirstOrDefault(format => string.Equals(format.Extension,
            Path.GetExtension(currentPath), StringComparison.OrdinalIgnoreCase)) ?? formats[0];
        _newName.Text = $"{identifier.TrimEnd(':')}{SelectedFormat.Extension}";
        interfaceChoice.Text = SelectedFormat.InterfaceName;
        _imageFormat.SelectionChanged += (_, _) =>
        {
            interfaceChoice.Text = SelectedFormat.InterfaceName;
            _newName.Text = Path.ChangeExtension(_newName.Text, SelectedFormat.Extension);
            SetSizeChoices();
        };
        var reader = StorageDialogUi.SideBySide(
            StorageDialogUi.IconCard("\uEDA2", LocExtension.Get("Emulation.Device.Name"),
                StorageDialogUi.CompactFields((LocExtension.Get("Emulation.Device.Name.Id"), address))),
            StorageDialogUi.IconCard("\uE8AB", LocExtension.Get("Emulation.Storage.Device.Interface"),
                StorageDialogUi.CompactFields((LocExtension.Get("Emulation.Storage.Device.Interface"), interfaceChoice))));

        _existingPath.Text = currentPath ?? string.Empty;
        var existing = new StackPanel { Margin = new Thickness(8) };
        existing.Children.Add(StorageDialogUi.PathField(LocExtension.Get("Emulation.Storage.Disk.Image"), _existingPath, BrowseExisting));
        existing.Children.Add(StorageDialogUi.Info(LocExtension.Get("Emulation.Storage.Disk.ExistingHint")));
        if (_deleteImage is not null)
        {
            var delete = new Button { Content = LocExtension.Get("Emulation.Hdd.Delete"), HorizontalAlignment = HorizontalAlignment.Left };
            delete.Click += async (_, _) =>
            {
                delete.IsEnabled = false;
                try
                {
                    if (!_formats.Any(format => string.Equals(format.Extension, Path.GetExtension(_existingPath.Text), StringComparison.OrdinalIgnoreCase)))
                    { ShowError(LocExtension.Get("Emulation.Hdd.InvalidFormat")); return; }
                    if (await _deleteImage(_existingPath.Text))
                    {
                        _existingPath.Clear();
                        SupportPath = null;
                    }
                }
                catch (Exception error) { ShowError(error.Message); }
                finally { delete.IsEnabled = true; }
            };
            existing.Children.Add(delete);
        }

        _sizeUnit.ItemsSource = new[] { LocExtension.Get("Emulation.Storage.Unit.MiB"), LocExtension.Get("Emulation.Storage.Unit.GiB") };
        _sizeUnit.SelectedIndex = 0;
        SetSizeChoices();
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
            (LocExtension.Get("Emulation.Hdd.Preparation"), _preparation),
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
        destinationAndAllocation.Children.Add(_limits);
        destinationAndAllocation.Children.Add(StorageDialogUi.Info(LocExtension.Get("Emulation.Hdd.PreparationHint")));
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
            Filter = string.Join('|', _formats.Select(format => $"{format.DisplayName}|*{format.Extension}")),
            InitialDirectory = _imageDirectory
        };
        if (dialog.ShowDialog(this) == true)
        {
            _existingPath.Text = dialog.FileName;
            _imageFormat.SelectedItem = _formats.First(format => string.Equals(format.Extension,
                Path.GetExtension(dialog.FileName), StringComparison.OrdinalIgnoreCase));
        }
    }

    private void Accept()
    {
        try { AcceptImage(); }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or ArgumentException)
        { ShowError(error.Message); }
    }

    private void AcceptImage()
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
            var format = _formats.FirstOrDefault(candidate => string.Equals(candidate.Extension,
                Path.GetExtension(SupportPath), StringComparison.OrdinalIgnoreCase));
            if (format is null) { ShowError(LocExtension.Get("Emulation.Hdd.InvalidFormat")); return; }
            HardDiskImageValidation.ValidateExisting(SupportPath, format);
            InterfaceId = format.InterfaceName;
            DialogResult = true;
            return;
        }

        var fileName = Path.GetFileName(_newName.Text.Trim());
        if (string.IsNullOrWhiteSpace(fileName)) fileName = _identifier.TrimEnd(':');
        if (string.IsNullOrEmpty(Path.GetExtension(fileName))) fileName += SelectedFormat.Extension;
        if (!string.Equals(Path.GetExtension(fileName), SelectedFormat.Extension, StringComparison.OrdinalIgnoreCase))
        { ShowError(LocExtension.Get("Emulation.Hdd.InvalidFormat")); return; }
        var folder = _imageDirectory;
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, fileName);
        if (File.Exists(path)) { ShowError(LocExtension.Get("Emulation.Hdd.Exists")); return; }
        if (!TryGetByteSize(out var byteSize))
        {
            MessageBox.Show(this, LocExtension.Get("Emulation.Storage.Disk.InvalidSize"), Title,
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (byteSize > SelectedFormat.MaximumBytes || byteSize < 512 || byteSize % 512 != 0)
        { ShowError(_limits.Text); return; }
        HardDiskImageCreation.Create(path, byteSize, SelectedFormat, _preallocate.IsChecked == true,
            _preparation.SelectedValue is HardDiskPreparation preparation ? preparation : HardDiskPreparation.Blank);
        SupportPath = path;
        InterfaceId = SelectedFormat.InterfaceName;
        DialogResult = true;
    }

    private HardDiskImageFormat SelectedFormat => (HardDiskImageFormat)_imageFormat.SelectedItem;

    private void ShowError(string message) => MessageBox.Show(this, message, Title,
        MessageBoxButton.OK, MessageBoxImage.Warning);

    private void SetSizeChoices()
    {
        var format = SelectedFormat;
        _preparation.ItemsSource = (format.Preparations ?? [HardDiskPreparation.Blank])
            .Select(value => new { Value = value, Label = LocExtension.Get("Emulation.Hdd.Prepare." + value) }).ToArray();
        _preparation.SelectedIndex = 0;
        _limits.Text = LocExtension.Get("Emulation.Hdd.Limits", StorageSizeFormatter.FormatCapacity(format.MaximumBytes));
        _sizePreset.ItemsSource = new long[] { 20, 40, 80, 120, 250, 500, 1024, 2047 }
            .Where(size => size * 1024 * 1024 <= format.MaximumBytes)
            .Select(size => new DiskSizeChoice(size, StorageSizeFormatter.FormatCapacity(size * 1024 * 1024)))
            .Append(new DiskSizeChoice(null, LocExtension.Get("Emulation.Storage.Geometry.CustomSize"))).ToArray();
        _sizePreset.SelectedIndex = 1;
        UpdateDiskGeometry();
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
            const long heads = 1;
            const long sectors = 32;
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

