using GWGUI.App.Controls;
using GWGUI.Domain.Settings;

namespace GWGUI.App.Options;

internal sealed class EngineOptionsController
{
    private readonly OptionsEnginesSection _section;
    private readonly Func<bool> _isInitializing;
    private readonly Func<Task> _persistSettings;

    public EngineOptionsController(
        OptionsEnginesSection section,
        EngineSettings settings,
        Func<bool> isInitializing,
        Func<Task> persistSettings)
    {
        _section = section;
        _isInitializing = isInitializing;
        _persistSettings = persistSettings;
        section.PhysicalRead.SelectedIndex = Index(settings.PhysicalRead);
        section.PhysicalWrite.SelectedIndex = Index(settings.PhysicalWrite);
        section.Conversion.SelectedIndex = Index(settings.Conversion);
        section.ExplorerRead.SelectedIndex = Index(settings.ExplorerRead);
        section.EngineChanged += EngineChanged;
    }

    public void ApplyTo(AppSettings settings)
    {
        settings.Engines.PhysicalRead = Engine(_section.PhysicalRead.SelectedIndex);
        settings.Engines.PhysicalWrite = Engine(_section.PhysicalWrite.SelectedIndex);
        settings.Engines.Conversion = Engine(_section.Conversion.SelectedIndex);
        settings.Engines.ExplorerRead = Engine(_section.ExplorerRead.SelectedIndex);
    }

    private async void EngineChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (!_isInitializing()) await _persistSettings();
    }

    private static int Index(OperationEngine engine) => engine == OperationEngine.Internal ? 0 : 1;

    private static OperationEngine Engine(int index) =>
        index == 0 ? OperationEngine.Internal : OperationEngine.GreaseweazleHostTools;
}
