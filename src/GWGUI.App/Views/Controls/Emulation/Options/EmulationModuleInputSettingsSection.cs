using GWGUI.App.Contracts.Emulation.Settings;
using GWGUI.App.Localization.Extensions;
using System.Windows;
using GWGUI.Emulation;

namespace GWGUI.App.Views.Controls.Emulation.Options;

internal sealed partial class EmulationModuleSettingsSection
{
    private UIElement BuildInputSettingsTab(EmulationMachineSettings settings, EmulationMachineTab tab)
    {
        if (_inputSettings is null) return BuildGenericSettingsTab(settings, tab);
        var fields = settings.Blocks.Where(block => block.Tab == tab && block.IsVisible)
            .SelectMany(block => block.Fields).Where(field => field.IsVisible)
            .Select(CreateControlField).ToArray();
        var content = _inputSettings.CreateContent(tab, _configuration, fields);
        ApplySettingsRules(settings);
        return content;
    }
}
