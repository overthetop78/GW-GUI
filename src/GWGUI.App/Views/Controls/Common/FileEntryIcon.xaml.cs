using GWGUI.App.Enums.Explorer;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GWGUI.App.Views.Controls.Common;

public partial class FileEntryIcon : UserControl
{
    private static readonly Dictionary<(string Name, int Size), ImageSource> Cache = [];

    public static readonly DependencyProperty CategoryProperty = DependencyProperty.Register(
        nameof(Category), typeof(ExplorerIconCategory), typeof(FileEntryIcon),
        new PropertyMetadata(ExplorerIconCategory.File, (owner, _) => ((FileEntryIcon)owner).Refresh()));

    public static readonly DependencyProperty ToneProperty = DependencyProperty.Register(
        nameof(Tone), typeof(ExplorerEntryTone), typeof(FileEntryIcon),
        new PropertyMetadata(ExplorerEntryTone.Default, (owner, _) => ((FileEntryIcon)owner).Refresh()));

    public FileEntryIcon()
    {
        InitializeComponent();
        SizeChanged += (_, _) => Refresh();
        Refresh();
    }

    public ExplorerIconCategory Category
    {
        get => (ExplorerIconCategory)GetValue(CategoryProperty);
        set => SetValue(CategoryProperty, value);
    }

    public ExplorerEntryTone Tone
    {
        get => (ExplorerEntryTone)GetValue(ToneProperty);
        set => SetValue(ToneProperty, value);
    }

    private void Refresh()
    {
        if (!IsInitialized) return;
        var source = LoadIcon(FileName(Category), Math.Max(20, (int)Math.Ceiling(Math.Max(ActualWidth, ActualHeight))));
        NativeIcon.Source = source;
        NativeIcon.Visibility = Tone == ExplorerEntryTone.Default ? Visibility.Visible : Visibility.Collapsed;
        TintedIcon.Visibility = Tone == ExplorerEntryTone.Default ? Visibility.Collapsed : Visibility.Visible;
        TintedIcon.Fill = Tone == ExplorerEntryTone.Executable
            ? new SolidColorBrush(Color.FromRgb(24, 134, 75))
            : TryFindResource("MutedTextBrush") as Brush ?? SystemColors.GrayTextBrush;
        TintedIcon.OpacityMask = new ImageBrush(source) { Stretch = Stretch.Uniform };
    }

    private static ImageSource LoadIcon(string fileName, int requestedSize)
    {
        var cacheSize = requestedSize <= 20 ? 20 : 64;
        lock (Cache)
        {
            if (Cache.TryGetValue((fileName, cacheSize), out var cached)) return cached;
            var uri = new Uri($"pack://application:,,,/GWGUI.App;component/Assets/Icons/FileTypes/{fileName}", UriKind.Absolute);
            var info = Application.GetResourceStream(uri) ?? throw new FileNotFoundException(uri.ToString());
            using var stream = info.Stream;
            var decoder = new IconBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            var frame = decoder.Frames
                .OrderBy(candidate => Math.Abs(candidate.PixelWidth - cacheSize))
                .ThenByDescending(candidate => candidate.PixelWidth)
                .First();
            frame.Freeze();
            Cache[(fileName, cacheSize)] = frame;
            return frame;
        }
    }

    internal static string FileName(ExplorerIconCategory category) => category switch
    {
        ExplorerIconCategory.Folder => "folder.ico",
        ExplorerIconCategory.Text => "text.ico",
        ExplorerIconCategory.Document => "document.ico",
        ExplorerIconCategory.SourceCode => "source-code.ico",
        ExplorerIconCategory.BasicProgram => "source-code.ico",
        ExplorerIconCategory.Executable => "executable.ico",
        ExplorerIconCategory.Command => "command.ico",
        ExplorerIconCategory.System => "system-chip.ico",
        ExplorerIconCategory.ObjectCode => "object-code.ico",
        ExplorerIconCategory.Data => "data.ico",
        ExplorerIconCategory.Configuration => "configuration.ico",
        ExplorerIconCategory.Library => "library.ico",
        ExplorerIconCategory.Image => "image.ico",
        ExplorerIconCategory.Audio => "audio-note.ico",
        ExplorerIconCategory.Media => "media.ico",
        ExplorerIconCategory.Archive => "archive.ico",
        ExplorerIconCategory.DiskImage => "disc-image.ico",
        ExplorerIconCategory.Link => "shortcut.ico",
        ExplorerIconCategory.Font => "font.ico",
        ExplorerIconCategory.Program => "executable.ico",
        _ => "file.ico"
    };
}
