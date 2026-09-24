using GWGUI.MediaEngine.Images.Formats;
using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Enums.Explorer;
using GWGUI.App.Functions.Explorer;
using GWGUI.App.Functions.Storage;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Presenters.Explorer;
using GWGUI.App.ViewModels.Explorer;
using GWGUI.App.Views.Dialogs.Explorer;
using GWGUI.App.Views.Controls.Common;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using GWGUI.MediaEngine.Contracts.Explorer;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Images.Models.Optical;
using GWGUI.MediaEngine.Images.Models.Sequential;
using GWGUI.MediaEngine.Images.Reading.Recognition;


namespace GWGUI.App.Views.Controls.Explorer;

public partial class ExplorerSection : UserControl
{
    private static readonly IReadOnlyList<DiskFormat> BuiltInFormats = new BuiltInImageFormatCatalog().Formats;
    private ExplorerFolderItem? _rootFolder;
    private ExploredDiskImage? _document;
    private ExploredMediaImage? _mediaDocument;
    private ExploredMediaVolume? _mediaVolume;
    private IReadOnlyList<FileSystemEntry> _rootEntries = [];
    private IReadOnlyList<DiskFormat> _formats = [];
    private IReadOnlyList<string> _detectedFormatIds = [];
    private string? _automaticDetectedFormatId;
    private readonly ObservableCollection<ExplorerFolderItem> _visibleFolders = [];
    private bool _updatingVolumeSelector;
    private bool _updatingSessionSelector;
    private bool _updatingDetectedFormatSelector;
    public ExplorerSection()
    {
        InitializeComponent();
        FolderList.ItemsSource = _visibleFolders;
        SetFormats([], null);
        Classification.ValueChanged += (_, _) =>
        {
            AutomaticDetection.IsChecked = false;
            Classification.SetAutomaticDetection(false);
            FormatChanged?.Invoke(this, EventArgs.Empty);
        };
        OpenButton.Click += (_, e) => OpenRequested?.Invoke(this, e);
        EmptyOpenButton.Click += (_, e) => OpenRequested?.Invoke(this, e);
        ReadDiskButton.Click += (_, e) => ReadDiskRequested?.Invoke(this, e);
        SessionSelector.SelectionChanged += SessionSelector_SelectionChanged;
        AudioTrackList.SelectionChanged += AudioTrackList_SelectionChanged;
    }

    public event RoutedEventHandler? OpenRequested;
    public event RoutedEventHandler? ReadDiskRequested;
    public event EventHandler? FormatChanged;
    public Button OpenImageButton => OpenButton;
    public CardSection HeaderCardControl => HeaderCardRoot;
    public DiskClassificationSelector ClassificationSelector => Classification;
    public IReadOnlyList<ExplorerFormatChoice> FormatChoices =>
        [new(null, LocExtension.Get("Explorer.Automatic")), .. _formats.Select(format => new ExplorerFormatChoice(format.Id, format.DisplayName))];
    public void SetReadDiskRunning(bool running) => ReadDiskButton.Content = LocExtension.Get(running ? "Common.Stop" : "Explorer.ReadDisk");
    public string? SelectedFormatId => Classification.SelectedFormatId;
    public string? FormatIdForNewImage => AutomaticDetection.IsChecked == true ? null : SelectedFormatId;

    public void SelectDetectedFormat(string formatId)
    {
        if (DetectedFormatSelector.ItemsSource is not IEnumerable<ExplorerFormatChoice> choices) return;
        var choice = choices.FirstOrDefault(item => string.Equals(item.Id, formatId, StringComparison.OrdinalIgnoreCase));
        if (choice is null) return;
        _updatingDetectedFormatSelector = true;
        DetectedFormatSelector.SelectedItem = choice;
        Classification.ApplyDetection(formatId, null, _detectedFormatIds);
        _updatingDetectedFormatSelector = false;
    }

    private void AutomaticDetection_Changed(object sender, RoutedEventArgs e)
    {
        if (!IsInitialized)
            return;

        var enabled = AutomaticDetection.IsChecked == true;
        Classification.SetAutomaticDetection(enabled);
        if (!enabled) return;

        var formatId = _automaticDetectedFormatId
            ?? _document?.PrimaryFormatId
            ?? _mediaDocument?.Document.FormatId;
        if (string.IsNullOrWhiteSpace(formatId)) return;

        SelectDetectedFormat(formatId);
        FormatChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetFormats(IEnumerable<DiskFormat> formats, string? selectedId)
    {
        var hadSelection = Classification.SelectedFormatId is not null;
        _formats = formats.ToArray();
        Classification.SetCatalog(_formats);
        if (!hadSelection && selectedId is not null) Classification.ApplyDetection(selectedId, null);
    }

    public void SetLoading(bool loading)
    {
        LoadingOverlay.Visibility = loading ? Visibility.Visible : Visibility.Collapsed;
        if (loading)
            SetLoadingProgress(LocExtension.Get("Explorer.Loading"), string.Empty, 0);
    }

    public void SetLoadingProgress(string stage, string detail, double value)
    {
        LoadingCard.SetProgress(stage, detail, value);
    }

    internal string LoadingStage => LoadingCard.Stage;
    internal string LoadingDetail => LoadingCard.Detail;
    internal double LoadingValue => LoadingCard.Value;
    internal string LoadingPercent => LoadingCard.Percent;

}
