using GWGUI.App.Constants.Localization;
using GWGUI.App.Functions.Views.Emulation.Storage;
using GWGUI.App.Localization.Extensions;
using GWGUI.Emulation.Contracts;
using System.Windows;
using System.Windows.Controls;

namespace GWGUI.App.Views.Dialogs.Emulation.Storage;

internal sealed class ManagedStorageConfigurationDialog : Window
{
    private readonly Dictionary<string, FrameworkElement> _controls = new(StringComparer.Ordinal);
    internal IReadOnlyDictionary<string, string> Options => _controls.ToDictionary(
        item => item.Key, item => StorageConfigurationFieldFunctions.Read(item.Value), StringComparer.Ordinal);

    internal ManagedStorageConfigurationDialog(string identifier, string model, string moduleId,
        IReadOnlyList<EmulationSettingsField> fields, IReadOnlyDictionary<string, string>? values,
        string? explanation = null)
    {
        Title = LocExtension.Get(EmulationResourceKeys.StorageDeviceConfigure) + " " + identifier;
        Owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive);
        ShowInTaskbar = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Width = 720;
        MinWidth = 560;
        MaxHeight = SystemParameters.WorkArea.Height * 0.85;
        SizeToContent = SizeToContent.Height;
        var body = new StackPanel();
        if (!string.IsNullOrWhiteSpace(explanation)) body.Children.Add(StorageDialogUi.Info(explanation));
        foreach (var field in fields)
        {
            var control = StorageConfigurationFieldFunctions.Create(field, moduleId, values);
            _controls[field.Id] = control;
            var presentation = StorageConfigurationFieldFunctions.Present(field, control, moduleId);
            presentation.Margin = new Thickness(0, 10, 0, 10);
            body.Children.Add(presentation);
        }
        Content = StorageDialogUi.DialogLayout(
            StorageDialogUi.DialogHeader("", Title, model),
            StorageDialogUi.Card(model, body),
            StorageDialogUi.Footer(this, LocExtension.Get(EmulationResourceKeys.CommonSave)));
    }
}
