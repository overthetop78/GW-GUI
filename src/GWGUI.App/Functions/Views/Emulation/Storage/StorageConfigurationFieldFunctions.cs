using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Contracts.Emulation.Settings;
using GWGUI.App.Functions.Views.Emulation.Settings;
using GWGUI.App.Localization.Extensions;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;

namespace GWGUI.App.Functions.Views.Emulation.Storage;

internal static partial class StorageConfigurationFieldFunctions
{
    internal static string Read(FrameworkElement control) => EmulationSettingsFieldControlFunctions.Read(control) ?? string.Empty;

    internal static FrameworkElement Create(EmulationSettingsField field, string? moduleId, IReadOnlyDictionary<string, string>? values)
    {
        field = field with { Value = values?.GetValueOrDefault(field.Id) ?? field.Value };
        var choices = (field.Choices ?? []).Select(choice => new EmulationSettingsChoiceView(choice, ChoiceLabel(choice, field, moduleId))).ToArray();
        var control = field.Editor == EmulationSettingsEditor.Toggle ? EmulationSettingsFieldControlFunctions.CreateToggle(field)
            : field.NumericRange is not null && field.Choices?.Count > 0 && field.AutomaticValue is null ? CreatePresets(field, moduleId)
            : field.NumericRange is not null ? EmulationSettingsFieldControlFunctions.CreateSlider(field, choice => ChoiceLabel(choice, field, moduleId))
            : EmulationSettingsFieldControlFunctions.CreateSelection(field, choices);
        control.IsEnabled = field.IsEnabled;
        return control;
    }

    private static string ChoiceLabel(EmulationSettingsChoice choice, EmulationSettingsField field, string? moduleId) =>
        EmulationSettingsValuePresentationFunctions.WithUnit(choice.InvariantDisplayValue ?? LocExtension.GetForModule(moduleId ?? string.Empty, choice.DisplayResourceKey), field.ValueUnitResourceKey);

    internal static FrameworkElement Present(EmulationSettingsField field, FrameworkElement control, string? moduleId) =>
        EmulationSettingsLayout.VideoSettingsChoice(new GWGUI.App.Contracts.Views.Emulation.Settings.EmulationVideoSettingsField(
            LocExtension.GetForModule(moduleId ?? string.Empty, field.LabelResourceKey), control,
            IsTrailingCheckBox: control is CheckBox,
            Explanation: field.ExplanationResourceKey is null ? null : LocExtension.GetForModule(moduleId ?? string.Empty, field.ExplanationResourceKey),
            DetailedExplanation: field.DetailedExplanationResourceKey is null ? null : LocExtension.GetForModule(moduleId ?? string.Empty, field.DetailedExplanationResourceKey)));
}
