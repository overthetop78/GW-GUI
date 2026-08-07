using GWGUI.App.Localization;

namespace GWGUI.App.Controls;
public partial class VisualizerTrackOverview : System.Windows.Controls.UserControl
{
 public VisualizerTrackOverview() => InitializeComponent();
 public void Configure(IReadOnlyDictionary<int,IReadOnlyList<int>> cylinders)
 {
  Visibility=cylinders.Values.Any(items=>items.Count>0)?System.Windows.Visibility.Visible:System.Windows.Visibility.Collapsed;
  ConfigureFace(Face0,Face0Count,0,cylinders.GetValueOrDefault(0) ?? []);
  ConfigureFace(Face1,Face1Count,1,cylinders.GetValueOrDefault(1) ?? []);
 }
 public void MarkPrepared(int head,int count)
 {
  var strip=head==0?Face0:Face1; var label=head==0?Face0Count:Face1Count;
  for(var index=0;index<strip.Segments.Count;index++) strip.SetState(strip.Segments[index].Cylinder,index<count?TrackSegmentState.Success:TrackSegmentState.Pending);
  label.Text=$"{Math.Min(count,strip.Segments.Count)} / {strip.Segments.Count}";
 }
 private static void ConfigureFace(TrackProgressStrip strip,System.Windows.Controls.TextBlock label,int head,IReadOnlyList<int> cylinders)
 {
  strip.Visibility=cylinders.Count==0?System.Windows.Visibility.Collapsed:System.Windows.Visibility.Visible;
  label.Visibility=strip.Visibility; strip.Configure(head,cylinders,LocExtension.Get("Visual.Side",head)); label.Text=$"0 / {cylinders.Count}";
 }
}
