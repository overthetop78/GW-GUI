using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Localization;
using GWGUI.Scp.Images;

namespace GWGUI.App.Controls;

public partial class ExplorerDetailsPanel : UserControl
{
    private ExploredDiskImage? _document;
    private ExplorerContentItem? _item;

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
        DetailsIcon.Kind = ExplorerIconKind.DiskImage;
        DetailsTitle.Text = "\u2014";
        SetRows([]);
    }

    public void ShowDisk(ExploredDiskImage document)
    {
        _document = document;
        _item = null;
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
        if (_document is null) { Clear(); return; }
        if (_item is null) RenderDisk(_document);
        else RenderItem(_item);
    }

    private void RenderDisk(ExploredDiskImage document)
    {
        Apply(ExplorerDetailsPresenter.ForDisk(document));
    }

    private void RenderItem(ExplorerContentItem item)
    {
        Apply(ExplorerDetailsPresenter.ForItem(item));
    }

    private void Apply(ExplorerDetailsPresentation presentation)
    {
        DetailsIcon.Kind = presentation.IconKind;
        DetailsTitle.Text = presentation.Title;
        SetRows(presentation.Rows.Select(row => ((string?)row.Key, (string?)row.Value)).ToArray());
    }

    private void SetRows(IReadOnlyList<(string? Key, string? Value)> values)
    {
        var rows = new[] { DetailRow1, DetailRow2, DetailRow3, DetailRow4, DetailRow5, DetailRow6, DetailRow7, DetailRow8 };
        var labels = new[] { DetailLabel1, DetailLabel2, DetailLabel3, DetailLabel4, DetailLabel5, DetailLabel6, DetailLabel7, DetailLabel8 };
        var displayedValues = new[] { DetailValue1, DetailValue2, DetailValue3, DetailValue4, DetailValue5, DetailValue6, DetailValue7, DetailValue8 };
        for (var index = 0; index < rows.Length; index++)
        {
            var visible = index < values.Count && values[index].Key is not null;
            rows[index].Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            if (!visible) continue;
            labels[index].Text = LocExtension.Get(values[index].Key!);
            displayedValues[index].Text = string.IsNullOrWhiteSpace(values[index].Value) ? "\u2014" : values[index].Value;
        }
    }
}
