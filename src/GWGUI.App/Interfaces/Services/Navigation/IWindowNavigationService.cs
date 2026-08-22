using GWGUI.Domain.Settings;
using GWGUI.App.Contracts.Services.Navigation;
using GWGUI.App.Enums.Services.Navigation;

namespace GWGUI.App.Interfaces.Services.Navigation;

public interface IWindowNavigationService
{
    bool ShowOptions(AppSettings settings, OptionsSection section = OptionsSection.General);
    void ShowLogHistory(string logsDirectory);
    void ShowAbout();
    void ShowGwTool(GwToolWindowRequest request);
}
