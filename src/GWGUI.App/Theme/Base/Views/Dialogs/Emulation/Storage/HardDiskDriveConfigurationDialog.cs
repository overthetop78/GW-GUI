using GWGUI.App.Constants.Emulation;
using GWGUI.App.Constants.Controls.Visual;
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
    private readonly int? _fixedImageSizeBytes;
    private readonly EmulationStorageDialogPresentation? _presentation;
    private readonly string _imageDirectory;
    private readonly Guid _clientGuid;
    private readonly TabControl _supportMode = new();
    private readonly TextBox _existingPath = new();
    private readonly TextBox _newName = new() { Text = EmulationControlDefaults.HardDiskFileName };
    private readonly ComboBox _sizePreset = new();
    private readonly TextBox _customSize = new() { Text = EmulationControlDefaults.HardDiskSizeMiB.ToString() };
    private readonly ComboBox _sizeUnit = new();
    private readonly ComboBox _imageFormat = new() { DisplayMemberPath = nameof(HardDiskImageFormat.DisplayName) };
    private readonly IReadOnlyList<HardDiskImageFormat> _formats;
    private readonly Func<string, Task<bool>>? _deleteImage;
    private readonly StackPanel _customSizeRow = new();
    private readonly TextBlock _limits = new() { TextWrapping = TextWrapping.Wrap };
    private readonly ComboBox _preparation = new() { DisplayMemberPath = "Label", SelectedValuePath = "Value" };
    private readonly CheckBox _preallocate = new() { IsChecked = true };
    private readonly CheckBox _automaticGeometry = new() { IsChecked = true, IsEnabled = false };
    private readonly TextBox _cylinders = new();
    private readonly TextBox _heads = new() { Text = EmulationControlDefaults.HardDiskHeads.ToString() };
    private readonly TextBox _sectors = new() { Text = EmulationControlDefaults.HardDiskSectorsPerTrack.ToString() };
    private readonly TextBox _bytesPerSector = new() { Text = EmulationControlDefaults.HardDiskBytesPerSector.ToString() };
    private readonly TextBlock _capacity = new() { TextWrapping = TextWrapping.Wrap };
    private readonly Dictionary<string, FrameworkElement> _fields = new(StringComparer.Ordinal);
    private readonly IReadOnlyDictionary<string, string> _initialOptions;

    public string? SupportPath { get; private set; }
    public string? InterfaceId { get; private set; }
    /// <summary>Réglages opaques du lecteur, conservés avec les champs fournis par le module.</summary>
    public IReadOnlyDictionary<string, string> Options
    {
        get
        {
            var values = new Dictionary<string, string>(_initialOptions, StringComparer.Ordinal);
            foreach (var entry in _fields) values[entry.Key] = StorageConfigurationFieldFunctions.Read(entry.Value);
            return values;
        }
    }

    public HardDiskDriveConfigurationDialog(string identifier, string machineName, string? currentPath,
        string imageDirectory, IReadOnlyList<HardDiskImageFormat> formats, Guid clientGuid,
        Func<string, Task<bool>>? deleteImage = null, int? fixedImageSizeBytes = null,
        string? defaultImageFileName = null, EmulationStorageDialogPresentation? presentation = null, IReadOnlyList<EmulationSettingsField>? configurationFields = null, IReadOnlyDictionary<string, string>? options = null, string? moduleId = null)
    {
        _initialOptions = options ?? new Dictionary<string, string>();
        _identifier = identifier;
        _fixedImageSizeBytes = fixedImageSizeBytes;
        _presentation = presentation;
        _imageDirectory = imageDirectory;
        _clientGuid = clientGuid;
        _formats = formats;
        if (formats.Count == 0) throw new ArgumentException(nameof(formats));
        _deleteImage = deleteImage;
        SupportPath = currentPath;
        Title = $"{LocExtension.Get(EmulationResourceKeys.StorageDeviceConfigure)} {identifier}";
        Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive);
        ShowInTaskbar = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Width = Math.Min(fixedImageSizeBytes is null ? StorageConfigurationLayoutConstants.HardDiskWidth : StorageConfigurationLayoutConstants.ManagedWidth, SystemParameters.WorkArea.Width * StorageConfigurationLayoutConstants.WorkAreaWidthRatio);
        Height = fixedImageSizeBytes is null ? StorageConfigurationLayoutConstants.HardDiskHeight : double.NaN;
        MinWidth = fixedImageSizeBytes is null ? StorageConfigurationLayoutConstants.FloppyWidth : StorageConfigurationLayoutConstants.MinimumWidth;
        MinHeight = fixedImageSizeBytes is null ? StorageConfigurationLayoutConstants.MinimumWidth : 0;
        SizeToContent = fixedImageSizeBytes is null ? SizeToContent.Manual : SizeToContent.Height;
        MaxHeight = SystemParameters.WorkArea.Height;
        ResizeMode = ResizeMode.CanResize;

        var interfaceChoice = new TextBlock { Margin = new Thickness(0, 3, 0, 0) };
        _imageFormat.ItemsSource = formats;
        _imageFormat.SelectedItem = formats.FirstOrDefault(format => string.Equals(format.Extension, Path.GetExtension(currentPath), StringComparison.OrdinalIgnoreCase) && File.Exists(currentPath) && HardDiskImageValidation.IsSupportedCapacity(new FileInfo(currentPath).Length, format)) ?? formats[0];
        _newName.Text = defaultImageFileName ?? $"{identifier.TrimEnd(':')}{SelectedFormat.Extension}";
        if (_fixedImageSizeBytes is not null) _newName.Text = Path.GetFileNameWithoutExtension(_newName.Text);
        interfaceChoice.Text = _fixedImageSizeBytes is not null ? LocExtension.Get("Emulation.Storage.MemoryCard.Device") : SelectedFormat.InterfaceName;
        _imageFormat.SelectionChanged += (_, _) =>
        {
            interfaceChoice.Text = _fixedImageSizeBytes is not null ? LocExtension.Get("Emulation.Storage.MemoryCard.Device") : SelectedFormat.InterfaceName;
            if (_fixedImageSizeBytes is null) _newName.Text = Path.ChangeExtension(_newName.Text, SelectedFormat.Extension);
            SetSizeChoices();
        };
        _existingPath.Text = currentPath ?? string.Empty;
        var existing = new StackPanel { Margin = new Thickness(8) };
        existing.Children.Add(StorageDialogUi.PathField(LocExtension.Get(_presentation?.ImageResourceKey ?? "Emulation.Storage.Disk.Image"), _existingPath, BrowseExisting));
        existing.Children.Add(StorageDialogUi.Info(LocExtension.Get(_presentation?.ExistingHintResourceKey ?? "Emulation.Storage.Disk.ExistingHint")));
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
        foreach (var field in new[] { _cylinders, _heads, _sectors, _bytesPerSector }) field.TextChanged += (_, _) => UpdateCapacity();
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
        var imageFields = _fixedImageSizeBytes is not null
            ? StorageDialogUi.CompactFields((LocExtension.Get("Emulation.Storage.MemoryCard.Name"), _newName))
            : StorageDialogUi.CompactFields(
            (LocExtension.Get(_presentation?.FileNameResourceKey ?? "Read.FileName"), _newName),
            (LocExtension.Get("Explorer.Format"), _imageFormat),
            (LocExtension.Get("Emulation.Hdd.Preparation"), SingleChoice(_preparation, () => _preparation.Text)),
            (LocExtension.Get("Emulation.Storage.Geometry.SizeProfile"), SingleChoice(_sizePreset, () => (_sizePreset.SelectedItem as HardDiskCapacityChoice)?.Text ?? _sizePreset.Text)));
        var image = new StackPanel();
        image.Children.Add(imageFields);
        if (_fixedImageSizeBytes is null)
        {
            _customSizeRow.Children.Add(StorageDialogUi.CompactFields((LocExtension.Get("Emulation.Storage.Geometry.CustomSize"), customSize)));
            image.Children.Add(_customSizeRow);
        }
        var destination = new TextBlock
        {
            Text = _imageDirectory,
            TextTrimming = TextTrimming.CharacterEllipsis,
            ToolTip = _imageDirectory,
            Margin = new Thickness(0, 6, 0, 0)
        };
        var destinationAndAllocation = new StackPanel();
        if (_fixedImageSizeBytes is null) destinationAndAllocation.Children.Add(destination);
        _preallocate.Margin = new Thickness(0, 18, 0, 0);
        if (_fixedImageSizeBytes is null)
        {
            destinationAndAllocation.Children.Add(_preallocate);
            _preallocate.ToolTip = LocExtension.Get("Emulation.Storage.File.PreallocationHint");
            destinationAndAllocation.Children.Add(_limits);
            _preparation.ToolTip = LocExtension.Get("Emulation.Hdd.PreparationHint");
        }
        else destinationAndAllocation.Children.Add(StorageDialogUi.Info(StorageSizeFormatter.FormatCapacity(_fixedImageSizeBytes.Value)));
        var createTop = new StackPanel();
        createTop.Children.Add(image);
        createTop.Children.Add(destinationAndAllocation);
        var create = new StackPanel { Margin = new Thickness(4) };
        create.Children.Add(createTop);
        if (_fixedImageSizeBytes is not null)
        {
            create.Children.Add(new TextBlock { Text = LocExtension.Get(_presentation?.DestinationFolderResourceKey ?? "Emulation.Storage.File.DestinationFolder"), FontWeight = FontWeights.SemiBold });
            create.Children.Add(destination);
            create.Children.Add(new TextBlock { Text = $"{_fixedImageSizeBytes.Value / 1024} KiB", Margin = new Thickness(0, 8, 0, 0) });
        }

        _supportMode.Items.Add(new TabItem { Header = LocExtension.Get(_presentation?.UseExistingResourceKey ?? "Emulation.Storage.Disk.UseExisting"), Content = existing });
        _supportMode.Items.Add(new TabItem { Header = LocExtension.Get(_presentation?.CreateResourceKey ?? "Emulation.Storage.HardDisk.Create"), Content = create });
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
            Content = geometryPanel,
            Margin = new Thickness(0, 10, 0, 0)
        };
        var support = new StackPanel();
        support.Children.Add(_supportMode);
        if (_fixedImageSizeBytes is null) support.Children.Add(advanced);

        var footer = StorageDialogUi.Footer(this, LocExtension.Get(_presentation?.UseActionResourceKey ?? "Emulation.Storage.Disk.Use"), Accept);
        var remove = new Button { Content = LocExtension.Get("Emulation.Storage.Media.Remove"), HorizontalAlignment = HorizontalAlignment.Left, Visibility = string.IsNullOrWhiteSpace(currentPath) ? Visibility.Collapsed : Visibility.Visible };
        remove.Click += (_, _) => { SupportPath = null; DialogResult = true; };
        footer.Children.Insert(0, remove);

        var body = new StackPanel();
        body.Children.Add(support);
        var behaviorGroups = StorageConfigurationFieldFunctions.CreateGroups(configurationFields ?? [], moduleId, _initialOptions, _fields);
        FrameworkElement arrangedBody = body;
        if (behaviorGroups.Children.Count > 0)
        {
            var columns = new Grid();
            columns.ColumnDefinitions.Add(new ColumnDefinition());
            columns.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(StorageConfigurationLayoutConstants.BehaviorColumnWidth) });
            body.Margin = new Thickness(0, 0, 10, 0);
            columns.Children.Add(body);
            Grid.SetColumn(behaviorGroups, 1);
            columns.Children.Add(behaviorGroups);
            arrangedBody = columns;
        }

        var root = new Grid { Margin = new Thickness(18) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition());
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var header = StorageDialogUi.DialogHeader(_fixedImageSizeBytes is null ? IconGlyphs.HardDisk : IconGlyphs.Memory, Title, $"{LocExtension.Get(_presentation?.DeviceResourceKey ?? "Emulation.Storage.HardDisk.Device")} · {machineName}", _fixedImageSizeBytes is null ? interfaceChoice : null);
        root.Children.Add(header);

        var scroll = new ScrollViewer
        {
            Content = arrangedBody,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Margin = new Thickness(0, 12, 0, 12)
        };
        Grid.SetRow(scroll, 1);
        root.Children.Add(scroll);

        Grid.SetRow(footer, 2);
        root.Children.Add(footer);
        Content = root;
        if (_fixedImageSizeBytes is null) UpdateDiskGeometry();
    }

    private void BrowseExisting()
    {
        var dialog = new OpenFileDialog
        {
            Filter = string.Join('|', _formats.Select(format => $"{format.DisplayName}|*{format.Extension}")),
            InitialDirectory = Directory.Exists(_imageDirectory) ? _imageDirectory : string.Empty,
            ClientGuid = _clientGuid
        };
        if (dialog.ShowDialog(this) == true)
        {
            _existingPath.Text = dialog.FileName;
            _imageFormat.SelectedItem = _formats.FirstOrDefault(format => string.Equals(format.Extension, Path.GetExtension(dialog.FileName), StringComparison.OrdinalIgnoreCase) && HardDiskImageValidation.IsSupportedCapacity(new FileInfo(dialog.FileName).Length, format)) ?? _formats.First(format => string.Equals(format.Extension, Path.GetExtension(dialog.FileName), StringComparison.OrdinalIgnoreCase));
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
                MessageBox.Show(this, LocExtension.Get(_presentation?.ImageRequiredResourceKey ?? "Emulation.Storage.Disk.ImageRequired"), Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            SupportPath = Path.GetFullPath(_existingPath.Text);
            var format = _formats.FirstOrDefault(candidate => string.Equals(candidate.Extension, Path.GetExtension(SupportPath), StringComparison.OrdinalIgnoreCase) && (_fixedImageSizeBytes is not null || HardDiskImageValidation.IsSupportedCapacity(new FileInfo(SupportPath).Length, candidate)));
            if (format is null) { ShowError(LocExtension.Get(_presentation?.InvalidFormatResourceKey ?? "Emulation.Hdd.InvalidFormat")); return; }
            if (_fixedImageSizeBytes is { } existingFixedSize)
            {
                if (new FileInfo(SupportPath).Length != existingFixedSize)
                { ShowError(LocExtension.Get(_presentation?.InvalidSizeResourceKey ?? "Emulation.Storage.Disk.InvalidSize")); return; }
            }
            else HardDiskImageValidation.ValidateExisting(SupportPath, format);
            InterfaceId = format.InterfaceName;
            DialogResult = true;
            return;
        }

        var fileName = Path.GetFileName(_newName.Text.Trim());
        if (string.IsNullOrWhiteSpace(fileName)) fileName = _identifier.TrimEnd(':');
        if (_fixedImageSizeBytes is not null) fileName += SelectedFormat.Extension;
        if (string.IsNullOrEmpty(Path.GetExtension(fileName))) fileName += SelectedFormat.Extension;
        if (!string.Equals(Path.GetExtension(fileName), SelectedFormat.Extension, StringComparison.OrdinalIgnoreCase))
        { ShowError(LocExtension.Get(_presentation?.InvalidFormatResourceKey ?? "Emulation.Hdd.InvalidFormat")); return; }
        var folder = _imageDirectory;
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, fileName);
        if (File.Exists(path)) { ShowError(LocExtension.Get("Emulation.Hdd.Exists")); return; }
        if (_fixedImageSizeBytes is { } fixedSize)
        {
            var created = false;
            try
            {
                using (var image = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    created = true;
                    image.SetLength(fixedSize);
                    SelectedFormat.InitializeBlankImage?.Invoke(image);
                }
            }
            catch
            {
                if (created && File.Exists(path)) File.Delete(path);
                throw;
            }
            SupportPath = path;
            InterfaceId = SelectedFormat.InterfaceName;
            DialogResult = true;
            return;
        }
        if (!TryGetByteSize(out var byteSize))
        {
            MessageBox.Show(this, LocExtension.Get("Emulation.Storage.Disk.InvalidSize"), Title, MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (!IsSupportedCapacity(byteSize, SelectedFormat))
        { ShowError(_limits.Text); return; }
        DiskImageGeometry? geometry = null;
        if (SelectedFormat.SupportsExplicitGeometry)
        {
            if (!long.TryParse(_cylinders.Text, out var cylinders) || !int.TryParse(_heads.Text, out var heads) || !int.TryParse(_sectors.Text, out var sectors) || !int.TryParse(_bytesPerSector.Text, out var sectorSize))
            { ShowError(LocExtension.Get("Emulation.Storage.Disk.InvalidSize")); return; }
            geometry = new(cylinders, heads, sectors, sectorSize);
        }
        HardDiskImageCreation.Create(path, byteSize, SelectedFormat, SelectedFormat.SupportsFixedAllocation && _preallocate.IsChecked == true, _preparation.SelectedValue is HardDiskPreparation preparation ? preparation : HardDiskPreparation.Blank, geometry);
        SupportPath = path;
        InterfaceId = SelectedFormat.InterfaceName;
        DialogResult = true;
    }

    /// <summary>Une valeur imposée reste visible sans présenter un choix qui n'existe pas.</summary>
    private static FrameworkElement SingleChoice(ComboBox choice, Func<string> label)
    {
        var text = new TextBlock { VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap };
        var panel = new Grid();
        panel.Children.Add(choice);
        panel.Children.Add(text);
        void Refresh()
        {
            var single = choice.Items.Count == 1;
            choice.Visibility = single ? Visibility.Collapsed : Visibility.Visible;
            text.Visibility = single ? Visibility.Visible : Visibility.Collapsed;
            text.Text = label();
            text.ToolTip = choice.ToolTip;
        }
        choice.SelectionChanged += (_, _) => Refresh();
        panel.Loaded += (_, _) => Refresh();
        Refresh();
        return panel;
    }

    private HardDiskImageFormat SelectedFormat => (HardDiskImageFormat)_imageFormat.SelectedItem;

    private void ShowError(string message) => MessageBox.Show(this, message, Title, MessageBoxButton.OK, MessageBoxImage.Warning);

    private void SetSizeChoices()
    {
        var format = SelectedFormat;
        _preparation.ItemsSource = format.SupportedPreparations.Select(value => new { Value = value, Label = LocExtension.Get("Emulation.Hdd.Prepare." + value) }).ToArray();
        _automaticGeometry.IsEnabled = format.SupportsExplicitGeometry && format.FixedGeometry is null;
        if (!format.SupportsExplicitGeometry || format.FixedGeometry is not null) _automaticGeometry.IsChecked = true;
        _preparation.SelectedIndex = 0;
        _limits.Visibility = format.MinimumBytes == format.MaximumBytes ? Visibility.Collapsed : Visibility.Visible;
        _limits.Text = LocExtension.Get("Emulation.Hdd.CapacityLimits", StorageSizeFormatter.FormatCapacity(format.MinimumBytes), StorageSizeFormatter.FormatCapacity(format.MaximumBytes), string.Join(" / ", format.LogicalSectorSizes.Order()));
        _preallocate.IsEnabled = format.SupportsFixedAllocation;
        if (!format.SupportsFixedAllocation) _preallocate.IsChecked = false;
        var sizeChoices = format.AllowedCapacitiesBytes is { } allowed
            ? allowed.Distinct().Order().Where(size => IsSupportedCapacity(size, format)).Select(size => new HardDiskCapacityChoice(size, StorageSizeFormatter.FormatCapacity(size))).ToArray()
            : EmulationControlDefaults.HardDiskCapacityMebibytes.Select(size => size * EmulationControlDefaults.BytesPerMebibyte)
                .Append(format.DefaultBytes).Distinct().Order().Where(size => IsSupportedCapacity(size, format))
                .Select(size => new HardDiskCapacityChoice(size, StorageSizeFormatter.FormatCapacity(size)))
                .Append(new HardDiskCapacityChoice(null, LocExtension.Get("Emulation.Storage.Geometry.CustomSize"))).ToArray();
        _sizePreset.ItemsSource = sizeChoices;
        _sizePreset.SelectedIndex = Math.Max(0, Array.FindIndex(sizeChoices, choice => choice.Bytes == format.DefaultBytes));
        UpdateDiskGeometry();
    }

    private void UpdateDiskGeometry()
    {
        var custom = SelectedFormat.AllowedCapacitiesBytes is null && (_sizePreset.SelectedItem as HardDiskCapacityChoice)?.Bytes is null;
        _customSizeRow.Visibility = SelectedFormat.AllowedCapacitiesBytes is null ? Visibility.Visible : Visibility.Collapsed;
        _customSize.IsEnabled = custom;
        _sizeUnit.IsEnabled = custom;
        var automatic = _automaticGeometry.IsChecked == true;
        _cylinders.IsEnabled = !automatic;
        _heads.IsEnabled = !automatic;
        _sectors.IsEnabled = !automatic;
        _bytesPerSector.IsEnabled = !automatic;
        if (SelectedFormat.FixedGeometry is { } fixedGeometry)
        {
            _cylinders.Text = fixedGeometry.Cylinders.ToString();
            _heads.Text = fixedGeometry.Heads.ToString();
            _sectors.Text = fixedGeometry.SectorsPerTrack.ToString();
            _bytesPerSector.Text = fixedGeometry.SectorSize.ToString();
        }
        else if (automatic && TryGetSelectedSize(out var byteSize))
        {
            var heads = SelectedFormat.Geometry?.Heads ?? 1;
            var sectors = SelectedFormat.Geometry?.SectorsPerTrack ?? 32;
            var bytesPerSector = SelectedFormat.LogicalSectorSizes.Contains(EmulationControlDefaults.HardDiskBytesPerSector) ? EmulationControlDefaults.HardDiskBytesPerSector : SelectedFormat.LogicalSectorSizes.Min();
            _heads.Text = heads.ToString();
            _sectors.Text = sectors.ToString();
            _bytesPerSector.Text = bytesPerSector.ToString();
            _cylinders.Text = Math.Max(1, (long)Math.Ceiling(byteSize / (double)(heads * sectors * bytesPerSector))).ToString();
        }
        UpdateCapacity();
    }

    private void UpdateCapacity()
    {
        _capacity.Text = TryGetByteSize(out var byteSize) ? LocExtension.Get("Emulation.Storage.Geometry.CalculatedCapacity", StorageSizeFormatter.FormatCapacity(byteSize)) : LocExtension.Get("Emulation.Storage.Disk.InvalidSize");
    }

    private bool TryGetByteSize(out long byteSize)
    {
        if (_automaticGeometry.IsChecked == true && !SelectedFormat.SupportsExplicitGeometry) return TryGetSelectedSize(out byteSize);
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
        if ((_sizePreset.SelectedItem as HardDiskCapacityChoice)?.Bytes is long preset)
        {
            byteSize = preset;
            return true;
        }
        if (double.TryParse(_customSize.Text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.CurrentCulture, out var custom) && custom > 0)
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

    private static bool IsSupportedCapacity(long byteSize, HardDiskImageFormat format) => HardDiskImageValidation.IsSupportedCapacity(byteSize, format);

}

