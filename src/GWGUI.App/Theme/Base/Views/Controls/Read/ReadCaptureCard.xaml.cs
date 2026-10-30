using GWGUI.App.Localization.Extensions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GWGUI.App.Views.Controls.Read;

public partial class ReadCaptureCard : UserControl
{
    private static readonly Brush InProgressBackground = CreateBrush(0xE7, 0xF0, 0xFF);
    private static readonly Brush InProgressBorder = CreateBrush(0x4D, 0x76, 0xE8);
    private static readonly Brush CompletedBackground = CreateBrush(0xE5, 0xF5, 0xEA);
    private static readonly Brush CompletedBorder = CreateBrush(0x42, 0xA5, 0x66);
    private static readonly Brush ArchivedBackground = CreateBrush(0xF1, 0xF3, 0xF5);
    private static readonly Brush ArchivedBorder = CreateBrush(0xC7, 0xCD, 0xD4);

    public ReadCaptureCard(string path)
    {
        InitializeComponent();
        Path = path;
        FileNameText.Text = System.IO.Path.GetFileName(path);
        ExploreButton.Click += (_, _) => ExploreRequested?.Invoke(path);
        VisualizeButton.Click += (_, _) => VisualizeRequested?.Invoke(path);
        SetInProgress();
    }

    public string Path { get; }

    public event Action<string>? ExploreRequested;
    public event Action<string>? VisualizeRequested;

    public void SetCompleted(string summary)
    {
        CardBorder.Background = CompletedBackground;
        CardBorder.BorderBrush = CompletedBorder;
        StateText.Text = LocExtension.Get("Status.Success");
        SummaryText.Text = summary;
        SummaryText.Visibility = string.IsNullOrWhiteSpace(summary) ? Visibility.Collapsed : Visibility.Visible;
        ExploreButton.IsEnabled = true;
        VisualizeButton.IsEnabled = true;
    }

    public void SetArchived()
    {
        CardBorder.Background = ArchivedBackground;
        CardBorder.BorderBrush = ArchivedBorder;
    }

    private void SetInProgress()
    {
        CardBorder.Background = InProgressBackground;
        CardBorder.BorderBrush = InProgressBorder;
        StateText.Text = LocExtension.Get("Status.Running");
    }

    private static SolidColorBrush CreateBrush(byte red, byte green, byte blue)
    {
        var brush = new SolidColorBrush(Color.FromRgb(red, green, blue));
        brush.Freeze();
        return brush;
    }
}
