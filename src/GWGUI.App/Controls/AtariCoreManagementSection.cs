using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GWGUI.App.Localization;
using GWGUI.Emulation.Atari;
using GWGUI.Emulation.Atari.Cores;

namespace GWGUI.App.Controls;

public sealed class AtariCoreInstallationChangedEventArgs(AtariCoreKind kind, AtariCoreInstallationPaths paths)
    : EventArgs
{
    public AtariCoreKind Kind { get; } = kind;
    public AtariCoreInstallationPaths Paths { get; } = paths;
}

public sealed class AtariCoreManagementSection : UserControl
{
    private static readonly HttpClient Client = new();
    private readonly IAtariCoreReleaseService _service;
    private readonly Func<string, object[], string> _localize;
    private readonly TextBlock _required = new() { Name = "RequiredCore", TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _installed = new() { Name = "InstalledCore", TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _status = new() { Name = "CoreStatus", TextWrapping = TextWrapping.Wrap };
    private readonly ComboBox _versions = new() { Name = "CoreVersions", MinWidth = 320, Visibility = Visibility.Collapsed };
    private readonly Button _search = new() { Name = "SearchCoreVersions", MinWidth = 140 };
    private readonly Button _download = new() { Name = "DownloadCoreVersion", MinWidth = 170, Visibility = Visibility.Collapsed };
    private readonly Button _cancel = new() { Name = "CancelCoreDownload", MinWidth = 100, Visibility = Visibility.Collapsed };
    private readonly ProgressBar _progress = new()
    {
        Name = "CoreDownloadProgress", Minimum = AtariCoreManagementConstants.InitialProgress,
        Maximum = AtariCoreManagementConstants.CompletedProgress, Height = 8,
        Visibility = Visibility.Collapsed
    };
    private readonly Border _statusBanner = new()
    {
        CornerRadius = new CornerRadius(6), BorderThickness = new Thickness(1),
        Padding = new Thickness(10, 7, 10, 7), Visibility = Visibility.Collapsed
    };
    private CancellationTokenSource? _downloadCancellation;
    private AtariCoreKind _kind;

    public event EventHandler<AtariCoreInstallationChangedEventArgs>? InstallationChanged;

    public AtariCoreManagementSection() : this(
        new AtariCoreReleaseService(Client, StoragePaths.AtariCoreDirectory),
        static (key, arguments) => LocExtension.Get(key, arguments))
    {
    }

    internal AtariCoreManagementSection(IAtariCoreReleaseService service,
        Func<string, object[], string> localize)
    {
        _service = service;
        _localize = localize;
        _search.Content = AtariCoreManagementFunctions.CreateButtonContent(
            AtariCoreManagementConstants.SearchGlyph, L(AtariCoreManagementConstants.SearchResource));
        _download.Content = AtariCoreManagementFunctions.CreateButtonContent(
            AtariCoreManagementConstants.DownloadGlyph, L(AtariCoreManagementConstants.DownloadResource));
        _cancel.Content = L(AtariCoreManagementConstants.CancelResource);
        Content = BuildContent();
        _search.Click += async (_, _) => await SearchAsync();
        _download.Click += async (_, _) => await DownloadSelectedAsync();
        _cancel.Click += (_, _) => _downloadCancellation?.Cancel();
    }

    internal AtariCoreKind RequiredKind => _kind;
    internal string RequiredText => _required.Text;
    internal string InstalledText => _installed.Text;
    internal string StatusText => _status.Text;
    internal Visibility VersionsVisibility => _versions.Visibility;
    internal Visibility DownloadVisibility => _download.Visibility;
    internal Visibility CancelVisibility => _cancel.Visibility;
    internal Visibility ProgressVisibility => _progress.Visibility;
    internal IReadOnlyList<AtariCoreRelease> Versions => _versions.Items.Cast<AtariCoreRelease>().ToArray();
    internal void SelectVersion(int index) => _versions.SelectedIndex = index;

    public async Task SetModelAsync(AtariMachineModel model, CancellationToken cancellationToken = default)
    {
        _kind = AtariCoreCatalog.Get(model).Kind;
        _required.Text = L(AtariCoreManagementConstants.RequiredForModelResource,
            AtariCoreCatalog.Get(_kind).LibraryName)
            + AtariCoreManagementConstants.DetailSeparator + model;
        HideResults();
        SetStatus(L(AtariCoreManagementConstants.SearchResource));
        await RefreshInstalledStateAsync(cancellationToken);
    }

    internal async Task SearchAsync(CancellationToken cancellationToken = default)
    {
        HideResults();
        SetStatus(L(AtariCoreManagementConstants.SearchingResource));
        _search.IsEnabled = false;
        try
        {
            var releases = await _service.GetAvailableAsync(_kind, cancellationToken);
            if (releases.Count == 0)
            {
                SetStatus(L(AtariCoreManagementConstants.NoneFoundResource));
                return;
            }
            _versions.ItemsSource = releases;
            _versions.SelectedIndex = AtariCoreManagementConstants.FirstVersionIndex;
            _versions.Visibility = Visibility.Visible;
            _download.Visibility = Visibility.Visible;
            SetStatus(L(AtariCoreManagementConstants.VersionsFoundResource, releases.Count));
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            SetStatus(ControlErrorPresenter.Describe(error, ControlErrorContexts.AtariCoreManagement),
                isError: true);
        }
        finally
        {
            _search.IsEnabled = true;
        }
    }

    internal async Task DownloadSelectedAsync()
    {
        if (_versions.SelectedItem is not AtariCoreRelease release) return;
        _downloadCancellation?.Dispose();
        var cancellation = new CancellationTokenSource();
        _downloadCancellation = cancellation;
        SetBusy(true);
        SetStatus(L(AtariCoreManagementConstants.DownloadingResource, release.DeclaredVersion));
        var progress = AtariCoreManagementFunctions.CreateDispatcherProgress<AtariCoreInstallProgress>(
            Dispatcher, value =>
        {
            if (value.Fraction is { } fraction) _progress.Value = fraction;
        });
        try
        {
            var paths = await _service.InstallAsync(release, progress, cancellation.Token);
            await RefreshInstalledStateAsync(CancellationToken.None);
            SetStatus(L(AtariCoreManagementConstants.InstalledPathResource, paths.LibraryPath));
            InstallationChanged?.Invoke(this, new AtariCoreInstallationChangedEventArgs(release.Kind, paths));
        }
        catch (OperationCanceledException)
        {
            SetStatus(L(AtariCoreManagementConstants.CancelledResource));
        }
        catch (Exception error)
        {
            SetStatus(ControlErrorPresenter.Describe(error, ControlErrorContexts.AtariCoreManagement),
                isError: true);
        }
        finally
        {
            SetBusy(false);
            if (ReferenceEquals(_downloadCancellation, cancellation))
            {
                _downloadCancellation.Dispose();
                _downloadCancellation = null;
            }
        }
    }

    private UIElement BuildContent()
    {
        var root = new Grid { Margin = new Thickness(16) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var heading = new Grid { Margin = new Thickness(0, 0, 0, 12) };
        heading.ColumnDefinitions.Add(new ColumnDefinition());
        heading.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var identity = new StackPanel { Orientation = Orientation.Horizontal };
        identity.Children.Add(new TextBlock
        {
            Text = AtariCoreManagementConstants.CoreGlyph, FontFamily = ControlVisualConstants.IconFont,
            FontSize = 18, Margin = new Thickness(0, 0, 8, 0)
        });
        identity.Children.Add(_required);
        heading.Children.Add(identity);
        Grid.SetColumn(_search, 1);
        heading.Children.Add(_search);
        root.Children.Add(heading);

        var installedCard = new Border
        {
            Child = _installed, Padding = new Thickness(12, 8, 12, 8), CornerRadius = new CornerRadius(6),
            BorderThickness = new Thickness(1), Margin = new Thickness(0, 0, 0, 10)
        };
        installedCard.SetResourceReference(BackgroundProperty, "WindowBrush");
        installedCard.SetResourceReference(BorderBrushProperty, "BorderBrush");
        Grid.SetRow(installedCard, 1);
        root.Children.Add(installedCard);

        var selection = new Grid { Margin = new Thickness(0, 0, 0, 10) };
        selection.ColumnDefinitions.Add(new ColumnDefinition());
        selection.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        selection.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        _versions.Margin = new Thickness(0, 0, 10, 0);
        selection.Children.Add(_versions);
        Grid.SetColumn(_download, 1);
        selection.Children.Add(_download);
        _cancel.Margin = new Thickness(10, 0, 0, 0);
        Grid.SetColumn(_cancel, 2);
        selection.Children.Add(_cancel);
        Grid.SetRow(selection, 2);
        root.Children.Add(selection);

        var status = new Grid();
        status.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        status.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _statusBanner.Child = _status;
        status.Children.Add(_statusBanner);
        _progress.Margin = new Thickness(0, 6, 0, 0);
        Grid.SetRow(_progress, 1);
        status.Children.Add(_progress);
        Grid.SetRow(status, 3);
        root.Children.Add(status);
        return root;
    }

    private async Task RefreshInstalledStateAsync(CancellationToken cancellationToken)
    {
        var paths = await _service.GetActiveInstallationAsync(_kind, cancellationToken);
        _installed.Text = paths is null
            ? L(AtariCoreManagementConstants.NotInstalledResource)
            : L(AtariCoreManagementConstants.InstalledResource, Path.GetFileName(paths.VersionDirectory))
              + AtariCoreManagementConstants.DetailLineSeparator
              + L(AtariCoreManagementConstants.InstalledPathResource, paths.LibraryPath);
    }

    private void HideResults()
    {
        _versions.ItemsSource = null;
        _versions.Visibility = Visibility.Collapsed;
        _download.Visibility = Visibility.Collapsed;
    }

    private void SetBusy(bool busy)
    {
        _search.IsEnabled = !busy;
        _download.IsEnabled = !busy;
        _cancel.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
        _progress.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
        if (busy) _progress.Value = AtariCoreManagementConstants.InitialProgress;
    }

    private void SetStatus(string text, bool isError = false)
    {
        _status.Text = text;
        _statusBanner.Visibility = string.IsNullOrWhiteSpace(text) ? Visibility.Collapsed : Visibility.Visible;
        if (isError)
        {
            _statusBanner.Background = new SolidColorBrush(Color.FromRgb(
                AtariCoreManagementConstants.ErrorBackgroundRed,
                AtariCoreManagementConstants.ErrorBackgroundGreen,
                AtariCoreManagementConstants.ErrorBackgroundBlue));
            _statusBanner.BorderBrush = new SolidColorBrush(Color.FromRgb(
                AtariCoreManagementConstants.ErrorBorderRed,
                AtariCoreManagementConstants.ErrorBorderGreen,
                AtariCoreManagementConstants.ErrorBorderBlue));
            _status.Foreground = new SolidColorBrush(Color.FromRgb(
                AtariCoreManagementConstants.ErrorTextRed,
                AtariCoreManagementConstants.ErrorTextGreen,
                AtariCoreManagementConstants.ErrorTextBlue));
        }
        else
        {
            _statusBanner.SetResourceReference(BackgroundProperty, "WindowBrush");
            _statusBanner.SetResourceReference(BorderBrushProperty, "BorderBrush");
            _status.SetResourceReference(ForegroundProperty, "TextBrush");
        }
    }

    private string L(string key, params object[] arguments) => _localize(key, arguments);
}
