using System.IO;
using System.Windows;
using System.Windows.Controls;
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

    internal MediaCreationDialog(Window owner, string title, FrameworkElement configuration, Func<string> destinationPath, Func<Task> create)
    {
        Owner = owner;
        Title = title;
        ShowInTaskbar = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Width = Math.Min(MediaCreationConstants.Width, SystemParameters.WorkArea.Width * StorageConfigurationLayoutConstants.WorkAreaWidthRatio);
        SizeToContent = SizeToContent.Height;
        ResizeMode = ResizeMode.NoResize;
        var body = new StackPanel();
        body.Children.Add(configuration);
        body.Children.Add(_status);
        var mount = Button(LocExtension.Get(MediaCreationConstants.CreateAndMount));
        var onlyCreate = Button(LocExtension.Get("Common.Create"));
        var close = Button(LocExtension.Get("Common.Close"));
        var cancel = Button(LocExtension.Get("Common.Cancel"));
        cancel.IsCancel = true;
        mount.IsDefault = true;
        close.Click += (_, _) => DialogResult = false;
        mount.Click += async (_, _) => await CreateAsync(true);
        onlyCreate.Click += async (_, _) => await CreateAsync(false);
        Closing += (_, args) => args.Cancel = _busy;
        Content = StorageDialogUi.DialogLayout(StorageDialogUi.DialogHeader(IconGlyphs.FloppyDisk, title, string.Empty), body, _buttons);

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
                    cancel.IsEnabled = false;
                    _status.Text = LocExtension.Get(MediaCreationConstants.Created, path);
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
