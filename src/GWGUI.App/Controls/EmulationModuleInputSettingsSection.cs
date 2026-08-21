using System.Windows;
using GWGUI.App.Localization;
using GWGUI.Emulation;

namespace GWGUI.App.Controls;

internal sealed partial class EmulationModuleSettingsSection
{
    private UIElement BuildInputSettingsTab(EmulationMachineSettings settings, EmulationMachineTab tab)
    {
        if (_inputSettings is null) return BuildGenericSettingsTab(settings, tab);
        var fields = settings.Blocks.Where(block => block.Tab == tab && block.IsVisible)
            .SelectMany(block => block.Fields).Where(field => field.IsVisible)
            .Select(field => new EmulationSettingsControlField(
                LocExtension.Get(field.LabelResourceKey), CreateField(field))).ToArray();
        var content = _inputSettings.CreateContent(tab, _configuration, fields);
        ApplySettingsRules(settings);
        return content;
    }
}
