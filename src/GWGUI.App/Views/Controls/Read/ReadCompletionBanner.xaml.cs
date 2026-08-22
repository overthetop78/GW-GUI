using System.Windows;
using System.Windows.Controls;

namespace GWGUI.App.Views.Controls.Read;

public partial class ReadCompletionBanner : UserControl
{
    public ReadCompletionBanner()
    {
        InitializeComponent();
        ExploreButton.Click += (_, e) => ExploreRequested?.Invoke(this, e);
        VisualizeButton.Click += (_, e) => VisualizeRequested?.Invoke(this, e);
    }

    public event RoutedEventHandler? ExploreRequested;
    public event RoutedEventHandler? VisualizeRequested;

    public TextBlock SummaryTextBlock => SummaryText;
}
