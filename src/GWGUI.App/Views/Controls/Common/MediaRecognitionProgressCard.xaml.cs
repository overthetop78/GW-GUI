using System.Windows.Controls;

namespace GWGUI.App.Views.Controls.Common;

public partial class MediaRecognitionProgressCard : UserControl
{
    public MediaRecognitionProgressCard() => InitializeComponent();

    public string Stage => StageText.Text;
    public string Detail => DetailText.Text;
    public double Value => Progress.Value;
    public string Percent => PercentText.Text;

    public void SetProgress(string stage, string detail, double value, bool indeterminate = false)
    {
        StageText.Text = stage;
        DetailText.Text = detail;
        Progress.IsIndeterminate = indeterminate;
        Progress.Value = Math.Clamp(value, 0, 100);
        PercentText.Text = indeterminate ? string.Empty : $"{Progress.Value:0} %";
    }
}
