using System.Windows;
using GWGUI.App.Functions.Views.Emulation.Settings;

namespace GWGUI.App.Views.Controls.Emulation.Options;

internal sealed partial class EmulationModuleSettingsSection
{
    private FrameworkElement CreateSlider(EmulationSettingsField field) =>
        EmulationSettingsFieldControlFunctions.CreateSlider(field, choice => ChoiceView(choice, field.ValueUnitResourceKey).DisplayName, ExecuteUserChangeAsync);
}
