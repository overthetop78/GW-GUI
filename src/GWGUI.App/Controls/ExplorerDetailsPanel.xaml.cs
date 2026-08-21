using System.Windows;
using GWGUI.MediaEngine.Exploration.Results;
using System.Windows.Controls;
using System.Windows.Media;
using GWGUI.App.Localization;
using GWGUI.App.Contracts;
using GWGUI.App.Enums;
using GWGUI.App.ViewModels;

namespace GWGUI.App.Controls;

public partial class ExplorerDetailsPanel : UserControl
{
    private ExploredDiskImage? _document;
    private ExplorerContentItem? _item;
    private string? _currentSystem;

    public ExplorerDetailsPanel()
    {
        InitializeComponent();
        Loaded += (_, _) => System.ComponentModel.PropertyChangedEventManager.AddHandler(LocalizationSource.Instance, LocalizationChanged, "Item[]");
        Unloaded += (_, _) => System.ComponentModel.PropertyChangedEventManager.RemoveHandler(LocalizationSource.Instance, LocalizationChanged, "Item[]");
        Clear();
    }

    public string DisplayedTitle => DetailsTitle.Text;
    public bool IsShowingDisk => _document is not null && _item is null;

    public void Clear()
    {
        _document = null;
        _item = null;
        _currentSystem = null;
        DetailsIcon.Category = ExplorerIconCategory.DiskImage;
        DetailsTitle.Text = "\u2014";
        DetailsTitle.Foreground = BrushFor(false);
        SetRows([]);
    }

    public void ShowDisk(ExploredDiskImage document, string? currentSystem = null)
    {
        _document = document;
        _item = null;
        _currentSystem = currentSystem;
        Render();
    }

    public void ShowItem(ExploredDiskImage document, ExplorerContentItem item)
    {
        _document = document;
        _item = item;
        Render();
    }

    private void LocalizationChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (Dispatcher.CheckAccess()) Render();
        else _ = Dispatcher.BeginInvoke(Render);
    }

    private void Render()
    {
        if (_document is null)
        {
            Clear();
            return;
        }

        if (_item is null)
        {
            RenderDisk(_document);
            return;
        }

        RenderItem(_item);
    }

    private void RenderDisk(ExploredDiskImage document)
    {
        Apply(ExplorerDetailsPresenter.ForDisk(document, _currentSystem));
    }

    private void RenderItem(ExplorerContentItem item)
    {
        Apply(ExplorerDetailsPresenter.ForItem(item));
    }

    private void Apply(ExplorerDetailsPresentation presentation)
    {
        DetailsIcon.Category = presentation.IconCategory;
        DetailsTitle.Text = presentation.Title;
        DetailsTitle.Foreground = BrushFor(presentation.IsSyntheticTitle);
        SetRows(presentation.Rows.Select(row => ((string?)row.Key, (string?)row.Value, row.IsSyntheticValue)).ToArray());
    }

    private Brush BrushFor(bool synthetic)
    {
        var resourceKey = synthetic ? "SyntheticNameBrush" : "TextBrush";
        return TryFindResource(resourceKey) as Brush ?? SystemColors.WindowTextBrush;
    }

    private void SetRows(IReadOnlyList<(string? Key, string? Value, bool IsSynthetic)> values)
    {
        var rows = new[] { DetailRow1, DetailRow2, DetailRow3, DetailRow4, DetailRow5, DetailRow6, DetailRow7, DetailRow8 };
        var labels = new[] { DetailLabel1, DetailLabel2, DetailLabel3, DetailLabel4, DetailLabel5, DetailLabel6, DetailLabel7, DetailLabel8 };
        var displayedValues = new[] { DetailValue1, DetailValue2, DetailValue3, DetailValue4, DetailValue5, DetailValue6, DetailValue7, DetailValue8 };
        for (var index = 0; index < rows.Length; index++)
        {
            var visible = index < values.Count && values[index].Key is not null;
            rows[index].Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            if (!visible)
            {
                continue;
            }

            labels[index].Text = LocExtension.Get(values[index].Key!);
            displayedValues[index].Text = string.IsNullOrWhiteSpace(values[index].Value) ? "\u2014" : values[index].Value;
            displayedValues[index].Foreground = BrushFor(values[index].IsSynthetic);
        }
    }
}
