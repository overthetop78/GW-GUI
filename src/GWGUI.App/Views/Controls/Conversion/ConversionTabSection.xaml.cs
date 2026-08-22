using GWGUI.App.Views.Controls.Common;
using GWGUI.App.Views.Controls.Options;
using System.Windows;
using System.Windows.Controls;


namespace GWGUI.App.Views.Controls.Conversion;

public partial class ConversionTabSection : UserControl
{
    public ConversionTabSection()
    {
        InitializeComponent();
        ExecuteButton.Click += (_, e) => ExecuteRequested?.Invoke(this, e);
        MigrationButton.Click += (_, e) => MigrationRequested?.Invoke(this, e);
    }

    public event RoutedEventHandler? ExecuteRequested;
    public event RoutedEventHandler? MigrationRequested;

    public ConversionAdvancedSection AdvancedBlock => AdvancedSection;
    public PathSection SourceBlock => SourceSection;
    public ProfileSection ProfileBlock => ProfileSection;
    public ConversionOutputSection OutputBlock => OutputSection;
    public ConversionFormatsSection FormatsBlock => FormatsSection;
    public Button ExecuteActionButton => ExecuteButton;
}
