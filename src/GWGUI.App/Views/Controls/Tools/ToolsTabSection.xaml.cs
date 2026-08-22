using GWGUI.App.Constants.Emulation;
using System.Windows;
using System.Windows.Controls;


namespace GWGUI.App.Views.Controls.Tools;

public partial class ToolsTabSection : UserControl
{
    public ToolsTabSection()
    {
        InitializeComponent();
        EraseTracksValueInput.Text = EmulationControlDefaults.EraseTrackRange;
        EraseRevsValueInput.Text = EmulationControlDefaults.EraseRevolutions.ToString();
        CleanCylindersValueInput.Text = EmulationControlDefaults.CleanCylinders.ToString();
        CleanPassesValueInput.Text = EmulationControlDefaults.CleanPasses.ToString();
        CleanLingerValueInput.Text = EmulationControlDefaults.CleanLingerMilliseconds.ToString();
        EraseButton.Click += (_, e) => EraseRequested?.Invoke(this, e);
        CleanButton.Click += (_, e) => CleanRequested?.Invoke(this, e);
    }

    public event SelectionChangedEventHandler? ToolSelectionChanged;
    public event RoutedEventHandler? InputChanged;
    public event RoutedEventHandler? EraseRequested;
    public event RoutedEventHandler? CleanRequested;

    public ListBox ToolsList => ToolSelector;
    public Border ErasePanel => EraseSection;
    public CheckBox EraseTracksEnabled => EraseTracksEnabledInput;
    public TextBox EraseTracksValue => EraseTracksValueInput;
    public CheckBox EraseRevsEnabled => EraseRevsEnabledInput;
    public TextBox EraseRevsValue => EraseRevsValueInput;
    public TextBox EraseExpertArguments => EraseExpertArgumentsInput;
    public Button EraseExecuteButton => EraseButton;
    public Border CleanPanel => CleanSection;
    public CheckBox CleanCylindersEnabled => CleanCylindersEnabledInput;
    public TextBox CleanCylindersValue => CleanCylindersValueInput;
    public CheckBox CleanPassesEnabled => CleanPassesEnabledInput;
    public TextBox CleanPassesValue => CleanPassesValueInput;
    public CheckBox CleanLingerEnabled => CleanLingerEnabledInput;
    public TextBox CleanLingerValue => CleanLingerValueInput;
    public TextBox CleanExpertArguments => CleanExpertArgumentsInput;
    public Button CleanExecuteButton => CleanButton;

    private void ToolSelector_SelectionChanged(object sender, SelectionChangedEventArgs e) => ToolSelectionChanged?.Invoke(sender, e);
    private void Input_Changed(object sender, RoutedEventArgs e) => InputChanged?.Invoke(sender, e);
}
