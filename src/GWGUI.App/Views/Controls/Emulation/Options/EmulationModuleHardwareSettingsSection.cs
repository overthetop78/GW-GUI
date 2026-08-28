using GWGUI.App.Contracts.Emulation.Machine;
using GWGUI.App.Contracts.Emulation.Memory;
using GWGUI.App.Contracts.Emulation.Settings;
using GWGUI.App.Contracts.Views.Emulation.Settings;
using GWGUI.App.Constants.Emulation;
using GWGUI.App.Functions.Views.Emulation.Settings;
using GWGUI.App.Localization.Extensions;
using System.Windows;
using System.Windows.Controls;
using GWGUI.Emulation;


namespace GWGUI.App.Views.Controls.Emulation.Options;

internal sealed partial class EmulationModuleSettingsSection
{
    private bool _applyingSettingsRule;

    private UIElement BuildCpuSettingsTab(EmulationMachineSettings settings)
    {
        var fields = VisibleFields(settings, EmulationMachineTab.Cpu);
        var cpu = FieldByLabel(fields, EmulationHardwareSettingsConstants.CpuModelResourceKey);
        if (cpu is null) return BuildGenericSettingsTab(settings, EmulationMachineTab.Cpu);
        var speed = FieldByLabel(fields, "Emulation.Cpu.Speed");
        var originalSpeedField = FieldByLabel(fields, "Emulation.Cpu.SpeedOriginal");
        var precision = FieldByLabel(fields, "Emulation.Cpu.Precision");
        var fpu = FieldByLabel(fields, "Emulation.Fpu.Model");
        var cpuControl = CreateField(cpu);
        var speedControl = speed is null ? null : CreateField(speed);
        var machineName = (_machines.SelectedItem as EmulationMachineChoice)?.DisplayName ?? settings.MachineId;
        var cpuName = EmulationSettingsValuePresentationFunctions.DisplayValue(cpu);
        var originalSpeed = originalSpeedField is null ? string.Empty
            : EmulationSettingsValuePresentationFunctions.DisplayValue(originalSpeedField);
        var page = EmulationSettingsLayout.CpuSettingsPage(new EmulationCpuSettingsContent(
            cpuControl,
            new TextBlock { Text = string.Join(" · ", new[] { machineName, cpuName, originalSpeed }
                .Where(value => !string.IsNullOrWhiteSpace(value))) },
            precision is null ? null : CreateField(precision),
            fpu is null ? null : CreateField(fpu),
            new TextBlock { Text = originalSpeed },
            speed is { IsEnabled: true } ? speedControl : null));
        ApplySettingsRules(settings);
        return EmulationSettingsLayout.ScrollPage(page);
    }

    private UIElement BuildMemorySettingsTab(EmulationMachineSettings settings)
    {
        var memoryBlocks = settings.Blocks.Where(block => block.Tab == EmulationMachineTab.Ram && block.IsVisible)
            .ToArray();
        var mainFields = memoryBlocks.Where(block => block.TitleResourceKey == "Emulation.Memory.Main")
            .SelectMany(block => block.Fields).Where(field => field.IsVisible).ToArray();
        if (mainFields.Length == 0) return BuildGenericSettingsTab(settings, EmulationMachineTab.Ram);
        var extensionFields = memoryBlocks.Where(block => block.TitleResourceKey != "Emulation.Memory.Main")
            .SelectMany(block => block.Fields).Where(field => field.IsVisible).ToArray();
        var mainControls = mainFields.Select(field =>
            new EmulationSettingsControlField(LocExtension.Get(field.LabelResourceKey), CreateField(field))).ToArray();
        var extensionControls = extensionFields.Select(field =>
            new EmulationSettingsControlField(LocExtension.Get(field.LabelResourceKey), CreateField(field))).ToArray();
        var fields = mainFields.Concat(extensionFields).ToArray();
        var machineName = (_machines.SelectedItem as EmulationMachineChoice)?.DisplayName ?? settings.MachineId;
        var total = new TextBlock();
        foreach (var control in mainControls.Concat(extensionControls).Select(field => field.Control))
            if (control is ComboBox selection)
                selection.SelectionChanged += (_, _) => UpdateMemoryTotal(fields, total);
            else if (control is CheckBox toggle)
            {
                toggle.Checked += (_, _) => UpdateMemoryTotal(fields, total);
                toggle.Unchecked += (_, _) => UpdateMemoryTotal(fields, total);
            }
        UpdateMemoryTotal(fields, total);
        var page = EmulationSettingsLayout.MemorySettingsPage(new EmulationMemorySettingsContent(
            mainControls,
            new TextBlock { Text = LocExtension.Get("Emulation.Memory.CompatibleWithModel", machineName) },
            extensionControls,
            new TextBlock { Text = LocExtension.Get("Emulation.Memory.ExtensionsCompatibleWithModel", machineName) },
            total));
        ApplySettingsRules(settings);
        return EmulationSettingsLayout.ScrollPage(page);
    }

    private UIElement BuildGenericSettingsTab(EmulationMachineSettings settings, EmulationMachineTab tab)
    {
        var panel = new StackPanel { Margin = new Thickness(12) };
        AddBlocks(panel, settings, tab);
        ApplySettingsRules(settings);
        return EmulationSettingsLayout.ScrollPage(panel);
    }

    private void ApplySettingsRules(EmulationMachineSettings settings)
    {
        foreach (var rule in settings.Rules ?? [])
        {
            if (!_fieldControls.TryGetValue(rule.SourceFieldId, out var source)
                || !_fieldControls.TryGetValue(rule.TargetFieldId, out var target)) continue;
            AttachValueChanged(source, () => ApplySettingsRule(rule, source, target, true));
            AttachValueChanged(target, () => ApplySettingsRule(rule, source, target, false));
            ApplySettingsRule(rule, source, target, false);
        }
    }

    private void ApplySettingsRule(EmulationSettingsRule rule, FrameworkElement source,
        FrameworkElement target, bool sourceChanged)
    {
        var sourceValue = ReadValue(source);
        var targetValue = ReadValue(target);
        if (rule.Category == EmulationSettingsRuleCategory.MutuallyExclusive
            && sourceValue != rule.ComparedValue && targetValue != rule.ComparedValue)
        {
            var wasApplyingRule = _applyingSettingsRule;
            _applyingSettingsRule = true;
            try { SelectValue(sourceChanged ? target : source, rule.ComparedValue); }
            finally { _applyingSettingsRule = wasApplyingRule; }
        }
        if (rule.Category == EmulationSettingsRuleCategory.VisibleWhenSourceDiffers)
            target.Visibility = sourceValue == rule.ComparedValue
                ? Visibility.Collapsed : Visibility.Visible;
    }

    private void AttachUserChangeHandlers()
    {
        foreach (var (control, action) in _userChangeHandlers.ToArray())
        {
            AttachUserValueChanged(control, action);
            _userChangeHandlers.Remove(control);
        }
    }

    private void AttachUserValueChanged(FrameworkElement control, Func<Task> action)
    {
        async void Changed(object? sender, RoutedEventArgs args)
        {
            if (!_applyingSettingsRule) await action();
        }
        if (control is ComboBox selection) selection.SelectionChanged += Changed;
        else if (control is CheckBox toggle)
        {
            toggle.Checked += Changed;
            toggle.Unchecked += Changed;
        }
    }

    private static void AttachValueChanged(FrameworkElement control, Action action)
    {
        if (control is ComboBox selection) selection.SelectionChanged += (_, _) => action();
        else if (control is CheckBox toggle)
        {
            toggle.Checked += (_, _) => action();
            toggle.Unchecked += (_, _) => action();
        }
    }

    private static void SelectValue(FrameworkElement control, string value)
    {
        if (control is ComboBox selection)
            selection.SelectedItem = selection.Items.Cast<EmulationSettingsChoiceView>()
                .FirstOrDefault(item => item.Choice.Id == value);
        else if (control is CheckBox toggle && toggle.Tag is EmulationSettingsField field)
            toggle.IsChecked = value == field.EnabledValue;
    }

    private static IReadOnlyList<EmulationSettingsField> VisibleFields(EmulationMachineSettings settings,
        EmulationMachineTab tab) => settings.Blocks.Where(block => block.Tab == tab && block.IsVisible)
        .SelectMany(block => block.Fields).Where(field => field.IsVisible).ToArray();

    private static EmulationSettingsField? FieldByLabel(IEnumerable<EmulationSettingsField> fields,
        string resourceKey) => fields.FirstOrDefault(field => field.LabelResourceKey == resourceKey);

    private long SelectedNumericValue(EmulationSettingsField field)
    {
        if (_fieldControls.TryGetValue(field.Id, out var control)
            && control is ComboBox { SelectedItem: EmulationSettingsChoiceView selected })
            return selected.Choice.NumericValue ?? 0;
        return EmulationSettingsValuePresentationFunctions.DefaultNumericValue(field);
    }

    private void UpdateMemoryTotal(IEnumerable<EmulationSettingsField> fields, TextBlock total)
    {
        var bytes = fields.Sum(SelectedNumericValue);
        var formatted = EmulationSettingsValuePresentationFunctions.FormatMemorySize(bytes);
        total.Text = LocExtension.Get("Emulation.Memory.TotalConfigured", formatted.Value, formatted.Unit);
    }

}
