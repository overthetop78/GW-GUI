using System.Windows;
using System.Windows.Controls;

namespace GWGUI.App.Controls;

public partial class ConversionTabSection : UserControl
{
    public ConversionTabSection()
    {
        InitializeComponent();
        ExecuteButton.Click += (_, e) => ExecuteRequested?.Invoke(this, e);
    }

    public event RoutedEventHandler? ExecuteRequested;

    public ConversionAdvancedSection AdvancedBlock => AdvancedSection;
    public PathSection SourceBlock => SourceSection;
    public ProfileSection ProfileBlock => ProfileSection;
    public ConversionOutputSection OutputBlock => OutputSection;
    public ConversionFormatsSection FormatsBlock => FormatsSection;
    public Button ExecuteActionButton => ExecuteButton;
}
