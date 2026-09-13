using GWGUI.Domain.Enums;
using GWGUI.Domain.Formats;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GWGUI.App.Views.Controls.Common;

public partial class MediaDocumentIdentity : UserControl
{
    private static readonly Dictionary<string, ImageSource> MediaImages = new(StringComparer.OrdinalIgnoreCase);
    private const string FloppyGeometry = "M3,2 H17 L21,6 V22 H3 Z M6,3 V9 H16 V3 Z M6,14 H18 V20 H6 Z M9,15 V19 H15 V15 Z";
    private const string FiveAndQuarterFloppyGeometry = "F0 M3,2 H19 L21,4 V22 H3 Z M6,4 H16 V8 H6 Z M8,14 A4,4 0 1 0 16,14 A4,4 0 1 0 8,14 M11,14 A1,1 0 1 0 13,14 A1,1 0 1 0 11,14 M17,9 A1,1 0 1 0 19,9 A1,1 0 1 0 17,9";
    private const string HardDiskGeometry = "M3,4 H21 V20 H3 Z M5,6 H19 V14 H5 Z M6,17 A1,1 0 1 0 8,17 A1,1 0 1 0 6,17 M10,16 H18 V18 H10 Z";
    private const string OpticalGeometry = "M12,2 A10,10 0 1 0 12,22 A10,10 0 1 0 12,2 M12,9 A3,3 0 1 0 12,15 A3,3 0 1 0 12,9 M14,4 L13,8 A4,4 0 0 1 16,11 L20,10 A8,8 0 0 0 14,4 Z";
    private const string CassetteGeometry = "M2,5 H22 V19 H2 Z M5,8 H19 V13 H5 Z M7,15 A2,2 0 1 0 11,15 A2,2 0 1 0 7,15 M13,15 A2,2 0 1 0 17,15 A2,2 0 1 0 13,15 M7,18 L9,14 H15 L17,18 Z";
    private const string TapeGeometry = "M6,3 A5,5 0 1 0 6,13 A5,5 0 1 0 6,3 M18,3 A5,5 0 1 0 18,13 A5,5 0 1 0 18,3 M6,6 A2,2 0 1 0 6,10 A2,2 0 1 0 6,6 M18,6 A2,2 0 1 0 18,10 A2,2 0 1 0 18,6 M6,13 H18 V21 H6 Z";
    private const string CartridgeGeometry = "M5,2 H19 V16 L16,22 H8 L5,16 Z M8,5 H16 V12 H8 Z M9,17 H15 L14,20 H10 Z";
    private const string FileGeometry = "M4,2 H15 L20,7 V22 H4 Z M14,3 V8 H19";

    public MediaDocumentIdentity() => InitializeComponent();

    public TextBlock FileNameText => FileName;
    public TextBlock SummaryText => Summary;
    public string MediaIconKind { get; private set; } = "file";

    public void Display(
        string path,
        string summary,
        MediaKind? mediaKind,
        DiskFormat? format = null)
    {
        FileName.Text = Path.GetFileName(path);
        FileName.ToolTip = path;
        Summary.Text = summary;
        ApplyIcon(mediaKind, path, format);
    }

    private void ApplyIcon(MediaKind? mediaKind, string path, DiskFormat? format)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();
        var presentation = mediaKind switch
        {
            MediaKind.Floppy when format?.FormFactor == FloppyFormFactor.ThreeInch =>
                ("floppy-3", FloppyGeometry, "#FF24658A", "#FFE4EDF5", "floppy-3.png"),
            MediaKind.Floppy when format?.FormFactor == FloppyFormFactor.ThreeAndHalfInch &&
                                  format.Density == FloppyDensity.ExtendedDensity =>
                ("floppy-3.5-ed", FloppyGeometry, "#FF24658A", "#FFE4EDF5", "floppy-3.5-ed-black.png"),
            MediaKind.Floppy when format?.FormFactor == FloppyFormFactor.ThreeAndHalfInch &&
                                  format.Density == FloppyDensity.HighDensity =>
                ("floppy-3.5-hd", FloppyGeometry, "#FF24658A", "#FFE4EDF5", "floppy-3.5-hd-black.png"),
            MediaKind.Floppy when format?.FormFactor == FloppyFormFactor.ThreeAndHalfInch =>
                ("floppy-3.5-dd", FloppyGeometry, "#FF24658A", "#FFE4EDF5", "floppy-3.5-dd-blue.png"),
            MediaKind.Floppy when format?.FormFactor == FloppyFormFactor.FiveAndQuarterInch =>
                ("floppy-5.25", FiveAndQuarterFloppyGeometry, "#FF24658A", "#FFE4EDF5", "floppy-5.25.png"),
            MediaKind.Floppy when format?.FormFactor == FloppyFormFactor.EightInch =>
                ("floppy-8", FloppyGeometry, "#FF24658A", "#FFE4EDF5", "floppy-8.png"),
            MediaKind.Floppy => ("floppy", FloppyGeometry, "#FF24658A", "#FFE4EDF5", (string?)null),
            MediaKind.HardDisk => ("hard-disk", HardDiskGeometry, "#FF77572B", "#FFF2E9DA", (string?)null),
            MediaKind.Optical => ("optical", OpticalGeometry, "#FF6B4BB6", "#FFEDE8F8", (string?)null),
            MediaKind.Tape when extension is ".cas" or ".cdt" or ".tzx" or ".wav" => ("cassette", CassetteGeometry, "#FFF4F7F9", "#FF344A57", "cassette-data.png"),
            MediaKind.Tape => ("tape", TapeGeometry, "#FF9A3D68", "#FFF5E3EC", (string?)null),
            _ when extension is ".crt" or ".car" or ".rom" or ".a26" or ".a52" or ".a78" or ".nes" => ("cartridge", CartridgeGeometry, "#FF3F7C48", "#FFE3F1E5", (string?)null),
            _ when extension is ".iso" or ".cue" or ".ccd" or ".mds" => ("optical", OpticalGeometry, "#FF6B4BB6", "#FFEDE8F8", (string?)null),
            _ => ("file", FileGeometry, "#FF24658A", "#FFE4EDF5", (string?)null)
        };
        MediaIconKind = presentation.Item1;
        if (presentation.Item5 is { } imageFile)
        {
            IconImage.Source = LoadMediaImage(imageFile);
            IconImage.Visibility = Visibility.Visible;
            IconVector.Visibility = Visibility.Collapsed;
        }
        else
        {
            IconImage.Source = null;
            IconImage.Visibility = Visibility.Collapsed;
            IconVector.Visibility = Visibility.Visible;
            Icon.Data = Geometry.Parse(presentation.Item2);
            Icon.Fill = (Brush)new BrushConverter().ConvertFromString(presentation.Item3)!;
        }
        IconTile.Background = (Brush)new BrushConverter().ConvertFromString(presentation.Item4)!;
    }

    private static ImageSource LoadMediaImage(string fileName)
    {
        if (MediaImages.TryGetValue(fileName, out var cached))
            return cached;

        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.UriSource = new Uri(
            $"pack://application:,,,/gwgui.app;component/Assets/Media/{fileName}",
            UriKind.Absolute);
        image.EndInit();
        image.Freeze();
        MediaImages[fileName] = image;
        return image;
    }
}
