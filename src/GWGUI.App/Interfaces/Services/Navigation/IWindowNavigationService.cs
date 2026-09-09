using GWGUI.Domain.Settings;
using GWGUI.App.Contracts.Services.Navigation;
using GWGUI.App.Enums.Services.Navigation;

namespace GWGUI.App.Interfaces.Services.Navigation;

public interface IWindowNavigationService
{
    bool ShowPreferences(AppSettings settings, PreferencesSection section = PreferencesSection.General);
    void ShowEmulationPreferences(AppSettings settings);
    void ShowEmulationModuleOptions(AppSettings settings, string moduleId);
    void ShowUpdates();
    void ShowLogHistory(string logsDirectory);
    void ShowAbout();
    void ShowGwTool(GwToolWindowRequest request);
}
