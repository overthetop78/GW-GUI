using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Localization;
using GWGUI.Emulation;

namespace GWGUI.App.Controls;

internal sealed partial class EmulationModuleSettingsSection
{
    private UIElement BuildCpuSettingsTab(EmulationMachineSettings settings)
    {
        var fields = VisibleFields(settings, EmulationMachineTab.Cpu);
        var cpu = FieldByLabel(fields, "Emulation.Cpu.Model");
        if (cpu is null) return BuildGenericSettingsTab(settings, EmulationMachineTab.Cpu);
        var speed = FieldByLabel(fields, "Emulation.Cpu.Speed");
        var precision = FieldByLabel(fields, "Emulation.Cpu.Precision");
        var fpu = FieldByLabel(fields, "Emulation.Fpu.Model");
        var cpuControl = CreateField(cpu);
        var speedControl = speed is null ? null : CreateField(speed);
        var machineName = (_machines.SelectedItem as EmulationMachineChoice)?.DisplayName ?? settings.MachineId;
        var cpuName = DisplayValue(cpu);
        var originalSpeed = speed is null ? string.Empty : DisplayValue(speed);
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
        var fields = VisibleFields(settings, EmulationMachineTab.Ram);
        var main = FieldByLabel(fields, "Emulation.Memory.Main");
        if (main is null) return BuildGenericSettingsTab(settings, EmulationMachineTab.Ram);
        var mainControl = CreateField(main);
        var extensions = fields.Where(field => !ReferenceEquals(field, main)).ToArray();
        var extensionControls = extensions.Select(field =>
            new EmulationSettingsControlField(LocExtension.Get(field.LabelResourceKey), CreateField(field))).ToArray();
        var machineName = (_machines.SelectedItem as EmulationMachineChoice)?.DisplayName ?? settings.MachineId;
        var total = new TextBlock();
        foreach (var control in new[] { mainControl }.Concat(extensionControls.Select(field => field.Control)))
            if (control is ComboBox selection)
                selection.SelectionChanged += (_, _) => UpdateMemoryTotal(fields, total);
            else if (control is CheckBox toggle)
            {
                toggle.Checked += (_, _) => UpdateMemoryTotal(fields, total);
                toggle.Unchecked += (_, _) => UpdateMemoryTotal(fields, total);
            }
        UpdateMemoryTotal(fields, total);
        var page = EmulationSettingsLayout.MemorySettingsPage(new EmulationMemorySettingsContent(
            [new(LocExtension.Get(main.LabelResourceKey), mainControl)],
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

    private static void ApplySettingsRule(EmulationSettingsRule rule, FrameworkElement source,
        FrameworkElement target, bool sourceChanged)
    {
        var sourceValue = ReadValue(source);
        var targetValue = ReadValue(target);
        if (rule.Category == EmulationSettingsRuleCategory.MutuallyExclusive
            && sourceValue != rule.ComparedValue && targetValue != rule.ComparedValue)
            SelectValue(sourceChanged ? target : source, rule.ComparedValue);
        if (rule.Category == EmulationSettingsRuleCategory.VisibleWhenSourceDiffers)
            target.Visibility = sourceValue == rule.ComparedValue
                ? Visibility.Collapsed : Visibility.Visible;
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

    private static string DisplayValue(EmulationSettingsField field)
    {
        var choice = field.Choices?.FirstOrDefault(value => value.Id == field.Value)
            ?? field.Choices?.FirstOrDefault();
        return choice?.InvariantDisplayValue ?? (choice is null ? field.Value ?? string.Empty
            : LocExtension.Get(choice.DisplayResourceKey));
    }

    private static long DefaultNumericValue(EmulationSettingsField field)
    {
        if (field.Editor == EmulationSettingsEditor.Information
            && field.NumericValue is { } information) return information;
        return field.Choices?.FirstOrDefault(choice => choice.Id == field.Value)?.NumericValue ?? 0;
    }

    private long SelectedNumericValue(EmulationSettingsField field)
    {
        if (_fieldControls.TryGetValue(field.Id, out var control)
            && control is ComboBox { SelectedItem: EmulationSettingsChoiceView selected })
            return selected.Choice.NumericValue ?? 0;
        return DefaultNumericValue(field);
    }

    private void UpdateMemoryTotal(IEnumerable<EmulationSettingsField> fields, TextBlock total)
    {
        var bytes = fields.Sum(SelectedNumericValue);
        var formatted = FormatMemorySize(bytes);
        total.Text = LocExtension.Get("Emulation.Memory.TotalConfigured", formatted.Value, formatted.Unit);
    }

    private static (string Value, string Unit) FormatMemorySize(long bytes) => bytes >= 1024 * 1024
        ? ($"{bytes / (1024d * 1024d):0.##}", "MiB")
        : bytes >= 1024 ? ($"{bytes / 1024d:0.##}", "KiB") : (bytes.ToString(), "B");
}
