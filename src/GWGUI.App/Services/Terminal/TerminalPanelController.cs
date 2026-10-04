using GWGUI.Infrastructure.Settings;
using GWGUI.Infrastructure.Settings.Window;
using GWGUI.App.Views.Controls.Shell;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;



namespace GWGUI.App.Services.Terminal;

public sealed class TerminalPanelController
{
    private readonly TerminalSection terminal;
    private readonly RowDefinition terminalRow;
    private readonly GridSplitter splitter;
    private readonly Func<AppSettings> settings;
    private int selectedTab;

    public TerminalPanelController(
        TerminalSection terminal,
        RowDefinition terminalRow,
        GridSplitter splitter,
        Func<AppSettings> settings)
    {
        this.terminal = terminal;
        this.terminalRow = terminalRow;
        this.splitter = splitter;
        this.settings = settings;
    }

    public bool IsVisible => terminal.Visibility == Visibility.Visible;
    public double ActualHeight => terminalRow.ActualHeight;

    public string GetCompleteText() =>
        terminal.CommandTextBox.Text + Environment.NewLine + Environment.NewLine + terminal.OutputText;

    public void CopyToClipboard()
    {
        Clipboard.SetText(GetCompleteText());
    }

    public Task ExportAsync(string path) => File.WriteAllTextAsync(path, GetCompleteText());

    public void AppendError(string entry) => AppendEntry(entry, Brushes.IndianRed);

    public void AppendWarning(string entry) => AppendEntry(entry, Brushes.DarkOrange);

    private void AppendEntry(string entry, Brush foreground)
    {
        if (!string.IsNullOrEmpty(terminal.OutputText)
            && !terminal.OutputText.EndsWith(Environment.NewLine, StringComparison.Ordinal))
            terminal.OutputTextBox.AppendText(Environment.NewLine);
        if (terminal.OutputTextBox.Document.Blocks.LastBlock is not Paragraph paragraph)
        {
            paragraph = new Paragraph { Margin = new Thickness(0) };
            terminal.OutputTextBox.Document.Blocks.Add(paragraph);
        }
        paragraph.Inlines.Add(new Run(entry) { Foreground = foreground });
        terminal.OutputTextBox.ScrollToEnd();
        SetVisibility(true);
    }

    public void Toggle() => SetVisibility(!IsVisible);

    public void SetVisibility(bool visible)
    {
        var state = CurrentState();
        var displayedHeight = DisplayedHeight();
        if (IsVisible && displayedHeight >= 100)
            state.Height = displayedHeight;
        state.Expanded = visible;

        terminal.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        splitter.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        terminalRow.Height = visible
            ? new GridLength(Math.Max(100, state.Height))
            : new GridLength(0);
    }

    public void SelectTab(int tabIndex)
    {
        if (tabIndex < 0 || tabIndex == selectedTab) return;
        CaptureCurrent();
        selectedTab = tabIndex;
        var state = CurrentState();
        terminal.Visibility = state.Expanded ? Visibility.Visible : Visibility.Collapsed;
        splitter.Visibility = state.Expanded ? Visibility.Visible : Visibility.Collapsed;
        terminalRow.Height = state.Expanded
            ? new GridLength(Math.Max(100, state.Height))
            : new GridLength(0);
    }

    public void CaptureCurrent()
    {
        var state = CurrentState();
        state.Expanded = IsVisible;
        var displayedHeight = DisplayedHeight();
        if (IsVisible && displayedHeight >= 100)
            state.Height = displayedHeight;
    }

    private ConsolePanelSettings CurrentState()
    {
        var currentSettings = settings();
        currentSettings.ConsolePanels ??= [];
        if (currentSettings.ConsolePanels.TryGetValue(selectedTab, out var state)) return state;
        state = new ConsolePanelSettings
        {
            Expanded = currentSettings.ConsoleExpanded,
            Height = currentSettings.ConsoleHeight
        };
        currentSettings.ConsolePanels[selectedTab] = state;
        return state;
    }

    private double DisplayedHeight() => terminalRow.ActualHeight >= 100
        ? terminalRow.ActualHeight
        : terminalRow.Height.IsAbsolute ? terminalRow.Height.Value : 0;
}
