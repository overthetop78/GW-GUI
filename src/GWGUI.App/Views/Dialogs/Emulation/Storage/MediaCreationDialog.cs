using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Constants.Emulation;
using GWGUI.App.Functions.Views.Emulation.Storage;
using GWGUI.App.Localization.Extensions;

namespace GWGUI.App.Views.Dialogs.Emulation.Storage;

/// <summary>La création et le montage sont deux décisions distinctes ; le montage appartient à l'appelant.</summary>
internal sealed class MediaCreationDialog : Window
{
    private bool _busy;
    private bool _created;
    private readonly StackPanel _buttons = new() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
    private readonly TextBlock _status = new() { TextWrapping = TextWrapping.Wrap };

    /// <summary>Fichier effectivement créé, conservé lorsque la configuration est ensuite verrouillée.</summary>
    internal string? CreatedPath { get; private set; }

    internal MediaCreationDialog(Window owner, string title, string path, Func<Task> create)
        : this(owner, title, new TextBox { Text = path, IsReadOnly = true, TextWrapping = TextWrapping.Wrap }, () => path, create)
    {
    }

    internal MediaCreationDialog(Window owner, string title, FrameworkElement configuration, Func<string> destinationPath, Func<Task> create, Func<bool>? canCreate = null)
    {
        Owner = owner;
        Title = title;
        ShowInTaskbar = false;
        SetResourceReference(StyleProperty, "ApplicationWindowStyle");
        StorageDialogUi.CompactAppearance(this);
        var styles = new ResourceDictionary { Source = new Uri("/gwgui.app;component/Views/Styles/StorageCreationStyles.xaml", UriKind.Relative) };
        Resources.MergedDictionaries.Add(styles);
        Resources[typeof(Button)] = styles["StorageCreationButton"];
        Resources[typeof(TextBox)] = styles["StorageCreationTextBox"];
        Resources[typeof(ComboBox)] = styles["StorageCreationComboBox"];
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Width = Math.Min(MediaCreationConstants.Width, SystemParameters.WorkArea.Width * StorageConfigurationLayoutConstants.WorkAreaWidthRatio);
        SizeToContent = SizeToContent.Height;
        MinWidth = StorageConfigurationLayoutConstants.MinimumWidth;
        MaxHeight = Math.Min(StorageConfigurationLayoutConstants.CompactMaximumHeight, SystemParameters.WorkArea.Height * StorageConfigurationLayoutConstants.WorkAreaHeightRatio);
        ResizeMode = ResizeMode.CanResize;
        var body = new StackPanel();
        body.Children.Add(configuration);
        _status.Margin = new Thickness(0, 10, 0, 0);
        _status.Visibility = Visibility.Collapsed;
        _buttons.Margin = new Thickness(0, 14, 0, 0);
        body.Children.Add(_status);
        var mount = Button(LocExtension.Get(MediaCreationConstants.CreateAndMount));
        var onlyCreate = Button(LocExtension.Get("Common.Create"));
        var cancel = Button(LocExtension.Get("Common.Cancel"));
        cancel.IsCancel = true;
        mount.IsDefault = true;
        mount.Click += async (_, _) => await CreateAsync(true);
        onlyCreate.Click += async (_, _) => await CreateAsync(false);
        void RefreshCommands()
        {
            var ready = canCreate?.Invoke() ?? true;
            mount.IsEnabled = !_busy && (_created || ready);
            onlyCreate.IsEnabled = !_busy && !_created && ready;
        }
        configuration.AddHandler(Selector.SelectionChangedEvent, new SelectionChangedEventHandler((_, _) => RefreshCommands()));
        configuration.AddHandler(TextBox.TextChangedEvent, new TextChangedEventHandler((_, _) => RefreshCommands()));
        RefreshCommands();
        Closing += (_, args) => args.Cancel = _busy;
        var root = StorageDialogUi.DialogLayout(StorageDialogUi.DialogHeader(IconGlyphs.FloppyDisk, title, string.Empty, compact: true),
            new ScrollViewer { Content = body, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled }, _buttons);
        root.Margin = new Thickness(18);
        root.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);
        Content = root;

        async Task CreateAsync(bool mountAfterCreation)
        {
            _busy = true;
            _buttons.IsEnabled = false;
            var completed = false;
            try
            {
                if (!_created)
                {
                    var path = destinationPath();
                    if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException(LocExtension.Get(MediaCreationConstants.PathRequired));
                    if (File.Exists(path)) throw new IOException(LocExtension.Get(MediaCreationConstants.AlreadyExists, path));
                    await create();
                    _created = true;
                    CreatedPath = path;
                    configuration.IsEnabled = false;
                    onlyCreate.IsEnabled = false;
                    cancel.Content = LocExtension.Get("Common.Close");
                    _status.Text = LocExtension.Get(MediaCreationConstants.Created, path);
                    _status.Visibility = Visibility.Visible;
                }
                completed = true;
            }
            catch (Exception error)
            {
                MessageBox.Show(this, error.Message, Title, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                _busy = false;
                _buttons.IsEnabled = true;
                RefreshCommands();
            }
            if (completed && mountAfterCreation) DialogResult = true;
        }
    }

    private Button Button(string text)
    {
        var button = new Button { Content = text, MinWidth = MediaCreationConstants.ButtonWidth };
        _buttons.Children.Add(button);
        return button;
    }
}
