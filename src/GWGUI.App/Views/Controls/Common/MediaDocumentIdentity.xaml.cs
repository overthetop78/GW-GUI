using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Rendering.Media;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Formats;
using GWGUI.MediaEngine.Images.Visualization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GWGUI.App.Views.Controls.Common;

public partial class MediaDocumentIdentity : UserControl
{
    public MediaDocumentIdentity() => InitializeComponent();

    public TextBlock FileNameText => FileName;
    public TextBlock SummaryText => Summary;
    public string MediaIconKind { get; private set; } = MediaIconIds.File;

    public void Display(
        string path,
        string summary,
        MediaKind? mediaKind,
        DiskFormat? format = null)
    {
        FileName.Text = Path.GetFileName(path);
        FileName.ToolTip = path;
        Summary.Text = summary;
        ApplyIcon(MediaIconSelector.Select(mediaKind, path, format));
    }

    private void ApplyIcon(string iconId)
    {
        var presentation = MediaIconVisualConstants.For(iconId);
        MediaIconKind = iconId;
        if (presentation.ImageFile is { } imageFile)
        {
            IconImage.Source = MediaIconImageCache.Load(imageFile);
            IconImage.Visibility = Visibility.Visible;
            IconVector.Visibility = Visibility.Collapsed;
        }
        else
        {
            IconImage.Source = null;
            IconImage.Visibility = Visibility.Collapsed;
            IconVector.Visibility = Visibility.Visible;
            Icon.Data = Geometry.Parse(presentation.Geometry);
            Icon.Fill = (Brush)new BrushConverter().ConvertFromString(presentation.Foreground)!;
        }
        IconTile.Background = (Brush)new BrushConverter().ConvertFromString(presentation.Background)!;
    }
}
