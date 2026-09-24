using System.Windows;
using System.Windows.Controls;

namespace GWGUI.App.Views.Controls.Read;

public partial class ReadCompletionBanner : UserControl
{
    private readonly TextBlock fallbackSummary = new() { TextWrapping = TextWrapping.Wrap };
    private ReadCaptureCard? currentCapture;

    public ReadCompletionBanner()
    {
        InitializeComponent();
    }

    public event Action<string>? ExploreRequested;
    public event Action<string>? VisualizeRequested;
    public TextBlock SummaryTextBlock => currentCapture?.FindName("SummaryText") as TextBlock ?? fallbackSummary;

    public void BeginCapture(string path)
    {
        currentCapture?.SetArchived();

        var capture = new ReadCaptureCard(path);
        capture.ExploreRequested += requestedPath => ExploreRequested?.Invoke(requestedPath);
        capture.VisualizeRequested += requestedPath => VisualizeRequested?.Invoke(requestedPath);
        CaptureHistoryPanel.Children.Insert(0, capture);
        currentCapture = capture;
        Visibility = Visibility.Visible;
    }

    public void CompleteCapture(string path, string summary)
    {
        if (currentCapture is null || !string.Equals(currentCapture.Path, path, StringComparison.OrdinalIgnoreCase))
            return;

        currentCapture.SetCompleted(summary);
    }

    public void DiscardCapture(string path)
    {
        if (currentCapture is null || !string.Equals(currentCapture.Path, path, StringComparison.OrdinalIgnoreCase))
            return;

        CaptureHistoryPanel.Children.Remove(currentCapture);
        currentCapture = null;
        if (CaptureHistoryPanel.Children.Count == 0)
            Visibility = Visibility.Collapsed;
    }
}
