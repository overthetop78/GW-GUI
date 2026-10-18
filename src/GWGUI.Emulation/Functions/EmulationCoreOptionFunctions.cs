using System.Runtime.InteropServices;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Interop;
using GWGUI.Emulation.Enums;

namespace GWGUI.Emulation.Functions;

public static partial class EmulationCoreOptionFunctions
{
    internal static IReadOnlyList<TOption> WithDefaults<TOption>(IReadOnlyList<TOption> options,
        IReadOnlyDictionary<string, string> defaults, IReadOnlyDictionary<string, string> currentValues,
        Func<TOption, string> key, Func<TOption, string> originalDefault,
        Func<TOption, string, string, TOption> update) => options.Select(option =>
        {
            var defaultValue = defaults.GetValueOrDefault(key(option), originalDefault(option));
            return update(option, defaultValue, currentValues.GetValueOrDefault(key(option), defaultValue));
        }).ToArray();

    internal static IReadOnlyList<EmulationSettingsBlock> CreateSettingsBlocks<TOption>(IReadOnlyList<TOption> options,
        Func<TOption, bool> visible, Func<TOption, string?> category,
        Func<EmulationMachineTab, string> resourceFor,
        Func<TOption, EmulationMachineTab, string, EmulationSettingsField> field) =>
        options.Where(option => visible(option) && category(option) is not null)
            .GroupBy(option => Enum.Parse<EmulationMachineTab>(category(option)!))
            .Select(group =>
            {
                var resource = resourceFor(group.Key);
                return new EmulationSettingsBlock(resource + ".Options", group.Key, resource,
                    group.Select(option => field(option, group.Key, resource)).ToArray())
                    { IsCompact = group.Key == EmulationMachineTab.Audio };
            }).ToArray();


    internal static EmulationSettingsField CreateSettingsField<TValue>(string key, EmulationMachineTab tab,
        string block, Func<string> resource, IReadOnlyList<TValue> values,
        Func<TValue, EmulationSettingsChoice> choice, Func<TValue, bool> enabledChoice,
        Func<TValue, bool> disabledChoice, Func<TValue, string> nativeValue,
        Func<string> currentValue, Func<string?> explanation, Func<bool>? isEnabled = null)
        where TValue : class
    {
        var resourceKey = resource();
        var choices = values.Select(choice).ToArray();
        var enabledOption = values.FirstOrDefault(enabledChoice);
        var enabled = enabledOption is null ? null : nativeValue(enabledOption);
        var disabledOption = values.FirstOrDefault(disabledChoice);
        var disabled = disabledOption is null ? null : nativeValue(disabledOption);
        var toggle = values.Count == 2 && enabled is not null && disabled is not null;
        return CreatePreparedSettingsField(key, tab, block, resourceKey,
            toggle ? EmulationSettingsEditor.Toggle : EmulationSettingsEditor.Selection,
            choices, currentValue, explanation,
            () => enabled ?? EmulationSettingConstants.Enabled,
            () => disabled ?? EmulationSettingConstants.Disabled, isEnabled);
    }

    internal static EmulationSettingsField CreatePreparedSettingsField(string key, EmulationMachineTab tab,
        string block, string resourceKey, EmulationSettingsEditor editor, IReadOnlyList<EmulationSettingsChoice> choices,
        Func<string> currentValue, Func<string?> explanation, Func<string> enabledValue,
        Func<string> disabledValue, Func<bool>? isEnabled = null) =>
        new(key, tab, block, resourceKey, editor,
            currentValue(), choices,
            IsEnabled: isEnabled?.Invoke() ?? true,
            ExplanationResourceKey: explanation(),
            EnabledValue: enabledValue(),
            DisabledValue: disabledValue(),
            RequiresRestart: false);

    internal static EmulationSettingsField CreateSettingsFieldWithToggle<TValue>(string key, EmulationMachineTab tab,
        string block, Func<string> resource, IReadOnlyList<TValue> values, Func<TValue, EmulationSettingsChoice> choice,
        Func<bool> isToggle, Func<string> currentValue, Func<string?> explanation,
        Func<bool, string>? enabledValue = null, Func<bool, string>? disabledValue = null)
        where TValue : class
    {
        var resourceKey = resource();
        var choices = values.Select(choice).ToArray();
        var toggle = isToggle();
        return CreatePreparedSettingsField(key, tab, block, resourceKey,
            toggle ? EmulationSettingsEditor.Toggle : EmulationSettingsEditor.Selection, choices,
            currentValue, explanation,
            () => enabledValue is null ? EmulationSettingConstants.Enabled : enabledValue(toggle),
            () => disabledValue is null ? EmulationSettingConstants.Disabled : disabledValue(toggle));
    }


    internal const int CoreOptionPointerFieldsBeforeValues = 6;
    internal const int CoreOptionValueFieldCount = 2;
    internal const int CoreOptionTerminatorFieldCount = 1;
    internal const int CoreOptionDescriptionPointerIndex = 3;
    internal const int CoreOptionCategoryPointerIndex = 5;
    internal const int MaximumCoreOptionDefinitions = 1024;
    internal const int MaximumCoreOptionValues = 128;

    public static IReadOnlyList<EmulationOption> ReadLegacy(nint data, IDictionary<string, string> currentValues,
        IReadOnlyDictionary<string, bool> visibility)
    {
        var size = Marshal.SizeOf<ExternalCoreApi.Variable>();
        var catalog = new List<EmulationOption>();
        for (var current = data; current != 0; current += size)
        {
            var variable = Marshal.PtrToStructure<ExternalCoreApi.Variable>(current);
            if (variable.Key == 0) break;
            var key = Marshal.PtrToStringUTF8(variable.Key)!;
            var definition = Marshal.PtrToStringUTF8(variable.Value);
            var parts = definition?.Split(';', 2) ?? [];
            var name = parts.ElementAtOrDefault(0)?.Trim() ?? key;
            var values = parts.ElementAtOrDefault(1)?.Trim().Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(value => new EmulationOptionValue(value, value)).ToArray() ?? [];
            var defaultValue = values.FirstOrDefault()?.Value ?? string.Empty;
            if (!currentValues.ContainsKey(key) && defaultValue.Length > 0) currentValues[key] = defaultValue;
            catalog.Add(new EmulationOption(key, name, null, null, defaultValue, defaultValue, values,
                !visibility.TryGetValue(key, out var visible) || visible));
        }
        return catalog;
    }

    public static IReadOnlyList<EmulationOption>? ReadVersionTwo(nint options, IDictionary<string, string> currentValues,
        IReadOnlyDictionary<string, bool> visibility) => options == 0 ? null
            : ReadSimpleDefinitions(Marshal.ReadIntPtr(options, IntPtr.Size), true, currentValues, visibility);

    public static IReadOnlyList<EmulationOption>? ReadVersionOne(nint definitions, IDictionary<string, string> currentValues,
        IReadOnlyDictionary<string, bool> visibility) => definitions == 0 ? null
            : ReadSimpleDefinitions(definitions, false, currentValues, visibility);

    private static IReadOnlyList<EmulationOption> ReadSimpleDefinitions(nint definitions, bool versionTwo,
        IDictionary<string, string> currentValues, IReadOnlyDictionary<string, bool> visibility)
    {
        var prefix = versionTwo ? CoreOptionPointerFieldsBeforeValues : NativeOptionLayout.LegacyDefinitionPointerCount;
        var definitionSize = (prefix
            + MaximumCoreOptionValues * CoreOptionValueFieldCount
            + CoreOptionTerminatorFieldCount) * IntPtr.Size;
        var valuesOffset = prefix * IntPtr.Size;
        var defaultOffset = valuesOffset
            + MaximumCoreOptionValues * CoreOptionValueFieldCount * IntPtr.Size;
        var catalog = new List<EmulationOption>();

        for (var optionIndex = 0; optionIndex < MaximumCoreOptionDefinitions; optionIndex++)
        {
            var definition = definitions + optionIndex * definitionSize;
            var keyPointer = Marshal.ReadIntPtr(definition);
            if (keyPointer == 0) break;
            var key = Marshal.PtrToStringUTF8(keyPointer)!;
            var name = StringAt(definition, IntPtr.Size) ?? key;
            var description = StringAt(
                definition, (versionTwo ? CoreOptionDescriptionPointerIndex : NativeOptionLayout.DescriptionPointerIndex) * IntPtr.Size);
            var category = versionTwo ? StringAt(definition, CoreOptionCategoryPointerIndex * IntPtr.Size) : null;
            var defaultValue = StringAt(definition, defaultOffset) ?? string.Empty;
            var values = new List<EmulationOptionValue>();
            for (var valueIndex = 0; valueIndex < MaximumCoreOptionValues; valueIndex++)
            {
                var valueOffset = valuesOffset
                    + valueIndex * CoreOptionValueFieldCount * IntPtr.Size;
                var value = StringAt(definition, valueOffset);
                if (value is null) break;
                values.Add(new EmulationOptionValue(value, StringAt(definition, valueOffset + IntPtr.Size) ?? value));
            }
            catalog.Add(new EmulationOption(key, name, description, category, defaultValue, defaultValue, values,
                !visibility.TryGetValue(key, out var visible) || visible));
            if (!currentValues.ContainsKey(key) && defaultValue.Length > 0) currentValues[key] = defaultValue;
        }
        return catalog;
    }

    private static string? StringAt(nint structure, int offset)
    {
        var pointer = Marshal.ReadIntPtr(structure, offset);
        return pointer == 0 ? null : Marshal.PtrToStringUTF8(pointer);
    }
}
